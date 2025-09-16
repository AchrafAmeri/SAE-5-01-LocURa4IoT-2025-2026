using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class NodeGenerator : MonoBehaviour
{
    [SerializeField] GameObject MobileNodePrefab;
    [SerializeField] GameObject NodePrefab;
    [SerializeField] GameObject CanvaPrefab;
    [SerializeField] GameObject EstimatedNodePrefab;
    [SerializeField] GameObject EstimatedParent;
    [SerializeField] GameObject RangingPrefab;
    [SerializeField] GameObject RangeSpherePrefab;

    [SerializeField] LineRenderer lineEstimationPrefab;
    [SerializeField] LineRenderer lineCommunicationPrefab;
    [SerializeField] LineRenderer lineErrorPrefab;
    [SerializeField] GameObject lineParent;
    [SerializeField] GameObject rangingLineParent;
    [SerializeField] GameObject communicationLineParent;
    [SerializeField] Material ConnectedNodeMaterial;

    [SerializeField] MQTT MqttManager;
    [SerializeField] TopicSubscribeHandling TopicList;


    public Task CreateNewNode(string name)
    {
        string start = "localisation/";
        string end = "/setup";
        int indexDebut = name.IndexOf(start) + start.Length;
        int indexFin = name.IndexOf(end);
        string NomDuNode = name.Substring(indexDebut, indexFin-indexDebut);

        GameObject NewNode = Instantiate(NodePrefab);
        NewNode.transform.SetParent(gameObject.transform);
        NewNode.name = NomDuNode;
        NewNode.GetComponent<SetNodePosition>().SetupMQTT(MqttManager, TopicList, ConnectedNodeMaterial);

        GameObject NewCanva = Instantiate(CanvaPrefab);
        NewCanva.transform.SetParent(NewNode.transform);
        NewCanva.GetComponent<TextMeshProUGUI>().text = NomDuNode;

        return Task.CompletedTask;
    }

    public Task CreateNewMobileNode(string name)
    {
        string start = "localisation/";
        string end = "/mobile";
        int indexDebut = name.IndexOf(start) + start.Length;
        int indexFin = name.IndexOf(end);
        string NomDuNode = name.Substring(indexDebut, indexFin - indexDebut);

        GameObject NewNode = Instantiate(MobileNodePrefab);
        NewNode.transform.SetParent(gameObject.transform);
        NewNode.name = NomDuNode;
        NewNode.GetComponent<SetMobileNodePosition>().SetupMQTT(MqttManager, TopicList, ConnectedNodeMaterial);

        GameObject NewCanva = Instantiate(CanvaPrefab);
        NewCanva.transform.SetParent(NewNode.transform);
        NewCanva.GetComponent<TextMeshProUGUI>().text = NomDuNode;
        return Task.CompletedTask;

    }

    public Task CreateNewEstimatedNode(string name)
    {
        string start = "localisation/";
        string end = "/estimation";
        int indexDebut = name.IndexOf(start) + start.Length;
        int indexFin = name.IndexOf(end);
        string NomDuNode = name.Substring(indexDebut, indexFin - indexDebut);

        GameObject NewNode = Instantiate(EstimatedNodePrefab);
        NewNode.transform.SetParent(EstimatedParent.transform);
        NewNode.name = NomDuNode;
        NewNode.GetComponent<SetEstimatedNodePosition>().SetupMQTT(MqttManager, TopicList, gameObject.transform, lineEstimationPrefab, lineCommunicationPrefab, lineParent.transform, communicationLineParent.transform, ConnectedNodeMaterial);

        GameObject NewCanva = Instantiate(CanvaPrefab);
        NewCanva.transform.SetParent(NewNode.transform);
        NewCanva.GetComponent<TextMeshProUGUI>().text = NomDuNode;
        return Task.CompletedTask;

    }

    public Task CreateNewRanging(string Topic)
    {
        string AnchorName = Topic.Substring(13, 4);
        string RangingName = $"{Topic.Substring(8, 4)}-{Topic.Substring(13, 4)}";

        GameObject NewRanging = Instantiate(RangingPrefab);
        foreach (Transform node in gameObject.transform)
        {
            if (node.GetComponent<SetNodePosition>() != null)
            {
                if (node.GetComponent<SetNodePosition>().Node_id == AnchorName)
                {
                    NewRanging.transform.SetParent(node.transform);
                    NewRanging.transform.position = node.transform.position;
                }
            }

            else if (node.GetComponent<SetMobileNodePosition>() != null)
            {
                if (node.GetComponent<SetMobileNodePosition>() != null)
                {
                    if (node.GetComponent<SetMobileNodePosition>().Node_id == AnchorName)
                    {
                        NewRanging.transform.SetParent(node.transform);
                        NewRanging.transform.position = node.transform.position;
                    }
                }
            }
        }
        NewRanging.name = RangingName;
        NewRanging.GetComponent<Ranging>().SetupMQTT(MqttManager, TopicList, Topic, gameObject, EstimatedParent, lineCommunicationPrefab, lineErrorPrefab, rangingLineParent, RangeSpherePrefab);
        return Task.CompletedTask;
    }

}