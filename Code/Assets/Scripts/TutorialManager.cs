using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class TutorialManager : MonoBehaviour
{
    private enum TutorialState { Welcome, InProgress, Done }
    private TutorialState currentState = TutorialState.Welcome;

    [Header("UI Elements (Mouvement)")]
    public GameObject stepMoveText;
    public GameObject stepGrabText;
    public GameObject skipButton;
    public Button nextButton;
    public RectTransform instructionImage;
    public RectTransform tutorialBackground;

    private TextMeshProUGUI moveTextTMP;

    private bool hasMoved = false;
    private bool hasJumped = false;
    private bool hasRotated = false;
    private bool hasLookedBack = false;
    private bool stepReady = false;

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction rotateAction;

    private bool movedLeft, movedRight, movedForward, movedBackward;
    private float fadeLeft, fadeRight, fadeForward, fadeBackward;
    private float jumpFade, rotateFade, lookBackFade;
    private int jumpCount;

    private Vector3 skipButtonWelcomePos = new Vector3(-230f, -76f, -868f);
    private Vector3 skipButtonTutorialPos = new Vector3(-309f, 557f, -868f);

    private Vector2 backgroundWelcomeSize = new Vector2(800f, 300f);
    private Vector2 backgroundTutorialSize = new Vector2(800f, 1000f);

    // --- Champs migrés depuis TabletTutorialManager ---
    [Header("UI Elements (Tablette)")]
    private TextMeshProUGUI tabletTutorialTMP;

    [Header("Buttons to Highlight (Tablette)")]
    public Button connectionButton;
    public Button courseButton;
    public Button rangingButton;
    public Button setCourseButton;
    public GameObject moveButtonsGroup;

    [Header("Panels (Tablette)")]
    public GameObject railPanel;
    public GameObject setCoursePanel;
    public GameObject rangingPanel;

    private int tabletCurrentStep = 0;
    private bool tabletTutorialActive = false;
    private readonly List<GameObject> _activeFrames = new List<GameObject>();
    private bool hasGrabbedTablet = false;


    void Start()
    {
        // --- Logique de Start (TutorialManager) ---
        var actionAsset = Resources.Load<InputActionAsset>("XRI Default Input Actions");
        if (actionAsset != null)
        {
            moveAction = actionAsset.FindAction("XRI Left Locomotion/Move");
            jumpAction = actionAsset.FindAction("XRI Right Locomotion/Jump");
            rotateAction = actionAsset.FindAction("XRI Right Locomotion/Turn");

            moveAction?.Enable();
            jumpAction?.Enable();
            rotateAction?.Enable();
        }

        moveTextTMP = stepMoveText.GetComponent<TextMeshProUGUI>();

        // --- Logique de Start (TabletTutorialManager) ---
        if (stepGrabText != null) // Utilise stepGrabText au lieu de tutorialTextObject
            tabletTutorialTMP = stepGrabText.GetComponent<TextMeshProUGUI>();

        railPanel?.SetActive(true);
        setCoursePanel?.SetActive(false);
        rangingPanel?.SetActive(false);

        // --- On cache tout en attendant l'appel de MQTT ---
        stepMoveText.SetActive(false);
        stepGrabText.SetActive(false);
        skipButton.SetActive(false);
        nextButton.gameObject.SetActive(false);
        if (instructionImage != null)
            instructionImage.gameObject.SetActive(false);
    }

    void Update()
    {
        // Ne s'exécute QUE si le tutoriel est en cours (pas à l'accueil)
        if (currentState != TutorialState.InProgress) return;

        // --- Étape 1 : Déplacement ---
        if (!hasMoved && moveAction != null)
        {
            Vector2 v = moveAction.ReadValue<Vector2>();

            if (v.magnitude > 0.2f)
            {
                if (v.x > 0.3f) movedRight = true;
                if (v.x < -0.3f) movedLeft = true;
                if (v.y > 0.3f) movedForward = true;
                if (v.y < -0.3f) movedBackward = true;
            }

            fadeLeft = Mathf.MoveTowards(fadeLeft, movedLeft ? 1f : 0f, Time.deltaTime * 3f);
            fadeRight = Mathf.MoveTowards(fadeRight, movedRight ? 1f : 0f, Time.deltaTime * 3f);
            fadeForward = Mathf.MoveTowards(fadeForward, movedForward ? 1f : 0f, Time.deltaTime * 3f);
            fadeBackward = Mathf.MoveTowards(fadeBackward, movedBackward ? 1f : 0f, Time.deltaTime * 3f);

            UpdateMoveStepText();

            if (movedLeft && movedRight && movedForward && movedBackward && !stepReady)
            {
                stepReady = true;
                nextButton.interactable = true;
                UpdateMoveStepText();
            }
        }

        // --- Étape 2 : Saut ---
        else if (hasMoved && !hasJumped && jumpAction != null)
        {
            if (jumpAction.triggered)
            {
                jumpCount++;
                jumpFade = Mathf.Min(1f, jumpFade + 0.33f);
                UpdateJumpStepText();

                if (jumpCount >= 3 && !stepReady)
                {
                    stepReady = true;
                    nextButton.interactable = true;
                }
            }
        }

        // --- Étape 3 : Rotation ---
        else if (hasJumped && !hasRotated && rotateAction != null)
        {
            Vector2 rotValue = rotateAction.ReadValue<Vector2>();
            if (Mathf.Abs(rotValue.x) > 0.5f && !stepReady)
            {
                rotateFade = 1f;
                stepReady = true;
                nextButton.interactable = true;
            }

            if (!stepReady)
                rotateFade = Mathf.MoveTowards(rotateFade, 0f, Time.deltaTime * 2f);
            UpdateRotateStepText();
        }

        // --- Étape 4 : Regarder derrière ---
        else if (hasRotated && !hasLookedBack && rotateAction != null)
        {
            Vector2 rotValue = rotateAction.ReadValue<Vector2>();
            if (rotValue.y < -0.5f && !stepReady)
            {
                lookBackFade = 1f;
                stepReady = true;
                nextButton.interactable = true;
            }

            if (!stepReady)
                lookBackFade = Mathf.MoveTowards(lookBackFade, 0f, Time.deltaTime * 2f);
            UpdateLookBackStepText();
        }
    }

    // --- Méthodes de TutorialManager (Original) ---

    private string ColoredLine(float fade, string text)
    {
        Color c = Color.Lerp(Color.gray, Color.green, fade);
        return $"<color=#{ColorUtility.ToHtmlStringRGB(c)}>{text}</color>";
    }

    private void UpdateMoveStepText()
    {
        string t =
            "Bougez dans les 4 directions avec le stick gauche :\n" +
            ColoredLine(fadeLeft, "Gauche") + "\n" +
            ColoredLine(fadeRight, "Droite") + "\n" +
            ColoredLine(fadeForward, "Avant") + "\n" +
            ColoredLine(fadeBackward, "Arrière");

        if (stepReady || hasMoved)
        {
            t += "\n\nAstuce :\n" +
                 "poussez le stick droit vers l’avant\n" +
                 "pour vous téléporter rapidement.";
        }

        moveTextTMP.text = t;
    }

    private void UpdateJumpStepText()
    {
        moveTextTMP.text = $"Sautez 3 fois avec le bouton A : {ColoredLine(jumpFade, $"{jumpCount}/3")}";
    }

    private void UpdateRotateStepText()
    {
        moveTextTMP.text =
            "Pivotez votre regard :\n" +
            ColoredLine(rotateFade, "Poussez le stick droit vers la gauche ou vers la droite");
    }

    private void UpdateLookBackStepText()
    {
        moveTextTMP.text =
            "Regardez derrière vous :\n" +
            ColoredLine(lookBackFade, "Poussez le stick droit vers l’arrière pour faire pivoter la caméra de 180°.");
    }

    /// <summary>
    /// Change le texte du composant TextMeshPro enfant d'un bouton.
    /// </summary>
    private void SetButtonText(GameObject buttonObject, string text)
    {
        if (buttonObject == null) return;

        TextMeshProUGUI btnText = buttonObject.GetComponentInChildren<TextMeshProUGUI>();
        if (btnText != null)
        {
            btnText.text = text;
        }
    }

    public void StartTutorial()
    {
        currentState = TutorialState.Welcome;

        // --- Afficher l'écran d'accueil ---
        if (tutorialBackground != null)
            tutorialBackground.sizeDelta = backgroundWelcomeSize;
            tutorialBackground.localPosition = new Vector3(tutorialBackground.localPosition.x, 0f, tutorialBackground.localPosition.z);

        // Masquer les éléments non désirés
        stepGrabText.SetActive(false);
        if (instructionImage != null)
            instructionImage.gameObject.SetActive(false);

        // Configurer le texte d'accueil
        stepMoveText.SetActive(true);
        moveTextTMP.text = "Bienvenue sur Locura4IoT VR !\n\nVoulez-vous suivre le tutoriel ?";

        // Configurer le bouton "Commencer"
        nextButton.gameObject.SetActive(true);
        nextButton.interactable = true;
        SetButtonText(nextButton.gameObject, "Commencer");

        // Configurer le bouton "Passer"
        skipButton.SetActive(true);
        SetButtonText(skipButton, "Passer le Tutoriel");
        skipButton.transform.localPosition = skipButtonWelcomePos;

        // Assigner les actions aux boutons
        nextButton.onClick.RemoveAllListeners();
        // "Commencer" appelle la fonction PRIVATE
        nextButton.onClick.AddListener(LaunchTutorialSteps);

        Button skipBtnComp = skipButton.GetComponent<Button>();
        skipBtnComp.onClick.RemoveAllListeners();
        skipBtnComp.onClick.AddListener(SkipTutorial); // "Passer" appelle SkipTutorial
    }

    private void LaunchTutorialSteps()
    {
        // Ne démarre que si on est sur l'écran d'accueil
        if (currentState != TutorialState.Welcome) return;

        // Passe à l'état "en cours"
        currentState = TutorialState.InProgress;

        if (tutorialBackground != null)
            tutorialBackground.sizeDelta = backgroundTutorialSize;
            tutorialBackground.localPosition = new Vector3(0f, 142f, -868f);


        // Ré-assigner les boutons pour le tutoriel
        nextButton.onClick.RemoveAllListeners();
        nextButton.onClick.AddListener(OnNextButtonPressed);
        // Le bouton Skip garde le même listener (SkipTutorial)

        // Changer le texte des boutons
        SetButtonText(nextButton.gameObject, "Suivant");
        SetButtonText(skipButton, "Passer le Tutoriel");

        // Activer l'image des manettes
        if (instructionImage != null)
            instructionImage.gameObject.SetActive(true);

        // Initialiser les variables du tutoriel
        hasMoved = hasJumped = hasRotated = hasLookedBack = false;
        movedLeft = movedRight = movedForward = movedBackward = false;
        fadeLeft = fadeRight = fadeForward = fadeBackward = 0f;
        jumpFade = rotateFade = lookBackFade = 0f;
        jumpCount = 0;
        stepReady = false;

        stepMoveText.SetActive(true);
        stepGrabText.SetActive(false);
        skipButton.SetActive(true);
        skipButton.transform.localPosition = skipButtonTutorialPos;
        nextButton.interactable = false;
        nextButton.gameObject.SetActive(true);

        UpdateMoveStepText(); // Affiche la première étape (mouvement)
    }

    public void OnNextButtonPressed()
    {
        // Ne s'exécute que si on est en cours de tuto
        if (currentState != TutorialState.InProgress || !stepReady) return;

        stepReady = false;
        nextButton.interactable = false;

        if (!hasMoved)
        {
            hasMoved = true;
            UpdateJumpStepText();
        }
        else if (!hasJumped)
        {
            hasJumped = true;
            UpdateRotateStepText();
        }
        else if (!hasRotated)
        {
            hasRotated = true;
            UpdateLookBackStepText();
        }
        else if (!hasLookedBack)
        {
            hasLookedBack = true;
            StartCoroutine(SecondStepGrab()); // Transition vers la partie tablette
        }
    }

    private IEnumerator SecondStepGrab()
    {
        nextButton.gameObject.SetActive(false);

        // --- 1. FONDUS SORTANTS ---
        StartCoroutine(FadeOut(instructionImage.gameObject, 0.8f));
        StartCoroutine(FadeOut(skipButton, 0.8f));
        if (tutorialBackground != null)
            StartCoroutine(FadeOut(tutorialBackground.gameObject, 0.8f));

        yield return StartCoroutine(FadeOut(stepMoveText, 0.8f));

        yield return new WaitForSeconds(0.2f); // Petite pause

        // --- 2. APPLIQUER LES NOUVELLES POSITIONS (pendant que c'est invisible) ---
        if (tutorialBackground != null)
        {
            tutorialBackground.localPosition = new Vector3(0f, -64f, 508f);
            tutorialBackground.sizeDelta = new Vector2(800f, 450f);
        }
        if (skipButton != null)
        {
            skipButton.transform.localPosition = new Vector3(-250f, -170f, 508f);
        }
        if (nextButton != null)
        {
            nextButton.transform.localPosition = new Vector3(250f, -170f, 508f);
        }

        // --- 3. PRÉPARER LA LOGIQUE TABLETTE (AVANT LE FONDU) ---
        tabletTutorialActive = true;
        tabletCurrentStep = 0;
        // On appelle ShowTabletStep() MAINTENANT pour que le texte (case 0)
        // soit prêt AVANT que le fondu ne commence.
        ShowTabletStep();

        // --- 4. FONDUS ENTRANTS ---
        StartCoroutine(FadeIn(skipButton, 0.8f));
        if (tutorialBackground != null)
            StartCoroutine(FadeIn(tutorialBackground.gameObject, 0.8f));
        if (nextButton != null)
            StartCoroutine(FadeIn(nextButton.gameObject, 0.8f));

        // Le stepGrabText a maintenant le bon texte, on peut lancer son fondu
        yield return StartCoroutine(FadeIn(stepGrabText, 0.8f));

        yield return new WaitForSeconds(1f); // Temps pour lire

        // --- 5. LOGIQUE TABLETTE (FIN) ---
        // Le texte et l'état sont déjà prêts, on configure juste le bouton
        if (nextButton != null)
        {
            nextButton.interactable = true;
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(NextTabletStep);
        }

        // On n'a plus besoin d'appeler ShowTabletStep() ici, c'est déjà fait
        // ShowTabletStep(); 
    }

    public void SkipTutorial()
    {
        currentState = TutorialState.Done; // Marque le tuto comme "fait"

        stepMoveText.SetActive(false);
        stepGrabText.SetActive(false);
        tutorialBackground.gameObject.SetActive(false);

        tabletTutorialActive = false;
        StopHighlight();

        EndTutorial();
    }

    void EndTutorial()
    {
        currentState = TutorialState.Done; // Confirme l'état "fait"

        skipButton.SetActive(false);
        nextButton.gameObject.SetActive(false);
        tutorialBackground.gameObject.SetActive(false);

        if (instructionImage != null)
            instructionImage.gameObject.SetActive(false);

        StopHighlight();

        setCoursePanel?.SetActive(false);
        rangingPanel?.SetActive(false);
        railPanel?.SetActive(true);
    }

    // --- Méthodes de fondu (FadeIn / FadeOut) ---
    // (Inchangé)
    private IEnumerator FadeIn(GameObject target, float duration = 1f)
    {
        if (target == null) yield break;
        CanvasGroup group = target.GetComponent<CanvasGroup>();
        if (group == null)
            group = target.AddComponent<CanvasGroup>();

        group.alpha = 0f;
        target.SetActive(true);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            yield return null;
        }
        group.alpha = 1f;
    }
    private IEnumerator FadeOut(GameObject target, float duration = 1f)
    {
        if (target == null) yield break;
        CanvasGroup group = target.GetComponent<CanvasGroup>();
        if (group == null)
            group = target.AddComponent<CanvasGroup>();
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            group.alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            yield return null;
        }
        group.alpha = 0f;
        target.SetActive(false);
    }


    // --- Méthodes migrées depuis TabletTutorialManager ---
    // (Inchangé)
    public void NextTabletStep()
    {
        if (!tabletTutorialActive) return;
        tabletCurrentStep++;
        ShowTabletStep();
    }
    private void ShowTabletStep()
    {
        StopHighlight();
        switch (tabletCurrentStep)
        {
            case 0:
                tabletTutorialTMP.text = "<b>Voici la tablette virtuelle :</b>\nElle permet de piloter le rail et d’afficher les mesures.\n<color=#B0B0B0>(Finissez le tuto avant de l'attraper)</color>";
                break;
            case 1:
                tabletTutorialTMP.text = "Ces boutons déplacent le chariot sur le rail.";
                HighlightGroup(moveButtonsGroup);
                break;
            case 2:
                tabletTutorialTMP.text = "Ce bouton indique la connexion au réseau MQTT :\n   <color=#00FF00>vert = connecté</color>\n   <color=#FF0000>rouge = déconnecté</color>.";
                HighlightButton(connectionButton);
                break;
            case 3:
                tabletTutorialTMP.text = "Ce panneau affiche la position actuelle du chariot.";
                HighlightButton(courseButton);
                break;
            case 4:
                tabletTutorialTMP.text = "En cliquant dessus, vous pouvez choisir une position précise avec le curseur puis valider.";
                railPanel.SetActive(false);
                setCoursePanel.SetActive(true);
                HighlightButton(setCourseButton);
                break;
            case 5:
                tabletTutorialTMP.text = "Le bouton Ranging permet d'ouvrir le menu des mesures.";
                setCoursePanel.SetActive(false);
                railPanel.SetActive(true);
                HighlightButton(rangingButton);
                break;
            case 6:
                tabletTutorialTMP.text = "Ici, vous pouvez choisir comment afficher les mesures : par segments ou par sphères.";
                railPanel.SetActive(false);
                rangingPanel.SetActive(true);
                break;
            case 7:
                tabletTutorialTMP.text =
                    "Bravo ! Maintenant vous savez utiliser la tablette.\n" +
                    "<color=#B0B0B0>vous pouvez l'attraper avec le grip droit ou gauche.</color>";
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
    public void OnTabletGrab()
    {
        if (!tabletTutorialActive || hasGrabbedTablet) return;
        if (tabletCurrentStep == 7)
        {
            hasGrabbedTablet = true;
            tabletTutorialTMP.text =
                "Bravo ! Maintenant vous savez utiliser la tablette.\n" +
                "<color=#00FF00>vous pouvez l'attraper avec le grip droit ou gauche.</color>";
            if (nextButton != null)
                nextButton.interactable = true;
        }
    }
    private void EndTabletTutorial()
    {
        tabletTutorialActive = false;
        StopHighlight();

        setCoursePanel.SetActive(false);
        rangingPanel.SetActive(false);
        railPanel.SetActive(true);

        if (stepGrabText != null)
            stepGrabText.SetActive(false);

        if (nextButton != null)
            nextButton.gameObject.SetActive(false);

        if (skipButton != null)
            skipButton.SetActive(false);

        if (tutorialBackground != null)
            tutorialBackground.gameObject.SetActive(false);

        currentState = TutorialState.Done; // Marque le tuto comme "fait"
    }

    // --- Système de surlignage (Highlight) ---
    // (Inchangé)
    private void HighlightButton(Button btn)
    {
        if (btn == null) return;
        StopHighlight();
        var frame = CreateBorderLines(btn.transform as RectTransform);
        _activeFrames.Add(frame);
        StartCoroutine(PulseLines(frame));
    }
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
        CreateLine(frame.transform, new Vector2(0, 1), new Vector2(1, 1), borderColor, true);
        CreateLine(frame.transform, new Vector2(0, 0), new Vector2(1, 0), borderColor, true);
        CreateLine(frame.transform, new Vector2(0, 0), new Vector2(0, 1), borderColor, false);
        CreateLine(frame.transform, new Vector2(1, 0), new Vector2(1, 1), borderColor, false);
        return frame;
    }
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
        while (tabletTutorialActive)
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
    public void StopHighlight()
    {
        // Correction de la logique de StopAllCoroutines 
        // (le code original dans PulseLines est correct, il s'arrête avec tabletTutorialActive)
        foreach (var f in _activeFrames)
            if (f != null)
                Destroy(f);
        _activeFrames.Clear();
    }
}