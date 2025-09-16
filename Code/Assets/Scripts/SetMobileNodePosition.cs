using System;
using UnityEngine;

public class MobileNodePosition
{

    public float timestamp;
    public float x;
    public float y;
    public float z;
    public string type;
    public string color;
    public string UID;
}
public class SetMobileNodePosition : MonoBehaviour
{
    public string Node_id;
    [SerializeField] MQTT mqttManager;
    private GameObject MobileNode;
    private bool init = false;
    [SerializeField] TopicSubscribeHandling MessageDetection;
    [SerializeField] Material ConnectedMat;
    private Material DisconnectedMat;
    private float targetTime = 120f;


    private void Update()
    {
        if (init)
        {
            targetTime -= Time.deltaTime;
        }

        if (targetTime < 0 && init)
        {
            MobileNode.GetComponent<MeshRenderer>().material = DisconnectedMat;
            init = false;
        }
    }

    public void SetupMQTT(MQTT mqtt,TopicSubscribeHandling Handler, Material connected)
    {
        mqttManager = mqtt;
        MessageDetection = Handler;
        MobileNode = gameObject;
        ConnectedMat = connected;
        DisconnectedMat = MobileNode.GetComponent<MeshRenderer>().material;
        AddLocalisationListener();
    }



    public void Handleupdate(string payload)
    {
        MobileNodePosition InfoPosition = JsonUtility.FromJson<MobileNodePosition>(payload);
        MobileNode.transform.position = new Vector3(-InfoPosition.x, InfoPosition.z, -InfoPosition.y);
        Node_id = InfoPosition.UID;
    }

    public void HandleConnectionUpdate(string payload)
    {
        if (!init)
        {
            MobileNode.GetComponent<MeshRenderer>().material = ConnectedMat;
            init = true;
        }

        targetTime = 120f;
    }

    private void AddLocalisationListener()
    {
        foreach (TopicEvent topicElement in MessageDetection.topicEventPairs)
        {
            if (topicElement.topic == $"localisation/{MobileNode.name}/mobile")
            {
                try
                {
                    topicElement.OnMessageReceived.AddListener(Handleupdate);
                }
                catch (Exception e) 
                { 
                    Debug.LogException(e);
                }
                return;
            }
        }
    }

    public void AddConnectionListener()
    {
        foreach (TopicEvent topicElement in MessageDetection.topicEventPairs)
        {
            if (topicElement.topic == $"testbed/node/{MobileNode.name}/out")
            {
                try
                {
                    topicElement.OnMessageReceived.AddListener(HandleConnectionUpdate);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
                MobileNode.GetComponent<MeshRenderer>().material = ConnectedMat;
                init = true;
                targetTime = 120f;
                return;
            }
        }
    }
}
