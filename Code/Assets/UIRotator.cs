using UnityEngine;

public class UIRotator : MonoBehaviour
{
    private Transform trans;
    private Vector3 offset = new Vector3(0, 180, 0);
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        trans = GameObject.Find("Main Camera").GetComponent<Transform>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(trans);
        transform.Rotate(offset);
    }
}
