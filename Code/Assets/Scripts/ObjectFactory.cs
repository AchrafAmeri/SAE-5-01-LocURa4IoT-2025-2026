using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.XR.Interaction.Toolkit;
// Note : Selon ta version d'Unity, l'espace de nom peut varier.
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using GLTFast;

// --- 1. AJOUT : Structure pour la liste manuelle (Bibliothèque) ---
[Serializable]
public struct CatalogItem
{
    public string key;        // Le nom dans le JSON (ex: "M_Table")
    public GameObject prefab; // L'objet à glisser depuis tes dossiers
}

[Serializable]
public class FurniturePayload
{
    public string objectType;
    public string id;
    public Vector3 position;
    public Vector3 rotation;
    public Vector3 scale = Vector3.one;
    public string shape;
    public string color;
    public bool grabbable = false;
    public string prefab;
    public long timestamp = 0;
    public string url;
}

[Serializable]
public class SceneObjectMeta : MonoBehaviour
{
    public string id;
    public string topic;
    public long lastTimestamp;
    public string sourceUrl;

    // --- 2. AJOUT : Mémoire pour ne pas perdre les infos au retour ---
    public string prefab;
    public string shape;
    public string color;
    // ---------------------------------------------------------------

    public bool isGrabbed = false;
    public RigidbodyConstraints prevConstraints = RigidbodyConstraints.None;
}

public class ObjectFactory : MonoBehaviour
{
    // --- 3. AJOUT : La liste qui apparaît dans l'inspecteur ---
    [Header("Ma Bibliothèque Locale (Drag & Drop ici)")]
    public List<CatalogItem> localCatalog = new List<CatalogItem>();

    [Header("Prefabs / parents")]
    [SerializeField] private GameObject defaultPrimitiveParent;
    [SerializeField] private Material defaultMaterial;

    [Header("Optional Meshes")]
    [SerializeField] private Mesh cubeMesh;
    [SerializeField] private Mesh sphereMesh;
    [SerializeField] private Mesh cylinderMesh;

    [Header("MQTT (optionnel)")]
    [SerializeField] private MQTT mqttManager;

    [Header("Publication en drag")]
    [SerializeField] private int publishRateHz = 10;
    [SerializeField] private float publishMoveThreshold = 0.001f;

    [Header("Placement")]
    [SerializeField] private float minAllowedY = 0f;

    private Dictionary<Color, Material> _materialCache = new Dictionary<Color, Material>();
    private HashSet<string> _creationInProgress = new HashSet<string>();

