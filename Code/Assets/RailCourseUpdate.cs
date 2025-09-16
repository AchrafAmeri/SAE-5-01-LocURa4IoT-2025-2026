using UnityEngine;



public class RailCourseUpdate : MonoBehaviour
{
    Animator Animator;
    private void Start()
    {
        Animator = GetComponent<Animator>();
    }

    public void Handleupdate(string payload)
    {
        //Debug.Log(payload);
        courseIndication CourseInfo = JsonUtility.FromJson<courseIndication>(payload.ToString());
        Animator.SetFloat("course", CourseInfo.course);
    }
}
