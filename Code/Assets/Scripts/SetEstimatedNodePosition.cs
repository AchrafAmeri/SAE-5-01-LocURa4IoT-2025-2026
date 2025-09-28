using UnityEngine;
using System;


public class EstimatedNodePosition
{
    public string node_id;
    public string[] anchors;
    public float x;
    public float y;
    public float z;
    public float orientation;
    public string color;
    public string type;
    public string source;
    public string timestamp;
    public string error;
}


public class SetEstimatedNodePosition : MonoBehaviour
{

    public string node_id;
    private GameObject Node;
    private Transform ParentNode;
    private GameObject realNode = null;
    private Material realNodeMat;
    private string[] anchors_id;
    private bool init = false;
    [SerializeField] TopicSubscribeHandling MessageDetection;
    [SerializeField] DrawLine DrawLine;
    [SerializeField] LineRenderer lineEstimation;
    [SerializeField] LineRenderer lineCommunication;
    [SerializeField] Transform ParentLine;
    [SerializeField] Transform ParentComm;


    public void Handleupdate(string payload)
    {
        string[] previousAnchor_ID = anchors_id;
        EstimatedNodePosition InfoPosition = JsonUtility.FromJson<EstimatedNodePosition>(payload);
        
        Node.transform.position = new Vector3(-InfoPosition.x, InfoPosition.z, -InfoPosition.y);
        node_id = InfoPosition.node_id;
        anchors_id = InfoPosition.anchors;
        if (!init)
        {
            foreach (Transform child in ParentNode)
            {

                if (child.GetComponent<SetNodePosition>() != null)
                {

                    if (child.GetComponent<SetNodePosition>().Node_id == node_id)
                    {
                        try
                        {
                            realNode = child.gameObject;
                            realNode.GetComponent<MeshRenderer>().material = realNodeMat;
                            DrawLine.DrawEstimationLine(realNode, gameObject, lineEstimation);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e);
                        }
                    }


                }

                else if (child.GetComponent<SetMobileNodePosition>() != null)
                {
                    if (child.GetComponent<SetMobileNodePosition>().Node_id == node_id)
                    {
                        try
                        {
                            realNode = child.gameObject;
                            realNode.GetComponent<MeshRenderer>().material = realNodeMat;
                            DrawLine.DrawEstimationLine(realNode, gameObject, lineEstimation);
                        }
                        catch (Exception e)
                        {
                            Debug.LogError(e);
                        }
                    }
                }

                else
                {
                    Debug.LogWarning($"Le node {child.name} ne correspond à aucune des conditions");
                }
                
            }
            init = true;

        }

        else 
        {
                       
            if (realNode != null)
            {
                
                DrawLine.DrawEstimationLine(realNode, gameObject, lineEstimation);
               
            }
 
                
            else if (realNode == null & lineEstimation != null)
            {
                Destroy(ParentLine.Find(lineEstimation.name).gameObject);
            }

            else
            {
                foreach (Transform comm in ParentComm)
                {
                    if (comm.name.Contains(gameObject.name))
                    {
                        Destroy(comm.gameObject);
                    }
                }

                foreach (Transform child in ParentNode)
                {
                    if (child.GetComponent<SetNodePosition>() != null)
                    {
                        foreach (string anchor in anchors_id)
                        {
                            string start = "0x";
                            int indexDebut = anchor.IndexOf(start) + start.Length;
                            string anchor_id = anchor.Substring(indexDebut);
                            if (child.GetComponent<SetNodePosition>().Node_id == anchor_id)
                            {
                                LineRenderer communication = Instantiate(lineCommunication);
                                communication.name = $"Communication{gameObject.name}-{child.name}";
                                communication.transform.SetParent(ParentComm);
                                DrawLine.DrawEstimationLine(child.gameObject, gameObject, communication);
                            }
                        }
                    }

                    else if (child.GetComponent<SetMobileNodePosition>() != null)
                    {
                        foreach (string anchor in anchors_id)
                        {
                            string start = "0x";
                            int indexDebut = anchor.IndexOf(start) + start.Length;
                            string anchor_id = anchor.Substring(indexDebut);
                            if (child.GetComponent<SetMobileNodePosition>().Node_id == anchor_id)
                            {
                                LineRenderer communication = Instantiate(lineCommunication);
                                communication.name = $"Communication{gameObject.name}-{child.name}";
                                communication.transform.SetParent(ParentComm);
                                DrawLine.DrawEstimationLine(child.gameObject, gameObject, communication);
                            }
                        }
                    }
                }
            }

                
        }

    }

    private void AddLocalisationListener()
    {

        foreach (TopicEvent topicElement in MessageDetection.topicEventPairs)
        {
            if (topicElement.topic == $"localisation/{Node.name}/estimation")
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
    public void SetupMQTT(TopicSubscribeHandling Handler, Transform Parent, LineRenderer Estimation, LineRenderer Communication, Transform lineParent, Transform communicationParent, Material Connected)
    {
        MessageDetection = Handler;
        Node = gameObject;
        realNodeMat = Connected;
        ParentNode = Parent;
        ParentLine = lineParent;
        ParentComm = communicationParent;

        DrawLine = Parent.GetComponent<DrawLine>();
        lineEstimation = Instantiate(Estimation);
        lineEstimation.transform.SetParent(ParentLine);
        lineEstimation.name = $"Estimation{gameObject.name}";
        lineEstimation.positionCount = 2;
        
        lineCommunication = Communication;
        lineCommunication.positionCount = 2;

        AddLocalisationListener();
    }
}
