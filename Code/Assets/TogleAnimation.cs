using UnityEngine;

public class TogleAnimation : MonoBehaviour
{
    public Animator animator;
    private bool isGoingUp = false;

    public void Toggle()
    {
        isGoingUp = !isGoingUp;
        float newSpeed = isGoingUp ? 1f : -1f;
        animator.SetFloat("Speed", newSpeed);
    }
    private void Start()
    {
        animator = GetComponent<Animator>();
    }
    public bool getIsgoingUp()
    {
        return isGoingUp;
    }


}
