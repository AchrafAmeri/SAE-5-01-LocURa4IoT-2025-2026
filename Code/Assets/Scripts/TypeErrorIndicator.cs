using System;
using UnityEngine;
using UnityEngine.UI;

public class TypeErrorIndicator : MonoBehaviour
{

    private bool indicatorState = false;
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

    private void UpdateIndicator(bool state)
    {
        if (state)
        {
            indicator.color = ConnectedColor;

        }
        else
        {
            indicator.color = DisconnectedColor;
        }
    }

    public void IndicatorState()
    {
        if (indicatorState)
        {
            indicatorState = false;
            UpdateIndicator(indicatorState);
        }

        else 
        {
            indicatorState = true;
            UpdateIndicator(indicatorState);
        }
    }
}
