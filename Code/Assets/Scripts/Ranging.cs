using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;


public class RangingIndication
{
    public string initiator;
    public string target;
    public string protocol;
    public float range;
    public float seqnum;
    public float timestamp;
    public List<List<float>> Initiator;
    public float distance;
    public float rangingError;
    

}

public class Ranging : MonoBehaviour
{
    [SerializeField] MQTT mqttManager;
    [SerializeField] TopicSubscribeHandling MessageDetection;
    [SerializeField] GameObject ParentEstimNode;
    [SerializeField] LineRenderer LineCommunication;
    [SerializeField] LineRenderer LineError;
    [SerializeField] DrawLine DrawLine;
    [SerializeField] GameObject RangeSphere;


    public GameObject anchor;
    public GameObject RealNode;
    public GameObject EstimatedNode;

    private string nodeID;
    public float targetTime = 1f;

    private void Update()
    {
        targetTime -= Time.deltaTime;

        if (targetTime < 0)
        {
            LineCommunication.gameObject.SetActive(false);
            LineError.gameObject.SetActive(false);
            RangeSphere.gameObject.SetActive(false);
        }
    }

    public void Handleupdate(string payload)
    {
        targetTime = 1f;
        RangingIndication rangingInfo = JsonUtility.FromJson<RangingIndication>(payload);

        if (DrawLine.repRect)
        {
            LineCommunication.gameObject.SetActive(true);
            LineError.gameObject.SetActive(true);
            RangeSphere.gameObject.SetActive(false);

            if (RealNode != null)
            {
                if (DrawLine.errorRangingParRapportNodeRéel)
                {
                    DrawLine.DrawRangingLine(anchor, RealNode, LineCommunication, LineError, rangingInfo.distance, rangingInfo.range);
                }

                else if (DrawLine.errorRangingParRapportNodeEstim & EstimatedNode != null)
                {
                    DrawLine.DrawRangingLine(anchor, EstimatedNode, LineCommunication, LineError, rangingInfo.distance, rangingInfo.range);
                }

                else if (DrawLine.errorRangingParRapportNodeEstim & EstimatedNode == null)
                {
                    foreach (Transform child in ParentEstimNode.transform)
                    {
                        if (child.GetComponent<SetEstimatedNodePosition>().node_id == nodeID)
                        {
                            EstimatedNode = child.gameObject;
                            break;
                        }

                        if (EstimatedNode != null)
                        {
                            DrawLine.DrawRangingLine(anchor, EstimatedNode, LineCommunication, LineError, rangingInfo.distance, rangingInfo.range);
                        }
                    }
                }
            }

            else
            {
                if (EstimatedNode != null)
                {
                    DrawLine.DrawRangingLine(anchor, EstimatedNode, LineCommunication, LineError, rangingInfo.distance, rangingInfo.range);
                }
            }
            
        }


        else if (DrawLine.repSphere)
        {
            LineCommunication.gameObject.SetActive(false);
            LineError.gameObject.SetActive(false);
            if(RealNode != null)
            {
                RangeSphere.SetActive(true);
                RangeSphere.transform.localScale = new Vector3(2 * rangingInfo.range, 2 * rangingInfo.range, 2 * rangingInfo.range);

            }
        }
    }

    private void TopicListenner(string TopicName)
    {
        foreach (TopicEvent topicElement in MessageDetection.topicEventPairs)
        {
            if (topicElement.topic == TopicName)
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

    public Task SetupMQTT(MQTT mqtt, TopicSubscribeHandling Handler, string topic, GameObject ParentNodes, GameObject ParentEstimated, LineRenderer Communication, LineRenderer Error, GameObject ParentLine, GameObject sphere)
    {
        mqttManager = mqtt;
        MessageDetection = Handler;
        TopicListenner(topic);
        nodeID = topic.Substring(8, 4);
        anchor = gameObject;
        foreach(Transform child in ParentNodes.transform)
        {
            if (child.GetComponent<SetNodePosition>() != null!)
            {
                if (child.GetComponent<SetNodePosition>().Node_id == nodeID)
                {
                    RealNode = child.gameObject;
                    break;
                }
                
            }

            else if (child.GetComponent<SetMobileNodePosition>() != null!)
            {
                if (child.GetComponent<SetMobileNodePosition>().Node_id == nodeID)
                {
                    RealNode = child.gameObject;
                    break;
                }
            }
        }
        ParentEstimNode = ParentEstimated;
        foreach (Transform child in ParentEstimNode.transform)
        {
            if(child.GetComponent<SetEstimatedNodePosition>().node_id == nodeID)
            {
                EstimatedNode = child.gameObject;
                break;
            }

        }

        LineCommunication = Instantiate(Communication);
        LineCommunication.name = $"Communication {topic.Substring(8, 4)}-{topic.Substring(13, 4)}";
        LineCommunication.positionCount = 2;
        LineCommunication.transform.SetParent(ParentLine.transform);
        
        LineError = Instantiate(Error);
        LineError.name = $"Error {topic.Substring(8, 4)}-{topic.Substring(13, 4)}";
        LineError.positionCount = 2;
        LineError.transform.SetParent(ParentLine.transform);

        DrawLine = ParentNodes.GetComponent<DrawLine>();

        RangeSphere = Instantiate(sphere);
        RangeSphere.name = $"Range {topic.Substring(8, 4)}-{topic.Substring(13, 4)}";
        RangeSphere.transform.SetParent(gameObject.transform);
        RangeSphere.transform.position = gameObject.transform.position;
        RangeSphere.GetComponent<MeshRenderer>().material.color = DrawLine.ShereColor[0];
        DrawLine.ShereColor.Remove(DrawLine.ShereColor[0]);

        return Task.CompletedTask;
    }
}
