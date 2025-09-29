using System;
using UnityEngine;

//Création d'une classe publique servant de template pour créer un objet qui pourra récupérer les infos du json de localisation
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


//Ce scripte sert à mettre à jour en temps réel la position du node mobile à partir des données récupérées sur le broker MQTT
public class SetMobileNodePosition : MonoBehaviour
{
    public string Node_id; //Variable publique indiquant l'identifiant du node
    private GameObject MobileNode; //GameObject représentant le node mobile
    private bool init = false; //Vérifie l'initialisation de la connexion du node
    [SerializeField] TopicSubscribeHandling MessageDetection; //Liste des topic auxquel on est abonné
    [SerializeField] Material ConnectedMat; //Le matériau d'un node connecté
    private Material DisconnectedMat; //Le matériaux d'un node déconnecté
    private float targetTime = 120f; //Une limite de temps pour un compte à rebour


    // Update is called once per frame
    private void Update()
    {
        if (init) //Si le node est connecté à une expérience
        {

            targetTime -= Time.deltaTime;
        }

        if (targetTime < 0 && init) //Si on a dépacé les 120 secondes
        {
            //Cela signifie qu'on ne reçoit plus de message de connexion donc le node est déconnecté
            MobileNode.GetComponent<MeshRenderer>().material = DisconnectedMat; //On applique le matériau de node déconnecté
            init = false; //On définit la connection comme non initialisée
        }
    }

    //Fonction d'initialisation du scripte appelé par NodeGenerator.cs
    public void SetupMQTT(TopicSubscribeHandling Handler, Material connected) //Récupère en entrée une liste de topic et le matériau de node connecté
    {
        MessageDetection = Handler; //Définit notre variable de liste des topic
        MobileNode = gameObject; //définit l'objet du Node comme étatnt l'objet auquel est rattaché ce scripte
        ConnectedMat = connected; //Définit notre variable de matériau connecté
        DisconnectedMat = MobileNode.GetComponent<MeshRenderer>().material; //Définit notre matériau ce node déconecté comme étant le matériau par défaut de notre objetr MobileNode
        AddLocalisationListener(); //Ajoute le listener de localisation
    }


    //Appelée à chaque update du topic localisation/{nom_du_node}/mobile
    //Met à jour la position du node dans la scène
    public void Handleupdate(string payload)
    {
        MobileNodePosition InfoPosition = JsonUtility.FromJson<MobileNodePosition>(payload); //Cré un objet de la classe défini au début du scripte et le remplie avec les information du json
        MobileNode.transform.position = new Vector3(-InfoPosition.x, InfoPosition.z, -InfoPosition.y); //Donne au node la position idiqué par les coordonnée du json
        /* /!\ATTENTION
        Le repère 3D d'Unity n'est pas orienté en suivant la "règle de la main droite comme le voudrait la convention
        Il faut donc convertir les coordonné du repère carthésien récuopéré dans le json pour que le node s'affiche corrextement dans Unity
        Donc Unity x est égale à -x du json
                   y est égale à z du json
                   z est égale à -y du json
       */
        Node_id = InfoPosition.UID; //Donne à la variable publique Node_id la valeure du l'id du node récupérée dans le json

    }

    //Appelée à chaque update du topic testbed/node/{Node.name}/out
    //Vérifie si le node est toujours conecté
    public void HandleConnectionUpdate(string payload)
    {
        if (!init) //si on est pas préalablement connecté
        {
            MobileNode.GetComponent<MeshRenderer>().material = ConnectedMat; //attribue au node le matériau de node connecté
            init = true; //définit la connection comme initialisé
        }

        targetTime = 120f; //Reset le compte à rebourt
    }

    //Ajoute un listener sur le topic localisation/{Node.name}/mobile pour mattre à jour sa localisation à chaque update
    private void AddLocalisationListener()
    {
        //Recherche le topic correspondant au node dans la liste pour chaque élement de la liste
        foreach (TopicEvent topicElement in MessageDetection.topicEventPairs)
        {
            if (topicElement.topic == $"localisation/{MobileNode.name}/mobile") //Si le topic correspond au node
            {
                //ajoute un listenenr qui appelera la fonction Handleupdate
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
        //Recherche le topic correspondant au node dans la liste pour chaque élement de la liste
        foreach (TopicEvent topicElement in MessageDetection.topicEventPairs)
        {
            if (topicElement.topic == $"testbed/node/{MobileNode.name}/out") //Si le topic correspond au node
            {
                //ajoute un listenenr qui appelera la fonction HandleConnectionupdate
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
