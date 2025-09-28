using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Threading.Tasks;
using UnityEngine;


//Création d'une classe publique servant de template pour créer un objet qui pourra récupérer les infos du json de ranging
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

//Ce scripte sert à mettre à jour en temps réel les représentation des mesures de ranging à partir des données récupérées sur le broker MQTT
public class Ranging : MonoBehaviour
{
    //initialisation des variable utilisées
    [SerializeField] TopicSubscribeHandling MessageDetection; //Liste des topic auxquel on est abonné
    [SerializeField] GameObject ParentEstimNode; //L'Empty qui sera le parent de tous les nodes estimés
    [SerializeField] LineRenderer LineCommunication; //L'objet utilisé pour représenter une mesure de ranging par une ligne entre les 2 ndes communiquant
    [SerializeField] LineRenderer LineError; //L'objet utilisé pour représenter l'erreur de ranging
    [SerializeField] DrawLine DrawLine; //Le scripte DrawLine.cs
    [SerializeField] GameObject RangeSphere; //L'objet utilisé pour les sphère représentant les mesures de ranging


    public GameObject anchor; //Le node ancre
    public GameObject RealNode; //Le node détecté réel
    public GameObject EstimatedNode; //Le node détecté estimé

    private string nodeID; //L'id du node détecté
    public float targetTime = 1f; //Une limite de temps pour un compte à rebour

    private void Update()
    {
        targetTime -= Time.deltaTime; //on lance un compte à rebour de 120 seconde

        if (targetTime < 0) //Si on a dépacé une secondes
        {
            //Tout les objets de représentation de ranging sont désactivé car l'ancre ne publie plus
            LineCommunication.gameObject.SetActive(false);
            LineError.gameObject.SetActive(false);
            RangeSphere.gameObject.SetActive(false);
        }
    }


    //Appelée à chaque update du topic ranging/id_du_node_détecté/id_de_l'ancre/indication
    //Met à jour la représentation du ranging dans la scène
    public void Handleupdate(string payload)
    {
        targetTime = 1f; //Reset le compte à rebour car l'ancre publie
        RangingIndication rangingInfo = JsonUtility.FromJson<RangingIndication>(payload); //Cré un objet de la classe défini au début du scripte et le remplie avec les information du json

        if (DrawLine.repRect) //Si on a choisi de repprésenter le ranging avec des lignes
        {
            //On active les objet de représentation par ligne et on désactive l'objet de représentation par sphère
            LineCommunication.gameObject.SetActive(true);
            LineError.gameObject.SetActive(true);
            RangeSphere.gameObject.SetActive(false);

            if (RealNode != null) //On vérifie que le node détecté réel existe bien
            {
                //Ici on va verrifier si on veut tracer les lignes entre l'ancre et le node réel ou entre l'ancre et le node détecté
                if (DrawLine.errorRangingParRapportNodeRéel) //Si on a choisi de tracer entre l'ancre et le node réel
                {
                    //On appel la fonction du scripte DrawLine.cs qui sert à tracer les représentation rectiligne de ranging et de son erreur
                    DrawLine.DrawRangingLine(anchor, RealNode, LineCommunication, LineError, rangingInfo.distance, rangingInfo.range);
                }

                else if (DrawLine.errorRangingParRapportNodeEstim & EstimatedNode != null) //Si on a choisi de tracer entre l'ancre et le node estimé et que ce dernier exite bien dans la scène
                {
                    //On appel la fonction du scripte DrawLine.cs qui sert à tracer les représentation rectiligne de ranging et de son erreur
                    DrawLine.DrawRangingLine(anchor, EstimatedNode, LineCommunication, LineError, rangingInfo.distance, rangingInfo.range);
                }

                else if (DrawLine.errorRangingParRapportNodeEstim & EstimatedNode == null) //Si on a choisi de tracer entre l'ancre et le node estimé et que ce dernier n'exite PAS dans la scène
                {
                    //On vérifie que le node estimé n'a pas été ajouté à la scène
                    foreach (Transform child in ParentEstimNode.transform)
                    {
                        if (child.GetComponent<SetEstimatedNodePosition>().node_id == nodeID) //On recherche via l'Id du node
                        {   //Si on le trouve on détermine notre variable de node estimé sur l'objet trouvé dans la scène
                            EstimatedNode = child.gameObject;
                            break;
                        }

                        if (EstimatedNode != null) //Si on a trouvé un node estimé
                        {
                            //On appel la fonction du scripte DrawLine.cs qui sert à tracer les représentation rectiligne de ranging et de son erreur
                            DrawLine.DrawRangingLine(anchor, EstimatedNode, LineCommunication, LineError, rangingInfo.distance, rangingInfo.range);
                        }
                    }
                }
            }

            /*else
            {
                if (EstimatedNode != null)
                {
                    DrawLine.DrawRangingLine(anchor, EstimatedNode, LineCommunication, LineError, rangingInfo.distance, rangingInfo.range);
                }
            }*/
            
        }


        else if (DrawLine.repSphere) //Si on a choisi de repprésenter le ranging avec des sphères
        {
            //On désactive les objets de représentation rectiligne du ranging
            LineCommunication.gameObject.SetActive(false);
            LineError.gameObject.SetActive(false);

            if(RealNode != null) //On vérifie que le node détecté réel existe bien
            {
                RangeSphere.SetActive(true); //On active l'objet de représentation shérique du ranging
                RangeSphere.transform.localScale = new Vector3(2 * rangingInfo.range, 2 * rangingInfo.range, 2 * rangingInfo.range); //On lui donnze une taille égale à 2 fois le ranging car le diamètre de la sphère vaut 2 ranging

            }
        }
    }

