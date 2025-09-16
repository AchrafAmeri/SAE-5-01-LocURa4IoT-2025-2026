using TMPro;
using UnityEngine;


public class courseIndication
    {
    public float timestamp;
    public float course;
    }


public class CourseValueHandler : MonoBehaviour
{
    [SerializeField] TMP_Text coursetext;


    private void Start()
    {
        coursetext = GetComponent<TextMeshProUGUI>();
    }

    public void Handleupdate(string payload)
    {
        courseIndication CourseInfo = JsonUtility.FromJson<courseIndication>(payload);
        coursetext.text = CourseInfo.course.ToString();
    }
}
