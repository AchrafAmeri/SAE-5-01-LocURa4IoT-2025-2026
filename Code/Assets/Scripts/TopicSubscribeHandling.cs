using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
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
    [SerializeField] ObjectFactory objectFactory; 
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

            else if (topic.Contains("setup"))
            {

                string shape = "cube";

                Match match = Regex.Match(payload, @"\""shape\""\s*:\s*\""(\w+)\""");

                if (match.Success)
                {
                    shape = match.Groups[1].Value;
                }
                await NodeGenerator.CreateNewNode(topic, shape);
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
            else if (topic.Contains("furniture"))
            {
                if (objectFactory != null)
                {
                    await objectFactory.CreateOrUpdateFromPayload(topic, payload);
                }
                else
                {
                    Debug.LogWarning("ObjectFactory n'est pas assigné dans l'inspecteur !");
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

        try
        {
            // S'abonne à tout ce qui commence par furniture (ex: furniture/salon/table)
            await MqttManager.SubscribeAsync("furniture/#");
            Debug.Log("Abonné au topic furniture/#");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
}

