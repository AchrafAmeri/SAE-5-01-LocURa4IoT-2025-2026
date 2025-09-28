using System.Threading.Tasks;
using TMPro;
using UnityEngine;

/*
 * Ce scripte à pour but de créer dynamiquement 
 * chacun des nodes dont on reçoit pour la première 
 * fois leurs informations de localisation
 * */
public class NodeGenerator : MonoBehaviour
{
    //initialisation des variable utilisées
    [SerializeField] GameObject MobileNodePrefab; //Le prefab utilisé pour les nodes mobiles
    [SerializeField] GameObject NodePrefab; //Le prefab utilisé pour les nodes fixes
    [SerializeField] GameObject CanvaPrefab; //Le prefab utilisé pour les canva servant à afficher les nom des nodes
    [SerializeField] GameObject EstimatedNodePrefab; //Le prefab utilisé pour les nodes estimés
    [SerializeField] GameObject EstimatedParent; //L'Empty qui sera le parent de tous les nodes estimés
    [SerializeField] GameObject RangingPrefab; //Le prefab utilisé pour les empty qui piloteron le ranging
    [SerializeField] GameObject RangeSpherePrefab; //Le prefab utilisé pour les sphère représentant les mesures de ranging

    [SerializeField] LineRenderer lineEstimationPrefab; //Le prefab utilisé pour lier un node estimé à son node réel
    [SerializeField] LineRenderer lineCommunicationPrefab; //Le prefab utilisé pour représenter une mesure de ranging par une ligne entre les 2 ndes communiquant
    [SerializeField] LineRenderer lineErrorPrefab; //Le prefab utilisé pour représenter l'erreur de ranging
    [SerializeField] GameObject lineParent; //L'Empty parent de line reliant les nodes réels et estimés
    [SerializeField] GameObject rangingLineParent; //L'Empty parent des line de mesure de ranging et d'erreur
    [SerializeField] GameObject communicationLineParent; //L'Empty parent des line de représentation de communications (utile que pour les nodes estimé qui n'ont pas de position réelles connues)
    [SerializeField] Material ConnectedNodeMaterial; //le matériaux des node connectés pour une expérience

    [SerializeField] MQTT MqttManager; //Le scripte MQTT.cs
    [SerializeField] TopicSubscribeHandling TopicList; //La liste des topic auquels on est abonnée définit dans TopicSubscribeHandeling.cs


    //Création d'un node fixe
    public Task CreateNewNode(string name)
    {
        //On détermie le nom du node à partir du nom du topic, on retire donc de ce dernier le début et la fin de la chaine de catactères
        string start = "localisation/";
        string end = "/setup";
        int indexDebut = name.IndexOf(start) + start.Length;
        int indexFin = name.IndexOf(end);
        string NomDuNode = name.Substring(indexDebut, indexFin-indexDebut);

        //Création du node dans la scène
        GameObject NewNode = Instantiate(NodePrefab); //On clone le préfab
        NewNode.transform.SetParent(gameObject.transform); //On détermine son parent
        NewNode.name = NomDuNode; //On le nome
        NewNode.GetComponent<SetNodePosition>().SetupMQTT(TopicList, ConnectedNodeMaterial); //On initialise le scripte SetNodePosition.cs qui lui est associé qui pilote en temps réel la position du node

        //création de son canva pour afficher son nom
        GameObject NewCanva = Instantiate(CanvaPrefab);
        NewCanva.transform.SetParent(NewNode.transform);
        NewCanva.GetComponent<TextMeshProUGUI>().text = NomDuNode;

        return Task.CompletedTask;
    }

