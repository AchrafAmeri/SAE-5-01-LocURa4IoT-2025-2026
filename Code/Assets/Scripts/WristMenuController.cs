using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class WristMenuController : MonoBehaviour
{
    public enum Axis { Up, Down, Forward, Back, Right, Left }

    [Header("UI Elements")]
    [SerializeField] TextMeshProUGUI clockText;
    [SerializeField] GameObject menuPanel;

    [Header("References")]
    [SerializeField] Transform cameraTransform;
    [SerializeField] Transform controllerBase; // Utilise Controller_Base ici

    [Header("Settings")]
    [SerializeField] Axis lookAxis = Axis.Forward; // Souvent Forward sur le Controller_Base
    [SerializeField] float angleThreshold = 0.35f;

    [Header("Tutorial Reference")]
    [SerializeField] TutorialManager tutorialManager;

    [Header("Passthrough")]
    [SerializeField] ToggleAR passthroughController;

    void Start()
    {
        if (cameraTransform == null) cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        UpdateClock();
        HandleVisibility();
    }

    void UpdateClock()
    {
        if (clockText != null && clockText.gameObject.activeInHierarchy)
            clockText.text = DateTime.Now.ToString("HH:mm");
    }

    void HandleVisibility()
    {
        if (menuPanel == null || controllerBase == null) return;

        Vector3 worldAxis = GetSelectedAxis(controllerBase);
        Vector3 targetDir = (cameraTransform.position - controllerBase.position).normalized;
        float dot = Vector3.Dot(worldAxis, targetDir);

        // Active le menu si l'angle est correct
        menuPanel.SetActive(dot > angleThreshold);
    }

    public void Action_RestartTuto()
    {
        if (tutorialManager != null)
        {
            // 1. On s'assure que l'objet est actif
            tutorialManager.gameObject.SetActive(true);

            // 2. On réinitialise l'affichage du fond si nécessaire
            if (tutorialManager.tutorialBackground != null)
                tutorialManager.tutorialBackground.gameObject.SetActive(true);

            // 3. On lance la fonction de bienvenue
            tutorialManager.StartTutorial();
        }
    }

    public void Action_ChangeColor(Image buttonImage)
    {
        if (buttonImage != null)
        {
            buttonImage.color = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, 1.0f);
        }
    }

    // Méthode pour basculer le mode AR
    public void Action_TogglePassthrough()
    {
        if (passthroughController != null)
        {
            passthroughController.SwitchMode();
        }
    }

    Vector3 GetSelectedAxis(Transform t)
    {
        switch (lookAxis)
        {
            case Axis.Up: return t.up;
            case Axis.Down: return -t.up;
            case Axis.Forward: return t.forward;
            case Axis.Back: return -t.forward;
            case Axis.Right: return t.right;
            case Axis.Left: return -t.right;
            default: return -t.up;
        }
    }
}