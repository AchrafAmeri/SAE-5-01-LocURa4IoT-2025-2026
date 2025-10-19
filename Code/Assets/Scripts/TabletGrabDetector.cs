using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class TabletGrabDetector : MonoBehaviour
{
    private XRGrabInteractable grabInteractable;

    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnGrab);
        }
        else
        {
            Debug.LogWarning("Aucun XRGrabInteractable trouvé sur la tablette !");
        }
    }

    private void OnGrab(SelectEnterEventArgs args)
    {
        // Quand la tablette est saisie on appelle OnTabletGrabbed() du tuto
        var tutorial = FindObjectOfType<TutorialManager>();
        if (tutorial != null)
        {
            Debug.Log("Tablette attrapée !");
        }
        else
        {
            Debug.LogWarning("TutorialManager non trouvé dans la scène !");
        }
    }
}
