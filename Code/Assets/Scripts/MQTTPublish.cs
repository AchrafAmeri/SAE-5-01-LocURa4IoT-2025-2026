using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System;


public class MQTTPublish : MonoBehaviour
{
    [SerializeField] MQTT mqttManager;

    public List<PublishData> PublishData;

    private bool isPublishing = false;

    async public void PublishFromList(int index)
    {
        if (isPublishing) return;
        isPublishing = true;
        try
        {
            await mqttManager.PublishAsync(PublishData[index].topic, PublishData[index].message);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Erreur lors du publish : {ex.Message}");
        }
        isPublishing = false;
    }
    async public void publishMessage(PublishData data)
    {
        if (isPublishing) return;
        isPublishing = true;
        try
        {
            await mqttManager.PublishAsync(data.topic,data.message);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Erreur lors du publish : {ex.Message}");
        }
        isPublishing = false;
    }
}
