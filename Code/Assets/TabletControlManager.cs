using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.InputSystem;
using System.Collections;

public class TabletControlManager : MonoBehaviour
{
    [Header("Tablette et Physique")]
    [SerializeField] private GameObject tablet;
    [SerializeField] private XRGrabInteractable grabInteractable;
    [SerializeField] private Rigidbody tabletRigidbody;

    [Header("Entrées et Références")]
    [SerializeField] private InputActionReference teleportButtonAction;
    [SerializeField] private Transform leftControllerTransform;

    [Header("Réglages")]
    [SerializeField] private Vector3 teleportOffset = new Vector3(0, 0, 0.15f);
    [SerializeField] private double doubleClickTimeWindow = 0.3;

    private double lastClickTime = 0;
    // Variable pour suivre si la tablette est actuellement tenue
    private bool isBeingHeld = false;

    void Start()
    {
        if (tablet == null) tablet = gameObject;
        if (grabInteractable == null) grabInteractable = tablet.GetComponent<XRGrabInteractable>();
        if (tabletRigidbody == null) tabletRigidbody = tablet.GetComponent<Rigidbody>();

        if (teleportButtonAction != null && teleportButtonAction.action != null)
        {
            teleportButtonAction.action.performed += OnButtonPressed;
            teleportButtonAction.action.Enable();
        }

        // Écoute des événements de grab/release
        if (grabInteractable != null)
        {
            // Quand on attrape la tablette
            grabInteractable.selectEntered.AddListener(OnTabletGrabbed);
            // Quand on la lâche
            grabInteractable.selectExited.AddListener(OnTabletReleased);
        }
    }

    private void OnButtonPressed(InputAction.CallbackContext context)
    {
        // CONDITION : Si la tablette est déjà tenue, on ignore le double-clic
        if (isBeingHeld) return;

        double currentTime = Time.timeAsDouble;
        if ((currentTime - lastClickTime) < doubleClickTimeWindow)
        {
            TeleportTabletToHand();
        }
        lastClickTime = currentTime;
    }

    // Appelé quand la tablette est saisie normalement
    private void OnTabletGrabbed(SelectEnterEventArgs args)
    {
        isBeingHeld = true;
    }

    // Appelé quand la tablette est lâchée
    private void OnTabletReleased(SelectExitEventArgs args)
    {
        isBeingHeld = false;
        // Ton code existant pour la physique
        StartCoroutine(EnsurePhysicsEnabled());
    }

    private void TeleportTabletToHand()
    {
        if (leftControllerTransform == null) return;

        // Note : On ne vérifie plus grabInteractable.isSelected ici 
        // car isBeingHeld empêche déjà d'arriver jusqu'ici

        StartCoroutine(TeleportAndGrabSequence());
    }

    private IEnumerator TeleportAndGrabSequence()
    {
        yield return new WaitForEndOfFrame();

        tabletRigidbody.isKinematic = false;
        tabletRigidbody.linearVelocity = Vector3.zero;
        tabletRigidbody.angularVelocity = Vector3.zero;

        tabletRigidbody.isKinematic = true;

        tablet.transform.position = leftControllerTransform.TransformPoint(teleportOffset);
        tablet.transform.rotation = leftControllerTransform.rotation;

        yield return new WaitForFixedUpdate();

        IXRSelectInteractor interactor = leftControllerTransform.GetComponentInChildren<IXRSelectInteractor>();
        if (interactor != null && grabInteractable != null)
        {
            var manager = grabInteractable.interactionManager;
            manager.SelectEnter(interactor, grabInteractable);
        }

        yield return new WaitForEndOfFrame();

        if (!grabInteractable.isSelected)
        {
            tabletRigidbody.isKinematic = false;
        }
    }

    private IEnumerator EnsurePhysicsEnabled()
    {
        yield return new WaitForEndOfFrame();

        if (tabletRigidbody != null && tabletRigidbody.isKinematic)
        {
            tabletRigidbody.isKinematic = false;
        }
    }

    void OnDestroy()
    {
        if (teleportButtonAction != null && teleportButtonAction.action != null)
        {
            teleportButtonAction.action.performed -= OnButtonPressed;
        }

        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnTabletGrabbed);
            grabInteractable.selectExited.RemoveListener(OnTabletReleased);
        }
    }
}