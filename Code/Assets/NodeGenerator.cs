using System.Threading.Tasks;
using TMPro;
using UnityEngine;

/*
 * Ce scripte � pour but de cr�er dynamiquement 
 * chacun des nodes dont on re�oit pour la premi�re 
 * fois leurs informations de localisation
 * */
public class NodeGenerator : MonoBehaviour
{
    //initialisation des variable utilis�es
    [SerializeField] GameObject MobileNodePrefab; //Le prefab utilis� pour les nodes mobiles
    [SerializeField] GameObject NodePrefab; //Le prefab utilis� pour les nodes fixes
    [SerializeField] GameObject CanvaPrefab; //Le prefab utilis� pour les canva servant � afficher les nom des nodes
    [SerializeField] GameObject EstimatedNodePrefab; //Le prefab utilis� pour les nodes estim�s
    [SerializeField] GameObject EstimatedParent; //L'Empty qui sera le parent de tous les nodes estim�s
    [SerializeField] GameObject RangingPrefab; //Le prefab utilis� pour les empty qui piloteron le ranging
    [SerializeField] GameObject RangeSpherePrefab; //Le prefab utilis� pour les sph�re repr�sentant les mesures de ranging

    [SerializeField] LineRenderer lineEstimationPrefab; //Le prefab utilis� pour lier un node estim� � son node r�el
    [SerializeField] LineRenderer lineCommunicationPrefab; //Le prefab utilis� pour repr�senter une mesure de ranging par une ligne entre les 2 ndes communiquant
    [SerializeField] LineRenderer lineErrorPrefab; //Le prefab utilis� pour repr�senter l'erreur de ranging
    [SerializeField] GameObject lineParent; //L'Empty parent de line reliant les nodes r�els et estim�s
    [SerializeField] GameObject rangingLineParent; //L'Empty parent des line de mesure de ranging et d'erreur
    [SerializeField] GameObject communicationLineParent; //L'Empty parent des line de repr�sentation de communications (utile que pour les nodes estim� qui n'ont pas de position r�elles connues)
    [SerializeField] Material ConnectedNodeMaterial; //le mat�riaux des node connect�s pour une exp�rience

    [SerializeField] MQTT MqttManager; //Le scripte MQTT.cs
    [SerializeField] TopicSubscribeHandling TopicList; //La liste des topic auquels on est abonn�e d�finit dans TopicSubscribeHandeling.cs


    //Cr�ation d'un node fixe
    public Task CreateNewNode(string name)
    {
        //On d�termie le nom du node � partir du nom du topic, on retire donc de ce dernier le d�but et la fin de la chaine de catact�res
        string start = "localisation/";
        string end = "/setup";
        int indexDebut = name.IndexOf(start) + start.Length;
        int indexFin = name.IndexOf(end);
        string NomDuNode = name.Substring(indexDebut, indexFin-indexDebut);

        //Cr�ation du node dans la sc�ne
        GameObject NewNode = Instantiate(NodePrefab); //On clone le pr�fab
        NewNode.transform.SetParent(gameObject.transform); //On d�termine son parent
        NewNode.name = NomDuNode; //On le nome
        NewNode.GetComponent<SetNodePosition>().SetupMQTT(TopicList, ConnectedNodeMaterial); //On initialise le scripte SetNodePosition.cs qui lui est associ� qui pilote en temps r�el la position du node

        //cr�ation de son canva pour afficher son nom
        GameObject NewCanva = Instantiate(CanvaPrefab);
        NewCanva.transform.SetParent(NewNode.transform);
        NewCanva.GetComponent<TextMeshProUGUI>().text = NomDuNode;

        return Task.CompletedTask;
    }

