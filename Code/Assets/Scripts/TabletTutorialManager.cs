using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class TabletTutorialManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject tutorialTextObject;
    private TextMeshProUGUI tutorialTMP;

    [Header("Buttons to Highlight")]
    public Button connectionButton;
    public Button courseButton;
    public Button rangingButton;
    public Button setCourseButton;
    public GameObject moveButtonsGroup;

    [Header("Panels")]
    public GameObject railPanel;
    public GameObject setCoursePanel;
    public GameObject rangingPanel;

    [Header("Controls")]
    public Button nextButton;
    public GameObject skipButton;

    private int currentStep = 0;
    public bool tutorialActive = false;

    private readonly List<GameObject> _activeFrames = new List<GameObject>();

    private bool hasGrabbedTablet = false; 

    void Start()
    {
        if (tutorialTextObject != null)
            tutorialTMP = tutorialTextObject.GetComponent<TextMeshProUGUI>();

        railPanel?.SetActive(true);
        setCoursePanel?.SetActive(false);
        rangingPanel?.SetActive(false);
    }

    public void StartTabletTutorial(GameObject sharedText)
    {
        tutorialActive = true;
        currentStep = 0;

        tutorialTextObject = sharedText;
        tutorialTMP = tutorialTextObject.GetComponent<TextMeshProUGUI>();
        tutorialTextObject.SetActive(true);

        if (nextButton != null)
        {
            nextButton.gameObject.SetActive(true);
            nextButton.interactable = true;
            nextButton.transform.localPosition = new Vector3(205.8f, -457f, 312f);

            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(NextStep);
        }

        ShowStep();
    }

    public void NextStep()
    {
        if (!tutorialActive) return;

        currentStep++;
        ShowStep();
    }

    private void ShowStep()
    {
        StopHighlight();

        switch (currentStep)
        {
            case 0:
                tutorialTMP.text = "<b>Voici la tablette virtuelle :</b>\nElle permet de piloter le rail et d’afficher les mesures.\n<color=#B0B0B0>(Finir le tuto avant de l'attraper)</color>";
                break;

            case 1:
                tutorialTMP.text = "Ces boutons déplacent le chariot sur le rail.";
                HighlightGroup(moveButtonsGroup);
                break;

            case 2:
                tutorialTMP.text = "Ce bouton indique la connexion au réseau MQTT :\n   <color=#00FF00>vert = connecté</color>\n   <color=#FF0000>rouge = déconnecté</color>.";
                HighlightButton(connectionButton);
                break;

            case 3:
                tutorialTMP.text = "Ce panneau affiche la position actuelle du chariot.";
                HighlightButton(courseButton);
                break;

            case 4:
                tutorialTMP.text = "En cliquant dessus, tu peux choisir une position précise avec le curseur puis valider.";
                railPanel.SetActive(false);
                setCoursePanel.SetActive(true);
                HighlightButton(setCourseButton);
                break;

            case 5:
                tutorialTMP.text = "Le bouton Ranging permet d'ouvrir le menu des mesures.";
                setCoursePanel.SetActive(false);
                railPanel.SetActive(true);
                HighlightButton(rangingButton);
                break;

            case 6:
                tutorialTMP.text = "Ici, tu peux choisir comment afficher les mesures : par segments ou par sphères.";
                railPanel.SetActive(false);
                rangingPanel.SetActive(true);
                break;

            case 7:
                tutorialTMP.text =
                    "Bravo ! Maintenant tu sais utiliser la tablette.\n" +
                    "<color=#B0B0B0>Tu peux l'attraper avec le grip droit ou gauche.</color>";

                setCoursePanel.SetActive(false);
                rangingPanel.SetActive(false);
                railPanel.SetActive(true);

                StopHighlight();

                if (nextButton != null)
                {
                    nextButton.gameObject.SetActive(true);
                    nextButton.interactable = false;

                    TextMeshProUGUI btnText = nextButton.GetComponentInChildren<TextMeshProUGUI>();
                    if (btnText != null)
                        btnText.text = "Terminer";

                    nextButton.onClick.RemoveAllListeners();
                    nextButton.onClick.AddListener(EndTabletTutorial);
                }
                break;
        }
    }
    public void OnGrab()
    {
        if (!tutorialActive || hasGrabbedTablet) return;
        if (currentStep == 7)
        {
            hasGrabbedTablet = true;

            tutorialTMP.text =
                "Bravo ! Maintenant tu sais utiliser la tablette.\n" +
                "<color=#00FF00>Tu peux l'attraper avec le grip droit ou gauche.</color>";

            if (nextButton != null)
                nextButton.interactable = true;

        }
    }

    private void EndTabletTutorial()
    {
        tutorialActive = false;
        StopHighlight();

        setCoursePanel.SetActive(false);
        rangingPanel.SetActive(false);
        railPanel.SetActive(true);

        if (tutorialTextObject != null)
            tutorialTextObject.SetActive(false);

        if (nextButton != null)
            nextButton.gameObject.SetActive(false);

        if (skipButton != null)
            skipButton.SetActive(false);
    }

    // --- Pose un cadre lumineux autour d’un bouton ---
    private void HighlightButton(Button btn)
    {
        if (btn == null) return;
        StopHighlight();

        var frame = CreateBorderLines(btn.transform as RectTransform);
        _activeFrames.Add(frame);
        StartCoroutine(PulseLines(frame));
    }

    // --- Même chose pour un groupe ---
    private void HighlightGroup(GameObject group)
    {
        if (group == null) return;
        StopHighlight();

        var buttons = group.GetComponentsInChildren<Button>(true);
        foreach (var b in buttons)
        {
            var frame = CreateBorderLines(b.transform as RectTransform);
            _activeFrames.Add(frame);
            StartCoroutine(PulseLines(frame));
        }
    }

    // --- Crée 4 lignes lumineuses autour du bouton ---
    private GameObject CreateBorderLines(RectTransform parent)
    {
        GameObject frame = new GameObject("HL_BorderLines", typeof(RectTransform));
        var rt = frame.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = new Vector2(-8f, -8f);
        rt.offsetMax = new Vector2(8f, 8f);
        rt.SetAsLastSibling();

        Color borderColor = new Color(0f, 0.95f, 1f, 0.8f);

        CreateLine(frame.transform, new Vector2(0, 1), new Vector2(1, 1), borderColor, true);  // haut
        CreateLine(frame.transform, new Vector2(0, 0), new Vector2(1, 0), borderColor, true);  // bas
        CreateLine(frame.transform, new Vector2(0, 0), new Vector2(0, 1), borderColor, false); // gauche
        CreateLine(frame.transform, new Vector2(1, 0), new Vector2(1, 1), borderColor, false); // droite

        return frame;
    }

    // --- Crée une ligne épaisse horizontale ou verticale ---
    private void CreateLine(Transform parent, Vector2 anchorMin, Vector2 anchorMax, Color color, bool horizontal)
    {
        var line = new GameObject("Line", typeof(RectTransform), typeof(Image));
        var rt = line.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var img = line.GetComponent<Image>();
        img.color = color;
        img.raycastTarget = false;

        rt.sizeDelta = horizontal ? new Vector2(0, 4f) : new Vector2(4f, 0);
    }

    // --- Fait pulser la transparence + légère variation de teinte ---
    private IEnumerator PulseLines(GameObject frame)
    {
        if (frame == null) yield break;

        var images = frame.GetComponentsInChildren<Image>();
        if (images.Length == 0) yield break;

        float t = 0f;
        const float speed = 3.5f;
        const float minA = 0.3f;
        const float maxA = 1f;
        const float hueShift = 0.05f;

        while (tutorialActive)
        {
            t += Time.deltaTime * speed;
            float a = Mathf.Lerp(minA, maxA, Mathf.PingPong(t, 1f));

            Color.RGBToHSV(Color.yellow, out float h, out float s, out float v);
            h = Mathf.Lerp(h, h + hueShift, Mathf.PingPong(t * 0.5f, 1f));
            Color pulseColor = Color.HSVToRGB(h, s, v);
            pulseColor.a = a;

            foreach (var img in images)
            {
                if (img != null)
                    img.color = pulseColor;
            }

            yield return null;
        }

        foreach (var img in images)
        {
            if (img != null)
                img.color = new Color(img.color.r, img.color.g, img.color.b, 0f);
        }
    }

    // --- Nettoyage complet ---
    public void StopHighlight()
    {
        StopAllCoroutines();
        foreach (var f in _activeFrames)
            if (f != null)
                Destroy(f);
        _activeFrames.Clear();
    }

}
