using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;


public class DoorIndication
{
    public float battery;
    public bool battery_low;
    public bool contact;
    public float last_seen;
    public float linkquality;
    public bool tamper;
    public float voltage;
}

public class DoorSate : MonoBehaviour
{

    [SerializeField] TopicSubscribeHandling MessageDetection;
    public Material doorMaterial;
    public Material doorGhostMaterial;
    public Material doorHandleMaterial;
    public Material doorHandleGhostMaterial;
    Animator animator;
    float I = 1f;
    private float fadeDuration = 0.4f;
    private float alphaOpaque = 1f;
    private float alphaTransparent = 0.4f;
    private bool IsDoorWorking = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public Task SetupDoorState()
    {
        gameObject.GetComponent<Renderer>().material = doorMaterial;
        foreach (Transform child in gameObject.transform)
        {
            child.gameObject.GetComponent<Renderer>().material = doorHandleMaterial;
        }
        IsDoorWorking = true;

        AddDoorStateListener();

        return Task.CompletedTask;
    }


    void OnTriggerEnter(Collider other)
    {
        if (IsDoorWorking)
        {
            gameObject.GetComponent<Renderer>().material = doorGhostMaterial;
            StartCoroutine(FadeOut(gameObject, fadeDuration));

            foreach (Transform child in gameObject.transform)
            {
                child.gameObject.GetComponent<Renderer>().material = doorHandleGhostMaterial;
                StartCoroutine(FadeOut(child.gameObject, fadeDuration));
            }
        }
        
    }

    void OnTriggerExit(Collider other)
    {
        if (IsDoorWorking)
        {
            StartCoroutine(FadeIn(gameObject, doorMaterial, fadeDuration));

            foreach (Transform child in gameObject.transform)
            {

                StartCoroutine(FadeIn(child.gameObject, doorHandleMaterial, fadeDuration));
            }
        }
        

    }

    public void Handleupdate(string payload)
    {
        DoorIndication DoorInfo = JsonUtility.FromJson<DoorIndication>(payload);
        if (DoorInfo.contact)
        {
            I = 1f;
        }
        else 
        { 
            I = 0f;

        }
        animator.SetFloat("Porte", I);
    }

    public void AddDoorStateListener()
    {
        foreach (TopicEvent topicElement in MessageDetection.topicEventPairs)
        {
            if (topicElement.topic == $"api/3/room/{gameObject.name}/sensor/TS0203/id/1/indication")
            {
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

    IEnumerator FadeOut(GameObject gameObject, float duration)
    {
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float normalizedTime = t / duration;

            gameObject.GetComponent<Renderer>().material.color = new Color(
                gameObject.GetComponent<Renderer>().material.color.r,
                gameObject.GetComponent<Renderer>().material.color.g,
                gameObject.GetComponent<Renderer>().material.color.b,
                Mathf.Lerp(alphaOpaque, alphaTransparent, normalizedTime) );

            yield return null;
        }
        gameObject.GetComponent<Renderer>().material.color = new Color(
                gameObject.GetComponent<Renderer>().material.color.r,
                gameObject.GetComponent<Renderer>().material.color.g,
                gameObject.GetComponent<Renderer>().material.color.b,
                alphaTransparent);
    }

    IEnumerator FadeIn(GameObject gameObject, Material material, float duration)
    {
        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            float normalizedTime = t / duration;

            gameObject.GetComponent<Renderer>().material.color = new Color(
                gameObject.GetComponent<Renderer>().material.color.r,
                gameObject.GetComponent<Renderer>().material.color.g,
                gameObject.GetComponent<Renderer>().material.color.b,
                Mathf.Lerp(alphaTransparent, alphaOpaque, normalizedTime));

            yield return null;
        }
       gameObject.GetComponent<Renderer>().material = material;
    }
}
