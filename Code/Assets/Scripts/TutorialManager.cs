using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class TutorialManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject stepMoveText;
    public GameObject stepGrabText;
    public GameObject skipButton;
    public Button nextButton;
    public RectTransform instructionImage;

    private TextMeshProUGUI moveTextTMP;

    private bool hasMoved = false;
    private bool hasJumped = false;
    private bool hasRotated = false;
    private bool hasLookedBack = false;
    private bool hasGrabbed = false;
    private bool tutorialStarted = false;
    private bool stepReady = false; // Permet de savoir si l’étape actuelle est validée

    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction rotateAction;

    private bool movedLeft, movedRight, movedForward, movedBackward;
    private float fadeLeft, fadeRight, fadeForward, fadeBackward;
    private float jumpFade, rotateFade, lookBackFade;
    private int jumpCount;

    private Vector3 imageStartPos = new Vector3(-6f, 210f, -888f);
    private Vector3 imageEndPos = new Vector3(-6f, 243f, 403f);

    void Start()
    {
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
        stepMoveText.SetActive(false);
        stepGrabText.SetActive(false);

        if (instructionImage != null)
            instructionImage.localPosition = imageStartPos;
    }

    void Update()
    {
        if (!tutorialStarted) return;

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

            // Étape validée mais pas encore passée
            if (movedLeft && movedRight && movedForward && movedBackward && !stepReady)
            {
                stepReady = true;
                nextButton.interactable = true;
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

    private string ColoredLine(float fade, string text)
    {
        Color c = Color.Lerp(Color.gray, Color.green, fade);
        return $"<color=#{ColorUtility.ToHtmlStringRGB(c)}>{text}</color>";
    }

    private void UpdateMoveStepText()
    {
        string t =
            "Bouge dans les 4 directions avec le stick gauche :\n" +
            ColoredLine(fadeLeft, "Gauche") + "\n" +
            ColoredLine(fadeRight, "Droite") + "\n" +
            ColoredLine(fadeForward, "Avant") + "\n" +
            ColoredLine(fadeBackward, "Arrière") +
            "\n\nEn alternative, tu peux aussi te déplacer rapidement :\n" +
            "Pousse le stick droit vers l’avant pour faire apparaître la courbe bleue.\n" +
            "Relâche le stick pour te téléporter à l’endroit du cercle.";

        moveTextTMP.text = t;
    }

    private void UpdateJumpStepText()
    {
        moveTextTMP.text = $"Sauter 3 fois avec le bouton A : {ColoredLine(jumpFade, $"{jumpCount}/3")}";
    }

    private void UpdateRotateStepText()
    {
        moveTextTMP.text =
            "Pivoter ton regard :\n" +
            ColoredLine(rotateFade, "Pousse le stick droit vers la gauche ou vers la droite");
    }

    private void UpdateLookBackStepText()
    {
        moveTextTMP.text =
            "Regarde derrière toi :\n" +
            ColoredLine(lookBackFade, "Pousse le stick droit vers l’arrière pour faire pivoter la caméra de 180°.");
    }

    public void StartTutorial()
    {
        tutorialStarted = true;
        hasMoved = hasGrabbed = hasJumped = hasRotated = hasLookedBack = false;
        movedLeft = movedRight = movedForward = movedBackward = false;
        fadeLeft = fadeRight = fadeForward = fadeBackward = 0f;
        jumpFade = rotateFade = lookBackFade = 0f;
        jumpCount = 0;
        stepReady = false;

        stepMoveText.SetActive(true);
        stepGrabText.SetActive(false);
        skipButton.SetActive(true);
        nextButton.interactable = false;

        if (instructionImage != null)
            instructionImage.localPosition = imageStartPos;

        UpdateMoveStepText();
    }

    public void OnNextButtonPressed()
    {
        if (!tutorialStarted || !stepReady) return;

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
            StartCoroutine(MoveInstructionImageThenGrab());
        }
    }

    private IEnumerator MoveInstructionImageThenGrab()
    {
        nextButton.gameObject.SetActive(false);

        if (instructionImage != null)
        {
            float duration = 3f;
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                instructionImage.localPosition = Vector3.Lerp(imageStartPos, imageEndPos, elapsed / duration);
                yield return null;
            }
            instructionImage.localPosition = imageEndPos;
        }

        stepMoveText.SetActive(false);
        stepGrabText.SetActive(true);
    }

    public void OnTabletGrabbed()
    {
        if (!tutorialStarted || hasGrabbed) return;
        hasGrabbed = true;
        stepGrabText.SetActive(false);
        EndTutorial();
    }

    public void SkipTutorial()
    {
        if (!tutorialStarted) return;
        stepMoveText.SetActive(false);
        stepGrabText.SetActive(false);
        EndTutorial();
    }

    void EndTutorial()
    {
        tutorialStarted = false;
        skipButton.SetActive(false);
        nextButton.gameObject.SetActive(false);

        if (instructionImage != null)
            instructionImage.gameObject.SetActive(false);
    }
}