    public async Task CreateOrUpdateFromPayload(string topic, string json)
    {
        if (string.IsNullOrWhiteSpace(json)) return;

        FurniturePayload payload;
        try { payload = JsonUtility.FromJson<FurniturePayload>(json); }
        catch (Exception ex) { Debug.LogError($"JSON Error: {ex.Message}"); return; }

        if (payload == null) return;

        string id = string.IsNullOrWhiteSpace(payload.id) ? ExtractIdFromTopic(topic) : payload.id;
        if (string.IsNullOrWhiteSpace(id)) return;

        if (_creationInProgress.Contains(id)) return;
        _creationInProgress.Add(id);

        try
        {
            Transform searchParent = defaultPrimitiveParent != null ? defaultPrimitiveParent.transform : this.transform;
            Transform existing = searchParent.Find(id);

            if (existing == null)
            {
                var globalGo = GameObject.Find(id);
                if (globalGo != null) existing = globalGo.transform;
            }

            if (existing == null)
            {
#if UNITY_2023_1_OR_NEWER
                var metas = UnityEngine.Object.FindObjectsByType<SceneObjectMeta>(FindObjectsSortMode.None);
#else
                var metas = FindObjectsOfType<SceneObjectMeta>();
#endif
                foreach (var meta in metas)
                {
                    if (meta != null && meta.id == id) { existing = meta.transform; break; }
                }
            }

            GameObject go;

            if (existing != null)
            {
                go = existing.gameObject;
            }
            else
            {
                // 1. Essai URL
                if (!string.IsNullOrWhiteSpace(payload.url))
                {
                    go = await CreateFromUrlAsync(payload.url, searchParent);
                }
                else
                {
                    go = null;
                }

                // 2. Si pas d'URL, on cherche un PREFAB
                if (go == null)
                {
                    string resolvedPrefab = payload.prefab;

                    if (string.IsNullOrWhiteSpace(resolvedPrefab))
                    {
                        string t = (payload.objectType ?? "").ToLower();
                        if (t.Contains("table")) resolvedPrefab = "Table";
                        else if (t.Contains("chair") || t.Contains("chaise")) resolvedPrefab = "Chair";
                    }

                    if (!string.IsNullOrWhiteSpace(resolvedPrefab))
                    {
                        GameObject prefabToLoad = null;

                        // --- 4. MODIF : On cherche d'abord dans TA liste manuelle ---
                        if (localCatalog != null)
                        {
                            foreach (var item in localCatalog)
                            {
                                if (item.key.Equals(resolvedPrefab, StringComparison.OrdinalIgnoreCase))
                                {
                                    prefabToLoad = item.prefab;
                                    break;
                                }
                            }
                        }

                        // Ensuite dans Resources
                        if (prefabToLoad == null)
                        {
                            prefabToLoad = Resources.Load<GameObject>(resolvedPrefab);
                        }

                        if (prefabToLoad != null)
                        {
                            go = Instantiate(prefabToLoad, searchParent);
                        }
                        else
                        {
                            // Fallback primitifs
                            go = CreatePrimitiveFromShape(payload.shape);
                            ApplyMaterial(go, defaultMaterial);
                            go.transform.SetParent(searchParent);
                        }
                    }
                    else
                    {
                        go = CreatePrimitiveFromShape(payload.shape);
                        ApplyMaterial(go, defaultMaterial);
                        go.transform.SetParent(searchParent);
                    }
                }

                go.name = id;
                var meta = go.AddComponent<SceneObjectMeta>();
                meta.id = id;
                meta.topic = topic;
                meta.sourceUrl = payload.url;

                // --- 5. MODIF : On sauvegarde les infos dans la mémoire de l'objet ---
                meta.prefab = payload.prefab;
                meta.shape = payload.shape;
                meta.color = payload.color;
                // --------------------------------------------------------------------
            }

            var m = go.GetComponent<SceneObjectMeta>();
            if (m == null) m = go.AddComponent<SceneObjectMeta>();

            if (payload.timestamp != 0 && m.lastTimestamp != 0 && payload.timestamp <= m.lastTimestamp) return;
            if (payload.timestamp != 0) m.lastTimestamp = payload.timestamp;
            else m.lastTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // --- POSITIONNEMENT ---
            Vector3 safePos = payload.position;
            safePos.y += 0.2f;

            go.transform.localPosition = safePos;
            go.transform.localRotation = Quaternion.Euler(payload.rotation);
            if (payload.scale != Vector3.zero) go.transform.localScale = payload.scale;

            ClampLocalY(go.transform, minAllowedY);

            if (!string.IsNullOrWhiteSpace(payload.color))
            {
                Color col;
                if (ColorUtility.TryParseHtmlString(payload.color, out col)) ApplyColorOptimized(go, col);
            }

            // --- PHYSIQUE & COLLIDERS ---
            Collider[] colliders = go.GetComponentsInChildren<Collider>();
            if (colliders.Length == 0) FitColliderToChildren(go);
            else foreach (var c in colliders) if (c is MeshCollider mc) mc.convex = true;

            if (go.GetComponent<RespawnIfFallen>() == null) go.AddComponent<RespawnIfFallen>();

            // --- GRABBABLE ---
            var grab = go.GetComponent<XRGrabInteractable>();
            if (payload.grabbable)
            {
                if (grab == null) grab = go.AddComponent<XRGrabInteractable>();
                Rigidbody rb = go.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = false;
                    rb.useGravity = true;
                    rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
                    rb.linearDamping = 1f;
                    rb.angularDamping = 0.5f;
                    rb.maxDepenetrationVelocity = 2f;
                }

                if (mqttManager == null)
                {
#if UNITY_2023_1_OR_NEWER
                    mqttManager = UnityEngine.Object.FindFirstObjectByType<MQTT>();
#else
                    mqttManager = FindObjectOfType<MQTT>();
#endif
                }

                var publisher = go.GetComponent<GrabPublishHandler>();
                if (publisher == null) publisher = go.AddComponent<GrabPublishHandler>();
                publisher.Initialize(go, m, mqttManager, Mathf.Max(1, publishRateHz), publishMoveThreshold);

                try { grab.selectEntered.RemoveAllListeners(); } catch { }
                try { grab.selectExited.RemoveAllListeners(); } catch { }
                grab.selectEntered.AddListener(publisher.OnSelectEntered);
                grab.selectExited.AddListener(publisher.OnSelectExited);
            }
            else
            {
                if (grab != null) Destroy(grab);
                var rb = go.GetComponent<Rigidbody>();
                if (rb != null) rb.isKinematic = true;
                var pub = go.GetComponent<GrabPublishHandler>();
                if (pub != null) Destroy(pub);
            }

            Debug.Log($"[ObjectFactory] SUCCÈS : {id}");
        }
        finally
        {
            if (_creationInProgress.Contains(id)) _creationInProgress.Remove(id);
        }
    }

    // --- HELPER METHODS ---
    private void ClampLocalY(Transform t, float minY)
    {
        if (t == null) return;
        Vector3 p = t.localPosition;
        if (p.y < minY) p.y = minY;
        t.localPosition = p;
    }

    private async Task<GameObject> CreateFromUrlAsync(string url, Transform parent)
    {
        GameObject wrapper = new GameObject("GLTF_Container");
        wrapper.transform.SetParent(parent, false);
        var gltf = new GltfImport();
        var success = await gltf.Load(url);
        if (success) { if (await gltf.InstantiateMainSceneAsync(wrapper.transform)) return wrapper; }
        Destroy(wrapper); return null;
    }

    private void FitColliderToChildren(GameObject go)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) { go.AddComponent<BoxCollider>(); return; }
        Bounds bounds = renderers[0].bounds;
        foreach (Renderer r in renderers) bounds.Encapsulate(r.bounds);
        BoxCollider collider = go.GetComponent<BoxCollider>();
        if (collider == null) collider = go.AddComponent<BoxCollider>();
        collider.center = go.transform.InverseTransformPoint(bounds.center);
        Vector3 localSize = bounds.size;
        if (go.transform.lossyScale.x != 0) localSize.x /= go.transform.lossyScale.x;
        if (go.transform.lossyScale.y != 0) localSize.y /= go.transform.lossyScale.y;
        if (go.transform.lossyScale.z != 0) localSize.z /= go.transform.lossyScale.z;
        collider.size = localSize;
    }

    private void ApplyColorOptimized(GameObject go, Color color)
    {
        var rend = go.GetComponentInChildren<Renderer>();
        if (rend == null) return;
        if (_materialCache.TryGetValue(color, out Material cachedMat)) { if (rend.sharedMaterial != cachedMat) rend.sharedMaterial = cachedMat; }
        else { Material baseMat = defaultMaterial != null ? defaultMaterial : new Material(Shader.Find("Standard")); var newMat = new Material(baseMat); newMat.color = color; newMat.enableInstancing = true; _materialCache[color] = newMat; rend.sharedMaterial = newMat; }
    }

    private GameObject CreatePrimitiveFromShape(string shape)
    {
        string s = (shape ?? "cube").ToLower();
        GameObject primitive = s == "sphere" ? GameObject.CreatePrimitive(PrimitiveType.Sphere) : (s == "cylinder" ? GameObject.CreatePrimitive(PrimitiveType.Cylinder) : GameObject.CreatePrimitive(PrimitiveType.Cube));
        var mf = primitive.GetComponent<MeshFilter>();
        if (mf != null) { if (s == "cube" && cubeMesh != null) mf.sharedMesh = cubeMesh; else if (s == "sphere" && sphereMesh != null) mf.sharedMesh = sphereMesh; else if (s == "cylinder" && cylinderMesh != null) mf.sharedMesh = cylinderMesh; }
        return primitive;
    }

    private void ApplyMaterial(GameObject go, Material mat) { if (mat && go.GetComponentInChildren<Renderer>()) go.GetComponentInChildren<Renderer>().sharedMaterial = mat; }
    private string ExtractIdFromTopic(string topic) { try { return topic.Split('/')[2]; } catch { return null; } }

    private class GrabPublishHandler : MonoBehaviour
    {
        private GameObject _go; private SceneObjectMeta _meta; private MQTT _mqtt;
        private bool _isGrabbed; private float _intervalSec = 0.1f; private float _lastPublishTime = -999f;
        private float _moveThreshold = 0.001f; private Vector3 _lastPublishedPos;

        public void Initialize(GameObject go, SceneObjectMeta meta, MQTT mqtt, int rate, float thresh)
        { _go = go; _meta = meta; _mqtt = mqtt; _intervalSec = 1f / rate; _moveThreshold = thresh; _lastPublishedPos = go.transform.localPosition; }

        public void OnSelectEntered(SelectEnterEventArgs args)
        {
            _isGrabbed = true; if (_meta) _meta.isGrabbed = true;
            var rb = _go.GetComponent<Rigidbody>(); if (rb) rb.isKinematic = true;
            PublishState();
        }
        public void OnSelectExited(SelectExitEventArgs args)
        {
            _isGrabbed = false; if (_meta) _meta.isGrabbed = false;
            var rb = _go.GetComponent<Rigidbody>();
            if (rb) { rb.isKinematic = false; rb.useGravity = true; rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; rb.maxDepenetrationVelocity = 2f; }
            PublishState();
        }
        void Update()
        {
            if (!_isGrabbed) return;
            if (Time.time - _lastPublishTime >= _intervalSec && (_go.transform.localPosition - _lastPublishedPos).sqrMagnitude >= _moveThreshold * _moveThreshold) PublishState();
        }
        private void PublishState()
        {
            if (!_go || !_meta) return;
#if UNITY_2023_1_OR_NEWER
             if(!_mqtt) _mqtt = FindFirstObjectByType<MQTT>();
#else
            if (!_mqtt) _mqtt = FindObjectOfType<MQTT>();
#endif
            long ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // --- 6. MODIF : ON RENVOIE TOUT (Prefab, Shape, Color) ---
            var payload = new FurniturePayload
            {
                id = _meta.id,
                objectType = _go.name,
                position = _go.transform.localPosition,
                rotation = _go.transform.localRotation.eulerAngles,
                scale = _go.transform.localScale,
                grabbable = true,
                timestamp = ts,
                url = _meta.sourceUrl,

                // On récupère ce qu'on avait sauvegardé
                prefab = _meta.prefab,
                shape = _meta.shape,
                color = _meta.color
            };
            // ---------------------------------------------------------

            string json = JsonUtility.ToJson(payload);
            _meta.lastTimestamp = ts; _lastPublishedPos = _go.transform.localPosition;
            if (_mqtt) _mqtt.PublishAsync(_meta.topic, json, false);
            _lastPublishTime = Time.time;
        }
        void OnDestroy() { try { var g = _go.GetComponent<XRGrabInteractable>(); if (g) { g.selectEntered.RemoveListener(OnSelectEntered); g.selectExited.RemoveListener(OnSelectExited); } } catch { } }
    }
}

// --- CLASSE DE SECURITE (SOLUTION 3) ---
public class RespawnIfFallen : MonoBehaviour
{
    private Vector3 _startPos;
    private float _limitY = -10f;

    void Start()
    {
        _startPos = transform.localPosition;
    }

    void Update()
    {
        if (transform.localPosition.y < _limitY)
        {
            Vector3 resetPos = _startPos;
            resetPos.y = Mathf.Max(resetPos.y, 1f);
            transform.localPosition = resetPos;
            var rb = GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }
}