    //Ajoute un listener sur le topic ranging/id_du_node_détecté/id_de_l'ancre/indication pour mattre à jour la représentation du ranging à chaque update
    private void TopicListenner(string TopicName)
    {
        //Recherche le topic correspondant dans la liste pour chaque élement de la liste
        foreach (TopicEvent topicElement in MessageDetection.topicEventPairs)
        {

            if (topicElement.topic == TopicName) //Si le topic correspond
            {
                //ajoute un listenenr qui appelera la fonction Handleupdate
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

    //Fonction d'initialisation du scripte appelé par NodeGenerator.cs
    /*Récupère en entré la liste des topic, le nom du topic correspondant à nos mesures de ranging,
     * l'empty parent des nodes fixes et mobiles, la parent des nodes estimés, le prefab de la line de ranging, 
     * le prefab de la line d'erreur, le parent des lines et le prefab des shère de ranging
     */
    public Task SetupMQTT(TopicSubscribeHandling Handler, string topic, GameObject ParentNodes, GameObject ParentEstimated, LineRenderer Communication, LineRenderer Error, GameObject ParentLine, GameObject sphere)
    {
        MessageDetection = Handler; //Définit notre variable de liste des topic
        TopicListenner(topic); //Ajoute le listener de ranging

        nodeID = topic.Substring(8, 4); //Définit la variable de l'id du node détecté
        anchor = gameObject; //définit l'objet du node ancre comme étant l'objet auquel est associé ce scripte

        //Recherche le node réel dans la liste des enfants du parent des nodes
        foreach(Transform child in ParentNodes.transform)
        {
            if (child.GetComponent<SetNodePosition>() != null!)//vérifie si le node que l'on annalyse à cette itération de la boucle est fixe
            {
                if (child.GetComponent<SetNodePosition>().Node_id == nodeID) //si l'id du node analysé à cette itération correspond à celui de notre node réel
                {
                    RealNode = child.gameObject; //On définit notre variable de node réel comme étant ce node
                    break;
                }
                
            }

            else if (child.GetComponent<SetMobileNodePosition>() != null!) //vérifie si le node que l'on annalyse à cette itération de la boucle est mobile
            {
                if (child.GetComponent<SetMobileNodePosition>().Node_id == nodeID) //si l'id du node analysé à cette itération correspond à celui de notre node réel
                {
                    RealNode = child.gameObject; //On définit notre variable de node réel comme étant ce node
                    break;
                }
            }
        }

        ParentEstimNode = ParentEstimated; //Définit notre variable de parent des node estimé
        //Recherche le node estilmé dans la liste des enfants du parent des nodes estimé
        foreach (Transform child in ParentEstimNode.transform)
        {
            if(child.GetComponent<SetEstimatedNodePosition>().node_id == nodeID) //si l'id du node analysé à cette itération correspond à celui de notre node réel (qui est le même que pour l'estimé)
            {
                EstimatedNode = child.gameObject; //On définit notre variable de node estimé comme étant ce node
                break;
            }

        }

        //Set up de l'objet de représentation rectiligne de ranging
        LineCommunication = Instantiate(Communication);
        LineCommunication.name = $"Communication {topic.Substring(8, 4)}-{topic.Substring(13, 4)}";
        LineCommunication.positionCount = 2;
        LineCommunication.transform.SetParent(ParentLine.transform);

        //Set up de l'objet de représentation rectiligne de l'erreur de ranging
        LineError = Instantiate(Error);
        LineError.name = $"Error {topic.Substring(8, 4)}-{topic.Substring(13, 4)}";
        LineError.positionCount = 2;
        LineError.transform.SetParent(ParentLine.transform);

        DrawLine = ParentNodes.GetComponent<DrawLine>(); //Récupération de la classe associé au scripte DrawLine.cs

        //Set up de l'objet de représentation shérique de ranging
        RangeSphere = Instantiate(sphere);
        RangeSphere.name = $"Range {topic.Substring(8, 4)}-{topic.Substring(13, 4)}";
        RangeSphere.transform.SetParent(gameObject.transform);
        RangeSphere.transform.position = gameObject.transform.position;
        RangeSphere.GetComponent<MeshRenderer>().material.color = DrawLine.ShereColor[0];
        DrawLine.ShereColor.Remove(DrawLine.ShereColor[0]);

        return Task.CompletedTask;
    }
}
