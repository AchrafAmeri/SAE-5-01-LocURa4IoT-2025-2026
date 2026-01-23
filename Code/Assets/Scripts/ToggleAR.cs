using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ToggleAR : MonoBehaviour
{
    // Glissez ici votre Main Camera dans l'inspecteur
    public Camera mainCamera;

    // Le composant qui gère le flux vidéo (trouvé automatiquement ou assigné manuellement)
    private ARCameraManager arManager;

    void Start()
    {
        // Si non assigné, on le cherche sur la caméra
        if (mainCamera == null) mainCamera = Camera.main;
        arManager = mainCamera.GetComponent<ARCameraManager>();
    }

    // Fonction à appeler depuis un Bouton UI (On Click)
    public void SwitchMode()
    {
        if (arManager == null) return;

        // Inverse l'état actuel (Si activé -> désactive, et vice versa)
        bool isARActive = !arManager.enabled;
        arManager.enabled = isARActive;

        // Change le fond : Noir transparent pour AR, Skybox (ou couleur pleine) pour VR
        if (isARActive)
        {
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = new Color(0, 0, 0, 0); // Noir transparent
        }
        else
        {
            mainCamera.clearFlags = CameraClearFlags.Skybox; // Remet le ciel VR par défaut
            // Ou CameraClearFlags.SolidColor avec une couleur grise/bleue si vous préférez
        }
    }
}
