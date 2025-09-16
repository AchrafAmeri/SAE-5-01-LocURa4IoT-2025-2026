using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System;

public class SliderValueUpdate : MonoBehaviour
{
    [SerializeField] Slider ValueSlider;
    [SerializeField] PublishData PublishData;
    [SerializeField] TMP_Text BaseSliderValue;
    [SerializeField] MQTTPublish PublishManager;
    TMP_Text TextToUpdate;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TextToUpdate = GetComponent<TMP_Text>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetDefaultValue()
    {
        ValueSlider.value = float.Parse(BaseSliderValue.text);
        ValueUpdate();
    }
    public void ValueUpdate()
    {
        TextToUpdate.text = ValueSlider.value.ToString("F2");
    }
    public void PublishValue()
    {
        PublishData.message = ValueSlider.value.ToString("F2");
        PublishManager.publishMessage(PublishData);
    }

}

