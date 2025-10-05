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


    //  Optimisation : m�thode utilitaire pour extraire le nom d�un node � partir d�un topic
    private string ExtractNodeName(string topic, string startTag, string endTag)
    {
        int start = topic.IndexOf(startTag);
        if (start == -1) return topic;

        start += startTag.Length;
        int end = topic.IndexOf(endTag, start);
        if (end == -1 || end <= start) return topic.Substring(start);

        return topic.Substring(start, end - start);
    }

    //  Optimisation : m�thode utilitaire pour cr�er le canvas d�affichage
    private void CreateNodeLabel(GameObject parent, string nodeName)
    {
        GameObject newCanvas = Instantiate(CanvaPrefab, parent.transform);
        newCanvas.GetComponent<TextMeshProUGUI>().text = nodeName;
    }

    //Cr�ation d'un node fixe
    public Task CreateNewNode(string name)
    {
        //On d�termie le nom du node � partir du nom du topic, on retire donc de ce dernier le d�but et la fin de la chaine de catact�res
        string start = "localisation/";
        string end = "/setup";
        //  Optimisation : remplac� par une fonction plus s�re
        // int indexDebut = name.IndexOf(start) + start.Length;
        // int indexFin = name.IndexOf(end);
        // string NomDuNode = name.Substring(indexDebut, indexFin-indexDebut);
        string NomDuNode = ExtractNodeName(name, start, end);

        //Cr�ation du node dans la sc�ne
        GameObject NewNode = Instantiate(NodePrefab, transform); //On clone le pr�fab
        //  Optimisation : Instantiate(NodePrefab, transform) remplace Instantiate + SetParent
        // NewNode.transform.SetParent(gameObject.transform); 
        NewNode.name = NomDuNode; //On le nome
        //  Optimisation : stockage du composant au lieu de l�appeler plusieurs fois
        var setNode = NewNode.GetComponent<SetNodePosition>();
        setNode.SetupMQTT(TopicList, ConnectedNodeMaterial); //On initialise le scripte SetNodePosition.cs

        //cr�ation de son canva pour afficher son nom
        CreateNodeLabel(NewNode, NomDuNode);
        // Ancienne version :
        // GameObject NewCanva = Instantiate(CanvaPrefab);
        // NewCanva.transform.SetParent(NewNode.transform);
        // NewCanva.GetComponent<TextMeshProUGUI>().text = NomDuNode;

        return Task.CompletedTask;
    }

    //Cr�ation d'un node mobile
    //Pour commentaires d�taill�s voir la fonction CreateNewNode qui est tr�s semblable
    public Task CreateNewMobileNode(string name)
    {
        //d�termination du nom
        string start = "localisation/";
        string end = "/mobile";
        //  Optimisation : remplac� par une fonction r�utilisable
        // int indexDebut = name.IndexOf(start) + start.Length;
        // int indexFin = name.IndexOf(end);
        // string NomDuNode = name.Substring(indexDebut, indexFin - indexDebut);
        string NomDuNode = ExtractNodeName(name, start, end);

        //Cr�ation du node dans la sc�ne
        GameObject NewNode = Instantiate(MobileNodePrefab, transform);
        //  Optimisation : Instantiate(MobileNodePrefab, transform) au lieu de SetParent ensuite
        NewNode.name = NomDuNode;
        var setMobile = NewNode.GetComponent<SetMobileNodePosition>();
        setMobile.SetupMQTT(TopicList, ConnectedNodeMaterial);
        // Ancienne version :
        // NewNode.GetComponent<SetMobileNodePosition>().SetupMQTT(TopicList, ConnectedNodeMaterial);

        //cr�ation de son canva pour afficher son nom
        CreateNodeLabel(NewNode, NomDuNode);
        // Ancienne version :
        // GameObject NewCanva = Instantiate(CanvaPrefab);
        // NewCanva.transform.SetParent(NewNode.transform);
        // NewCanva.GetComponent<TextMeshProUGUI>().text = NomDuNode;

        return Task.CompletedTask;

    }

    //Cr�ation d'un node estim�
    //Pour commentaires d�taill�s voir la fonction CreateNewNode qui est tr�s semblable
    public Task CreateNewEstimatedNode(string name)
    {
        //d�termination du nom
        string start = "localisation/";
        string end = "/estimation";
        //  Optimisation : remplac� par la fonction ExtractNodeName
        // int indexDebut = name.IndexOf(start) + start.Length;
        // int indexFin = name.IndexOf(end);
        // string NomDuNode = name.Substring(indexDebut, indexFin - indexDebut);
        string NomDuNode = ExtractNodeName(name, start, end);

        //Cr�ation du node dans la sc�ne
        GameObject NewNode = Instantiate(EstimatedNodePrefab, EstimatedParent.transform);
        NewNode.name = NomDuNode;
        var setEstimated = NewNode.GetComponent<SetEstimatedNodePosition>();
        // Ancienne version :
        // NewNode.GetComponent<SetEstimatedNodePosition>().SetupMQTT(TopicList, gameObject.transform, lineEstimationPrefab, lineCommunicationPrefab, lineParent.transform, communicationLineParent.transform, ConnectedNodeMaterial);
        setEstimated.SetupMQTT(TopicList, gameObject.transform, lineEstimationPrefab, lineCommunicationPrefab, lineParent.transform, communicationLineParent.transform, ConnectedNodeMaterial);

        //cr�ation de son canva pour afficher son nom
        CreateNodeLabel(NewNode, NomDuNode);
        // Ancienne version :
        // GameObject NewCanva = Instantiate(CanvaPrefab);
        // NewCanva.transform.SetParent(NewNode.transform);
        // NewCanva.GetComponent<TextMeshProUGUI>().text = NomDuNode;

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
            //  Optimisation : factorisation et break quand trouv�
            var fixedNode = node.GetComponent<SetNodePosition>();
            if (fixedNode != null && fixedNode.Node_id == AnchorId)
            {
                NewRanging.transform.SetParent(node.transform);
                NewRanging.transform.position = node.position;
                break;
            }

            var mobileNode = node.GetComponent<SetMobileNodePosition>();
            if (mobileNode != null && mobileNode.Node_id == AnchorId)
            {
                NewRanging.transform.SetParent(node.transform);
                NewRanging.transform.position = node.position;
                break;
            }

            // Ancienne version :
            /*
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
            */
        }
        NewRanging.name = RangingName; //On nome l'empty

        //On initialise le scripte Ranging.cs associ� � l'objet empty qui pilote la repr�sentation des mesure de ranging en temps r�el
        NewRanging.GetComponent<Ranging>().SetupMQTT(TopicList, Topic, gameObject, EstimatedParent, lineCommunicationPrefab, lineErrorPrefab, rangingLineParent, RangeSpherePrefab);

        return Task.CompletedTask;
    }
}
