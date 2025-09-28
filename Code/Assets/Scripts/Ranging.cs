using System;
using System.Collections.Generic;
using System.Runtime.ConstrainedExecution;
using System.Threading.Tasks;
using UnityEngine;


//Cr�ation d'une classe publique servant de template pour cr�er un objet qui pourra r�cup�rer les infos du json de ranging
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

//Ce scripte sert � mettre � jour en temps r�el les repr�sentation des mesures de ranging � partir des donn�es r�cup�r�es sur le broker MQTT
public class Ranging : MonoBehaviour
{
    //initialisation des variable utilis�es
    [SerializeField] TopicSubscribeHandling MessageDetection; //Liste des topic auxquel on est abonn�
    [SerializeField] GameObject ParentEstimNode; //L'Empty qui sera le parent de tous les nodes estim�s
    [SerializeField] LineRenderer LineCommunication; //L'objet utilis� pour repr�senter une mesure de ranging par une ligne entre les 2 ndes communiquant
    [SerializeField] LineRenderer LineError; //L'objet utilis� pour repr�senter l'erreur de ranging
    [SerializeField] DrawLine DrawLine; //Le scripte DrawLine.cs
    [SerializeField] GameObject RangeSphere; //L'objet utilis� pour les sph�re repr�sentant les mesures de ranging


    public GameObject anchor; //Le node ancre
    public GameObject RealNode; //Le node d�tect� r�el
    public GameObject EstimatedNode; //Le node d�tect� estim�

    private string nodeID; //L'id du node d�tect�
    public float targetTime = 1f; //Une limite de temps pour un compte � rebour

    private void Update()
    {
        targetTime -= Time.deltaTime; //on lance un compte � rebour de 120 seconde

        if (targetTime < 0) //Si on a d�pac� une secondes
        {
            //Tout les objets de repr�sentation de ranging sont d�sactiv� car l'ancre ne publie plus
            LineCommunication.gameObject.SetActive(false);
            LineError.gameObject.SetActive(false);
            RangeSphere.gameObject.SetActive(false);
        }
    }


