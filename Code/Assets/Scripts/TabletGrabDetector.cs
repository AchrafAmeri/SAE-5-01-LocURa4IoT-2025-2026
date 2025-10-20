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
        // Quand la tablette est saisie on appelle OnGrab() du tuto
        var tutorial = FindObjectOfType<TabletTutorialManager>();
        if (tutorial != null)
        {
            tutorial.OnGrab();
        }
        else
        {
            Debug.LogWarning("TabletTutorialManager non trouvé dans la scène !");
        }
    }
}
