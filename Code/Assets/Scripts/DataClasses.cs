using System;

[Serializable]
public class RailIndication
{
    public float timestamp = 0;
    public float course = 0;
}

[Serializable]
public class PublishData
{
    public int id;
    public string topic;
    public string message;
}