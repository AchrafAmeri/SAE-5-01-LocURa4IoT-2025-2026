using System;
using System.Runtime.ConstrainedExecution;
using UnityEngine;

//Cr�ation d'une classe publique servant de template pour cr�er un objet qui pourra r�cup�rer les infos du json de localisation
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

//Ce scripte sert � mettre � jour en temps r�el la position du node � partir des donn�es r�cup�r�es sur le broker MQTT
public class SetNodePosition : MonoBehaviour
{
    [SerializeField] public string Node_id; //Variable publique indiquant l'identifiant du node
    private GameObject Node; //GameObject repr�sentant le node
    private bool init = false; //V�rifie l'initialisation de la connexion du node
    [SerializeField] TopicSubscribeHandling MessageDetection; //Liste des topic auxquel on est abonn�
    [SerializeField] Material ConnectedMat; //Le mat�riau d'un node connect�
    private Material DisconnectedMat; //Le mat�riaux d'un node d�connect�
    private float targetTime = 120f; //Une limite de temps pour un compte � rebour

    // Update is called once per frame
    private void Update()
    {
        if (init) //Si le node est connect� � une exp�rience
        {
            //on lance un compte � rebour de 120 seconde
            targetTime -= Time.deltaTime;
        }

        if (targetTime < 0 && init) //Si on a d�pac� les 120 secondes
        {
            //Cela signifie qu'on ne re�oit plus de message de connexion donc le node est d�connect�
            Node.GetComponent<MeshRenderer>().material = DisconnectedMat; //On applique le mat�riau de node d�connect�
            init = false; //On d�finit la connection comme non initialis�e
        }
    }
    
    //Appel�e � chaque update du topic localisation/{nom_du_node}/setup
    //Met � jour la position du node dans la sc�ne
    public void Handleupdate(string payload)
    {
        NodePosition InfoPosition = JsonUtility.FromJson<NodePosition>(payload); //Cr� un objet de la classe d�fini au d�but du scripte et le remplie avec les information du json
        Node.transform.position = new Vector3(-InfoPosition.x, InfoPosition.z, -InfoPosition.y); //Donne au node la position idiqu� par les coordonn�e du json
        /* /!\ATTENTION
         Le rep�re 3D d'Unity n'est pas orient� en suivant la "r�gle de la main droite comme le voudrait la convention
         Il faut donc convertir les coordonn� du rep�re carth�sien r�cuop�r� dans le json pour que le node s'affiche corrextement dans Unity
         Donc Unity x est �gale � -x du json
                    y est �gale � z du json
                    z est �gale � -y du json
        */

        Node_id = InfoPosition.UID; //Donne � la variable publique Node_id la valeure du l'id du node r�cup�r�e dans le json

    }


    //Appel�e � chaque update du topic testbed/node/{Node.name}/out
    //V�rifie si le node est toujours conect�
    public void HandleConnectionUpdate(string payload)
    {
        if (!init) //si on est pas pr�alablement connect�
        {
            Node.GetComponent<MeshRenderer>().material = ConnectedMat; //attribue au node le mat�riau de node connect�
            init = true; //d�finit la connection comme initialis�
        }

        targetTime = 120f; //Reset le compte � rebourt
    }

    //Ajoute un listener sur le topic localisation/{Node.name}/setup pour mattre � jour sa localisation � chaque update
    private void AddLocalisationListener()
    {
        //Recherche le topic correspondant au node dans la liste pour chaque �lement de la liste
        foreach (TopicEvent topicElement in MessageDetection.topicEventPairs)
        {
            if (topicElement.topic == $"localisation/{Node.name}/setup") //Si le topic correspond au node
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

    //Ajoute un listener sur le topic testbed/node/{Node.name}/out pour v�rifier sa connextion � chaque update
    public void AddConnectionListener()
    {
        //Recherche le topic correspondant au node dans la liste pour chaque �lement de la liste
        foreach (TopicEvent topicElement in MessageDetection.topicEventPairs)
        {
            if (topicElement.topic == $"testbed/node/{Node.name}/out") //Si le topic correspond au node
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
                Node.GetComponent<MeshRenderer>().material = ConnectedMat;
                init = true;
                targetTime = 120f;
                return;
            }
        }
    }

    //Fonction d'initialisation du scripte appel� par NodeGenerator.cs
    public void SetupMQTT(TopicSubscribeHandling Handler, Material connected) //R�cup�re en entr�e une liste de topic et le mat�riau de node connect�
    {
        MessageDetection = Handler; //D�finit notre variable de liste des topic
        Node = gameObject; //d�finit l'objet du Node comme �tatnt l'objet auquel est rattach� ce scripte
        ConnectedMat = connected; //D�finit notre variable de mat�riau connect�
        DisconnectedMat = Node.GetComponent<MeshRenderer>().material; //D�finit notre mat�riau ce node d�conect� comme �tant le mat�riau par d�faut de notre objetr Node
        AddLocalisationListener(); //Ajoute le listener de localisation
    }

}
