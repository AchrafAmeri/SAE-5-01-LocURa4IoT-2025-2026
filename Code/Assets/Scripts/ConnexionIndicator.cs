using System;
using UnityEngine;
using UnityEngine.UI;

public class ConnexionIndicator : MonoBehaviour
{

    Image indicator;
    [SerializeField] Color ConnectedColor = Color.green;
    [SerializeField] Color DisconnectedColor = Color.red;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        indicator = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateConnectionIndicator(bool state)
    {
        Debug.Log("ConnectionStateChange");
        if (state)
        {
            indicator.color = ConnectedColor;

        }
        else
        {
            indicator.color = DisconnectedColor;
        }
    }
}