    //Appel�e � chaque update du topic ranging/id_du_node_d�tect�/id_de_l'ancre/indication
    //Met � jour la repr�sentation du ranging dans la sc�ne
    public void Handleupdate(string payload)
    {
        targetTime = 1f; //Reset le compte � rebour car l'ancre publie
        RangingIndication rangingInfo = JsonUtility.FromJson<RangingIndication>(payload); //Cr� un objet de la classe d�fini au d�but du scripte et le remplie avec les information du json

        if (DrawLine.repRect) //Si on a choisi de reppr�senter le ranging avec des lignes
        {
            //On active les objet de repr�sentation par ligne et on d�sactive l'objet de repr�sentation par sph�re
            LineCommunication.gameObject.SetActive(true);
            LineError.gameObject.SetActive(true);
            RangeSphere.gameObject.SetActive(false);

            if (RealNode != null) //On v�rifie que le node d�tect� r�el existe bien
            {
                //Ici on va verrifier si on veut tracer les lignes entre l'ancre et le node r�el ou entre l'ancre et le node d�tect�
                if (DrawLine.errorRangingParRapportNodeR�el) //Si on a choisi de tracer entre l'ancre et le node r�el
                {
                    //On appel la fonction du scripte DrawLine.cs qui sert � tracer les repr�sentation rectiligne de ranging et de son erreur
                    DrawLine.DrawRangingLine(anchor, RealNode, LineCommunication, LineError, rangingInfo.distance, rangingInfo.range);
                }

                else if (DrawLine.errorRangingParRapportNodeEstim & EstimatedNode != null) //Si on a choisi de tracer entre l'ancre et le node estim� et que ce dernier exite bien dans la sc�ne
                {
                    //On appel la fonction du scripte DrawLine.cs qui sert � tracer les repr�sentation rectiligne de ranging et de son erreur
                    DrawLine.DrawRangingLine(anchor, EstimatedNode, LineCommunication, LineError, rangingInfo.distance, rangingInfo.range);
                }

                else if (DrawLine.errorRangingParRapportNodeEstim & EstimatedNode == null) //Si on a choisi de tracer entre l'ancre et le node estim� et que ce dernier n'exite PAS dans la sc�ne
                {
                    //On v�rifie que le node estim� n'a pas �t� ajout� � la sc�ne
                    foreach (Transform child in ParentEstimNode.transform)
                    {
                        if (child.GetComponent<SetEstimatedNodePosition>().node_id == nodeID) //On recherche via l'Id du node
                        {   //Si on le trouve on d�termine notre variable de node estim� sur l'objet trouv� dans la sc�ne
                            EstimatedNode = child.gameObject;
                            break;
                        }

                        if (EstimatedNode != null) //Si on a trouv� un node estim�
                        {
                            //On appel la fonction du scripte DrawLine.cs qui sert � tracer les repr�sentation rectiligne de ranging et de son erreur
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


        else if (DrawLine.repSphere) //Si on a choisi de reppr�senter le ranging avec des sph�res
        {
            //On d�sactive les objets de repr�sentation rectiligne du ranging
            LineCommunication.gameObject.SetActive(false);
            LineError.gameObject.SetActive(false);

            if(RealNode != null) //On v�rifie que le node d�tect� r�el existe bien
            {
                RangeSphere.SetActive(true); //On active l'objet de repr�sentation sh�rique du ranging
                RangeSphere.transform.localScale = new Vector3(2 * rangingInfo.range, 2 * rangingInfo.range, 2 * rangingInfo.range); //On lui donnze une taille �gale � 2 fois le ranging car le diam�tre de la sph�re vaut 2 ranging

            }
        }
    }

    //Ajoute un listener sur le topic ranging/id_du_node_d�tect�/id_de_l'ancre/indication pour mattre � jour la repr�sentation du ranging � chaque update
    private void TopicListenner(string TopicName)
    {
        //Recherche le topic correspondant dans la liste pour chaque �lement de la liste
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

    //Fonction d'initialisation du scripte appel� par NodeGenerator.cs
    /*R�cup�re en entr� la liste des topic, le nom du topic correspondant � nos mesures de ranging,
     * l'empty parent des nodes fixes et mobiles, la parent des nodes estim�s, le prefab de la line de ranging, 
     * le prefab de la line d'erreur, le parent des lines et le prefab des sh�re de ranging
     */
    public Task SetupMQTT(TopicSubscribeHandling Handler, string topic, GameObject ParentNodes, GameObject ParentEstimated, LineRenderer Communication, LineRenderer Error, GameObject ParentLine, GameObject sphere)
    {
        MessageDetection = Handler; //D�finit notre variable de liste des topic
        TopicListenner(topic); //Ajoute le listener de ranging

        nodeID = topic.Substring(8, 4); //D�finit la variable de l'id du node d�tect�
        anchor = gameObject; //d�finit l'objet du node ancre comme �tant l'objet auquel est associ� ce scripte

        //Recherche le node r�el dans la liste des enfants du parent des nodes
        foreach(Transform child in ParentNodes.transform)
        {
            if (child.GetComponent<SetNodePosition>() != null!)//v�rifie si le node que l'on annalyse � cette it�ration de la boucle est fixe
            {
                if (child.GetComponent<SetNodePosition>().Node_id == nodeID) //si l'id du node analys� � cette it�ration correspond � celui de notre node r�el
                {
                    RealNode = child.gameObject; //On d�finit notre variable de node r�el comme �tant ce node
                    break;
                }
                
            }

            else if (child.GetComponent<SetMobileNodePosition>() != null!) //v�rifie si le node que l'on annalyse � cette it�ration de la boucle est mobile
            {
                if (child.GetComponent<SetMobileNodePosition>().Node_id == nodeID) //si l'id du node analys� � cette it�ration correspond � celui de notre node r�el
                {
                    RealNode = child.gameObject; //On d�finit notre variable de node r�el comme �tant ce node
                    break;
                }
            }
        }

        ParentEstimNode = ParentEstimated; //D�finit notre variable de parent des node estim�
        //Recherche le node estilm� dans la liste des enfants du parent des nodes estim�
        foreach (Transform child in ParentEstimNode.transform)
        {
            if(child.GetComponent<SetEstimatedNodePosition>().node_id == nodeID) //si l'id du node analys� � cette it�ration correspond � celui de notre node r�el (qui est le m�me que pour l'estim�)
            {
                EstimatedNode = child.gameObject; //On d�finit notre variable de node estim� comme �tant ce node
                break;
            }

        }

        //Set up de l'objet de repr�sentation rectiligne de ranging
        LineCommunication = Instantiate(Communication);
        LineCommunication.name = $"Communication {topic.Substring(8, 4)}-{topic.Substring(13, 4)}";
        LineCommunication.positionCount = 2;
        LineCommunication.transform.SetParent(ParentLine.transform);

        //Set up de l'objet de repr�sentation rectiligne de l'erreur de ranging
        LineError = Instantiate(Error);
        LineError.name = $"Error {topic.Substring(8, 4)}-{topic.Substring(13, 4)}";
        LineError.positionCount = 2;
        LineError.transform.SetParent(ParentLine.transform);

        DrawLine = ParentNodes.GetComponent<DrawLine>(); //R�cup�ration de la classe associ� au scripte DrawLine.cs

        //Set up de l'objet de repr�sentation sh�rique de ranging
        RangeSphere = Instantiate(sphere);
        RangeSphere.name = $"Range {topic.Substring(8, 4)}-{topic.Substring(13, 4)}";
        RangeSphere.transform.SetParent(gameObject.transform);
        RangeSphere.transform.position = gameObject.transform.position;
        RangeSphere.GetComponent<MeshRenderer>().material.color = DrawLine.ShereColor[0];
        DrawLine.ShereColor.Remove(DrawLine.ShereColor[0]);

        return Task.CompletedTask;
    }
}
