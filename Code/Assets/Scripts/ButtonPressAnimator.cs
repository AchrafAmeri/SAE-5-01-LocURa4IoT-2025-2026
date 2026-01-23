using UnityEngine;
using UnityEngine.InputSystem;

public class ButtonPressAnimator : MonoBehaviour
{
    [Header("Input Action")]
    public InputActionReference buttonAction;

    [Header("Animation Settings")]
    public Vector3 pressDirection = new Vector3(0, -1, 0);
    public float pressDistance = 0.0015f;
    public float smoothSpeed = 10f;

    private Vector3 basePosition;
    private Vector3 targetPosition;

    void Start()
    {
        basePosition = transform.localPosition;
        targetPosition = basePosition;
    }

    void Update()
    {
        if (buttonAction == null || buttonAction.action == null)
            return;

        float pressed = buttonAction.action.ReadValue<float>();
        targetPosition = basePosition + pressDirection.normalized * pressDistance * pressed;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * smoothSpeed);
    }
}