    //Création d'un node mobile
    //Pour commentaires détaillés voir la fonction CreateNewNode qui est très semblable
    public Task CreateNewMobileNode(string name)
    {
        //détermination du nom
        string start = "localisation/";
        string end = "/mobile";
        int indexDebut = name.IndexOf(start) + start.Length;
        int indexFin = name.IndexOf(end);
        string NomDuNode = name.Substring(indexDebut, indexFin - indexDebut);

        //Création du node dans la scène
        GameObject NewNode = Instantiate(MobileNodePrefab);
        NewNode.transform.SetParent(gameObject.transform);
        NewNode.name = NomDuNode;
        NewNode.GetComponent<SetMobileNodePosition>().SetupMQTT(TopicList, ConnectedNodeMaterial);

        //création de son canva pour afficher son nom
        GameObject NewCanva = Instantiate(CanvaPrefab);
        NewCanva.transform.SetParent(NewNode.transform);
        NewCanva.GetComponent<TextMeshProUGUI>().text = NomDuNode;

        return Task.CompletedTask;

    }

    //Création d'un node estimé
    //Pour commentaires détaillés voir la fonction CreateNewNode qui est très semblable
    public Task CreateNewEstimatedNode(string name)
    {
        //détermination du nom
        string start = "localisation/";
        string end = "/estimation";
        int indexDebut = name.IndexOf(start) + start.Length;
        int indexFin = name.IndexOf(end);
        string NomDuNode = name.Substring(indexDebut, indexFin - indexDebut);

        //Création du node dans la scène
        GameObject NewNode = Instantiate(EstimatedNodePrefab);
        NewNode.transform.SetParent(EstimatedParent.transform);
        NewNode.name = NomDuNode;
        NewNode.GetComponent<SetEstimatedNodePosition>().SetupMQTT(TopicList, gameObject.transform, lineEstimationPrefab, lineCommunicationPrefab, lineParent.transform, communicationLineParent.transform, ConnectedNodeMaterial);

        //création de son canva pour afficher son nom
        GameObject NewCanva = Instantiate(CanvaPrefab);
        NewCanva.transform.SetParent(NewNode.transform);
        NewCanva.GetComponent<TextMeshProUGUI>().text = NomDuNode;
        return Task.CompletedTask;

    }

    //création de l'Empty qui pilote la représentation d'une mesure de ranging associé à un node détecté et un node ancre
    public Task CreateNewRanging(string Topic)
    {
        //Recherche de l'id de l'ancre à partir du nom du topic
        string AnchorId = Topic.Substring(13, 4);
        //Détermination du nom de l'Empty de type {Id du node détecté}-{Id de l'ancre}
        string RangingName = $"{Topic.Substring(8, 4)}-{Topic.Substring(13, 4)}";

        //Création de l'Empty
        GameObject NewRanging = Instantiate(RangingPrefab);
        //Recherche du node ancre associé qui sera le parent de l'empty de ranging
        foreach (Transform node in gameObject.transform)
        {
            /*
             * On veut retrouver l'ancre via son id qui est une varriable public dans les scriptes
             * SetNodePosition des nodes fixes et SetMobileNodePosition des node mobiles.
             * C'est pour cela que l'on cré deux cas, un pour les fixe et un pour les mobile
             * */
            if (node.GetComponent<SetNodePosition>() != null) //Si le node est fixe
            {
                if (node.GetComponent<SetNodePosition>().Node_id == AnchorId) //Si l'id correspond
                {
                    //On a trouvé l'ancre
                    NewRanging.transform.SetParent(node.transform); //l'ancre devien le parent de l'empty de ranging
                    NewRanging.transform.position = node.transform.position; //L'empty de renging se met à la position de l'ancre
                }
            }

            else if (node.GetComponent<SetMobileNodePosition>() != null) //Si le node est mobile
            {
                //Même chose que dans le if mais pour un node mobile
                if (node.GetComponent<SetMobileNodePosition>().Node_id == AnchorId)
                {
                    NewRanging.transform.SetParent(node.transform);
                    NewRanging.transform.position = node.transform.position;
                }
            }
        }
        NewRanging.name = RangingName; //On nome l'empty

        //On initialise le scripte Ranging.cs associé à l'objet empty qui pilote la représentation des mesure de ranging en temps réel
        NewRanging.GetComponent<Ranging>().SetupMQTT(TopicList, Topic, gameObject, EstimatedParent, lineCommunicationPrefab, lineErrorPrefab, rangingLineParent, RangeSpherePrefab);

        return Task.CompletedTask;
    }

}