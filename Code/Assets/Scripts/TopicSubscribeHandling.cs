using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

[Serializable]
public class PayloadEvent : UnityEngine.Events.UnityEvent<string>{}

[Serializable]
public class TopicEvent
{
    public int id;
    public string topic;
    public PayloadEvent OnMessageReceived;

    public TopicEvent(int id, string topic)
    {
        this.id = id;
        this.topic = topic;
        OnMessageReceived = new PayloadEvent();
    }
}


public class TopicSubscribeHandling : MonoBehaviour
{
    [SerializeField] public List<TopicEvent> topicEventPairs;
    [SerializeField] MQTT MqttManager;
    [SerializeField] TMP_InputField IpAdress;
    [SerializeField] TMP_InputField Port;
    [SerializeField] TMP_InputField UserName;
    [SerializeField] TMP_InputField Password;
    [SerializeField] NodeGenerator NodeGenerator;
    [SerializeField] GameObject DoorGhost;
    [SerializeField] GameObject Nodes;
    [SerializeField] Toggle TLS;
    public async Task<Task> HandleIncomingMessage(string topic, string payload)
    {
        bool findtopic = false;
        foreach (var pair in topicEventPairs)
        {
            if (pair.topic == topic)
            {
                pair.OnMessageReceived?.Invoke(payload);
                findtopic = true;
                break;
            }
        }
        if (!findtopic) 
        {
            topicEventPairs.Add(new TopicEvent(topicEventPairs.Count, topic));

            if(topic.Contains("mobile")){
               await NodeGenerator.CreateNewMobileNode(topic);
            }

            else if (topic.Contains("setup")){
               await NodeGenerator.CreateNewNode(topic);
            }

            else if (topic.Contains("estimation"))
            {
                await NodeGenerator.CreateNewEstimatedNode(topic);
            }

            else if (topic.Contains("api/3/room"))
            {
                Transform Door;
                string start = "api/3/room/";
                string end = "/sensor/TS0203/id/1/indication";
                int indexDebut = topic.IndexOf(start) + start.Length;
                int indexFin = topic.IndexOf(end);
                string DoorName = topic.Substring(indexDebut, indexFin - indexDebut);
                Door = DoorGhost.transform.Find(DoorName);

                if (Door != null)
                {
                    await Door.GetComponent<DoorSate>().SetupDoorState();
                }

                else
                {
                    Debug.LogWarning($"La porte {DoorName} n'est pas dans la scene");
                }
            }

            else if (topic.Contains("ranging"))
            {
                await NodeGenerator.CreateNewRanging(topic);
            }

            else if (topic.Contains("testbed/node/"))
            {
                string start = "testbed/node/";
                string end = "/out";
                int indexDebut = topic.IndexOf(start) + start.Length;
                int indexFin = topic.IndexOf(end);
                string nodeName = topic.Substring(indexDebut, indexFin - indexDebut);
                Debug.Log($"Node name = {nodeName}");
                Transform node = Nodes.transform.Find(nodeName);
                if (node.GetComponent<SetNodePosition>() != null)
                {
                    node.GetComponent<SetNodePosition>().AddConnectionListener();
                }
                
            }

            else
            {
                Debug.LogWarning($"Type de node non pris en charge: {topic}");
            }
            topicEventPairs[topicEventPairs.Count - 1].OnMessageReceived?.Invoke(payload);
        }
        return Task.CompletedTask;
    }


    public async void ConnectionMQTT()
    {

        if (IpAdress.text == "" && Port.text == "" && UserName.text == "" && Password.text == "")
        {
            Debug.Log("Connection");
            await MqttManager.ConnectAsync();
        }

        else
        {

            if (TLS.isOn)
            {
                Debug.Log("Connection");
                await MqttManager.ConnectTLSAsync(IpAdress.text, int.Parse(Port.text), UserName.text, Password.text);
            }
            else
            {
                Debug.Log("Connection");
                await MqttManager.ConnectAsync(IpAdress.text, int.Parse(Port.text), UserName.text, Password.text);
            }
        }
        
        Debug.Log("Subscribe");
        foreach (var pair in topicEventPairs)
        {
            try
            {
                await MqttManager.SubscribeAsync(pair.topic);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        try
        {
            await MqttManager.SubscribeAsync("localisation/+/#");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        try
        {
            await MqttManager.SubscribeAsync("api/3/room/+/sensor/TS0203/id/1/indication");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        try
        {
            await MqttManager.SubscribeAsync("ranging/+/+/indication");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

        try
        {
            await MqttManager.SubscribeAsync("testbed/node/+/out");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
}

