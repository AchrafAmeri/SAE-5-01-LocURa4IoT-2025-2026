using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GrabbablePosePublisher : MonoBehaviour
{
    [SerializeField] private MQTT mqtt;
    [SerializeField] private string topic = "localisation/dwm1001-999";
    [SerializeField] private bool publishOnStart = true;
    [SerializeField] private bool publishWhileGrabbed = true;
    [SerializeField] private float publishInterval = 0.2f;
    [SerializeField] private bool publishOnGrab = true;
    [SerializeField] private bool publishOnRelease = true;

    private UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable grab;
    private bool isGrabbed;
    private float nextPublishTime;

    private Vector3 initialPos;
    private Quaternion initialRot;
    private bool initialSent;

    private void Awake()
    {
        grab = GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();

        if (grab)
        {
            grab.selectEntered.AddListener(OnSelectEntered);
            grab.selectExited.AddListener(OnSelectExited);
        }

        initialPos = transform.position;
        initialRot = transform.rotation;

        if (mqtt != null)
        {
            mqtt.Connected += OnMqttConnected;
            Debug.Log("[PosePublisher] mqtt référencé = " + mqtt.name);
        }
        else
        {
            Debug.LogError("[PosePublisher] Champ 'mqtt' NON assigné (glisse MQTTManager ici)");
        }
    }

    private void OnDestroy()
    {
        if (mqtt != null) mqtt.Connected -= OnMqttConnected;
        if (grab)
        {
            grab.selectEntered.RemoveListener(OnSelectEntered);
            grab.selectExited.RemoveListener(OnSelectExited);
        }
    }

    private void Start()
    {
        if (publishOnStart)
        {
            if (mqtt != null && mqtt.IsConnected)
            {
                Debug.Log("[PosePublisher] Start: déjà connecté → publish initial");
                _ = PublishInitialPoseRetained();
            }
            else
            {
                Debug.Log("[PosePublisher] Start: pas encore connecté → on attend l'évènement");
            }
        }
    }

    private void OnMqttConnected()
    {
        Debug.Log("[PosePublisher] OnMqttConnected reçu → publish initial");
        if (publishOnStart) _ = PublishInitialPoseRetained();
    }

    // ---------- Handlers d’événements XRI ----------
    private void OnSelectEntered(SelectEnterEventArgs args)
    {
        isGrabbed = true;
        nextPublishTime = 0f;
        if (publishOnGrab) _ = PublishPose(retain: true);
    }

    private void OnSelectExited(SelectExitEventArgs args)
    {
        isGrabbed = false;
        if (publishOnRelease) _ = PublishPose(retain: true);
    }

    private async System.Threading.Tasks.Task PublishInitialPoseRetained()
    {
        if (initialSent || mqtt == null) return;
        initialSent = true;

        var p = initialPos; var r = initialRot;
        string json = "{"
            + "\"timestamp\":" + System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(System.Globalization.CultureInfo.InvariantCulture) + ","
            + "\"position\":{"
                + "\"x\":" + p.x.ToString(System.Globalization.CultureInfo.InvariantCulture) + ","
                + "\"y\":" + p.y.ToString(System.Globalization.CultureInfo.InvariantCulture) + ","
                + "\"z\":" + p.z.ToString(System.Globalization.CultureInfo.InvariantCulture)
            + "},"
            + "\"rotation\":{"
                + "\"x\":" + r.x.ToString(System.Globalization.CultureInfo.InvariantCulture) + ","
                + "\"y\":" + r.y.ToString(System.Globalization.CultureInfo.InvariantCulture) + ","
                + "\"z\":" + r.z.ToString(System.Globalization.CultureInfo.InvariantCulture) + ","
                + "\"w\":" + r.w.ToString(System.Globalization.CultureInfo.InvariantCulture)
            + "}"
        + "}";

        await mqtt.PublishAsync(topic, json, retain: true);
    }

    private async System.Threading.Tasks.Task PublishPose(bool retain)
    {
        if (mqtt == null) return;

        var t = transform; Vector3 p = t.position; Quaternion r = t.rotation;
        string json = "{"
            + "\"timestamp\":" + System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(System.Globalization.CultureInfo.InvariantCulture) + ","
            + "\"position\":{"
                + "\"x\":" + p.x.ToString(System.Globalization.CultureInfo.InvariantCulture) + ","
                + "\"y\":" + p.y.ToString(System.Globalization.CultureInfo.InvariantCulture) + ","
                + "\"z\":" + p.z.ToString(System.Globalization.CultureInfo.InvariantCulture)
            + "},"
            + "\"rotation\":{"
                + "\"x\":" + r.x.ToString(System.Globalization.CultureInfo.InvariantCulture) + ","
                + "\"y\":" + r.y.ToString(System.Globalization.CultureInfo.InvariantCulture) + ","
                + "\"z\":" + r.z.ToString(System.Globalization.CultureInfo.InvariantCulture) + ","
                + "\"w\":" + r.w.ToString(System.Globalization.CultureInfo.InvariantCulture)
            + "}"
        + "}";

        await mqtt.PublishAsync(topic, json, retain);
    }
}
