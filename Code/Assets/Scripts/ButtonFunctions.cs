using UnityEngine;
using System;
using TMPro;
public class ButtonFunctions : MonoBehaviour
{
    [SerializeField] MQTT mqttManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }
    private bool isPublishing = false;
    // Update is called once per frame
    void Update()
    {
        
    }
    public async void Connect()
    {
        if (isPublishing) return;
        isPublishing = true;
        try
        {
        await mqttManager.ConnectAsync();
        }
        catch(Exception ex)
        {
            Debug.LogError($"Erreur lors du publish : {ex.Message}");
        }
        try
        {
            await mqttManager.SubscribeAsync("rail/1/course/indication");
        }
        catch(Exception ex) {

            Debug.LogError($"Erreur lors du subscribe : {ex.Message}");
        }
        Debug.Log("Connexion Terminée");
        isPublishing = false;
    }
    public async void Forward()
    {
        if (isPublishing) return;
        isPublishing = true;
        try
        {
            await mqttManager.PublishAsync("rail/1/course/request", "+0.25");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Erreur lors du publish : {ex.Message}");
        }

        isPublishing = false;
    }
    public async void Back()
    {
        if (isPublishing) return;
        isPublishing = true;
        try
        {
        await mqttManager.PublishAsync("rail/1/course/request", "-0.25");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Erreur lors du publish : {ex.Message}");
        }
        isPublishing = false;
    }
    public async void Init()
    {
        if (isPublishing) return;
        isPublishing = true;
        try
        {
            await mqttManager.PublishAsync("rail/1/init/request", "1");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Erreur lors du publish : {ex.Message}");
        }
        isPublishing = false;
    }
    public async void Course0()
    {
        if (isPublishing) return;
        isPublishing = true;
        try
        {
            await mqttManager.PublishAsync("rail/1/course/request", "0");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Erreur lors du publish : {ex.Message}");
        }
        isPublishing = false;
    }
    public async void Course7()
    {
        if (isPublishing) return;
        isPublishing = true;
        try
        {
            await mqttManager.PublishAsync("rail/1/course/request", "7");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Erreur lors du publish : {ex.Message}");
        }
        isPublishing = false;
    }

}