    //Cr�ation d'un node mobile
    //Pour commentaires d�taill�s voir la fonction CreateNewNode qui est tr�s semblable
    public Task CreateNewMobileNode(string name)
    {
        //d�termination du nom
        string start = "localisation/";
        string end = "/mobile";
        int indexDebut = name.IndexOf(start) + start.Length;
        int indexFin = name.IndexOf(end);
        string NomDuNode = name.Substring(indexDebut, indexFin - indexDebut);

        //Cr�ation du node dans la sc�ne
        GameObject NewNode = Instantiate(MobileNodePrefab);
        NewNode.transform.SetParent(gameObject.transform);
        NewNode.name = NomDuNode;
        NewNode.GetComponent<SetMobileNodePosition>().SetupMQTT(TopicList, ConnectedNodeMaterial);

        //cr�ation de son canva pour afficher son nom
        GameObject NewCanva = Instantiate(CanvaPrefab);
        NewCanva.transform.SetParent(NewNode.transform);
        NewCanva.GetComponent<TextMeshProUGUI>().text = NomDuNode;

        return Task.CompletedTask;

    }

    //Cr�ation d'un node estim�
    //Pour commentaires d�taill�s voir la fonction CreateNewNode qui est tr�s semblable
    public Task CreateNewEstimatedNode(string name)
    {
        //d�termination du nom
        string start = "localisation/";
        string end = "/estimation";
        int indexDebut = name.IndexOf(start) + start.Length;
        int indexFin = name.IndexOf(end);
        string NomDuNode = name.Substring(indexDebut, indexFin - indexDebut);

        //Cr�ation du node dans la sc�ne
        GameObject NewNode = Instantiate(EstimatedNodePrefab);
        NewNode.transform.SetParent(EstimatedParent.transform);
        NewNode.name = NomDuNode;
        NewNode.GetComponent<SetEstimatedNodePosition>().SetupMQTT(TopicList, gameObject.transform, lineEstimationPrefab, lineCommunicationPrefab, lineParent.transform, communicationLineParent.transform, ConnectedNodeMaterial);

        //cr�ation de son canva pour afficher son nom
        GameObject NewCanva = Instantiate(CanvaPrefab);
        NewCanva.transform.SetParent(NewNode.transform);
        NewCanva.GetComponent<TextMeshProUGUI>().text = NomDuNode;
        return Task.CompletedTask;

    }

    //cr�ation de l'Empty qui pilote la repr�sentation d'une mesure de ranging associ� � un node d�tect� et un node ancre
    public Task CreateNewRanging(string Topic)
    {
        //Recherche de l'id de l'ancre � partir du nom du topic
        string AnchorId = Topic.Substring(13, 4);
        //D�termination du nom de l'Empty de type {Id du node d�tect�}-{Id de l'ancre}
        string RangingName = $"{Topic.Substring(8, 4)}-{Topic.Substring(13, 4)}";

        //Cr�ation de l'Empty
        GameObject NewRanging = Instantiate(RangingPrefab);
        //Recherche du node ancre associ� qui sera le parent de l'empty de ranging
        foreach (Transform node in gameObject.transform)
        {
            /*
             * On veut retrouver l'ancre via son id qui est une varriable public dans les scriptes
             * SetNodePosition des nodes fixes et SetMobileNodePosition des node mobiles.
             * C'est pour cela que l'on cr� deux cas, un pour les fixe et un pour les mobile
             * */
            if (node.GetComponent<SetNodePosition>() != null) //Si le node est fixe
            {
                if (node.GetComponent<SetNodePosition>().Node_id == AnchorId) //Si l'id correspond
                {
                    //On a trouv� l'ancre
                    NewRanging.transform.SetParent(node.transform); //l'ancre devien le parent de l'empty de ranging
                    NewRanging.transform.position = node.transform.position; //L'empty de renging se met � la position de l'ancre
                }
            }

            else if (node.GetComponent<SetMobileNodePosition>() != null) //Si le node est mobile
            {
                //M�me chose que dans le if mais pour un node mobile
                if (node.GetComponent<SetMobileNodePosition>().Node_id == AnchorId)
                {
                    NewRanging.transform.SetParent(node.transform);
                    NewRanging.transform.position = node.transform.position;
                }
            }
        }
        NewRanging.name = RangingName; //On nome l'empty

        //On initialise le scripte Ranging.cs associ� � l'objet empty qui pilote la repr�sentation des mesure de ranging en temps r�el
        NewRanging.GetComponent<Ranging>().SetupMQTT(TopicList, Topic, gameObject, EstimatedParent, lineCommunicationPrefab, lineErrorPrefab, rangingLineParent, RangeSpherePrefab);

        return Task.CompletedTask;
    }

}