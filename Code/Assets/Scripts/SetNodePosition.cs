using System;
using UnityEngine;

public class NodePosition
{

    public float x;
    public float y;
    public float z;
    public float orientation;
    public string type;
    public string color;
    public string UID;
}
public class SetNodePosition : MonoBehaviour
{
    [SerializeField] public string Node_id;
    [SerializeField] MQTT mqttManager;
    private GameObject Node;
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
            Node.GetComponent<MeshRenderer>().material = DisconnectedMat;
            init = false;
        }
    }
    
    public void Handleupdate(string payload)
    {
        NodePosition InfoPosition = JsonUtility.FromJson<NodePosition>(payload);
        Node.transform.position = new Vector3(-InfoPosition.x, InfoPosition.z, -InfoPosition.y);
        Node_id = InfoPosition.UID;

    }

    public void HandleConnectionUpdate(string payload)
    {
        if (!init)
        {
            Node.GetComponent<MeshRenderer>().material = ConnectedMat;
            init = true;
        }

        targetTime = 120f;
    }

    private void AddLocalisationListener()
    {
        foreach (TopicEvent topicElement in MessageDetection.topicEventPairs)
        {
            if (topicElement.topic == $"localisation/{Node.name}/setup")
            {
                try
                {
                    topicElement.OnMessageReceived.AddListener(Handleupdate);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
                return;
            }
        }
    }

    public void AddConnectionListener()
    {
        foreach (TopicEvent topicElement in MessageDetection.topicEventPairs)
        {
            if (topicElement.topic == $"testbed/node/{Node.name}/out")
            {
                try
                {
                    topicElement.OnMessageReceived.AddListener(HandleConnectionUpdate);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
                Node.GetComponent<MeshRenderer>().material = ConnectedMat;
                init = true;
                targetTime = 120f;
                return;
            }
        }
    }

    public void SetupMQTT(MQTT mqtt, TopicSubscribeHandling Handler, Material connected)
    {
        mqttManager = mqtt;
        MessageDetection = Handler;
        Node = gameObject;
        ConnectedMat = connected;
        DisconnectedMat = Node.GetComponent<MeshRenderer>().material;
        AddLocalisationListener();
    }

}
