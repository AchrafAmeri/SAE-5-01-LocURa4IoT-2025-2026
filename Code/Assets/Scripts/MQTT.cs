using System;
using System.Security.Authentication;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Server;
using UnityEngine;

public class MQTT : MonoBehaviour
{
    private IMqttClient mqttClient;
    private MqttClientOptions mqttOptions;
    [SerializeField] TopicSubscribeHandling handler;
    [SerializeField] ConnexionIndicator Indicator;
    [SerializeField] GameObject connectionPanel;
    [SerializeField] GameObject keyboard;
    public async Task ConnectAsync(string brokerIp = "192.168.7.13", int port = 1883, string userName = "", string password = "")
    {
        Debug.Log($"Tentative de connexion à {brokerIp}");
        var mqttFactory = new MqttFactory();
        mqttClient = mqttFactory.CreateMqttClient();
        mqttClient.ApplicationMessageReceivedAsync += async e =>
        {
            try
            {
                string topic = e.ApplicationMessage.Topic;
                string payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment.ToArray());
                Debug.Log($"[MQTT] Message reçu sur {topic} : {payload}");
                // Important : traiter sur le main thread
                UnityMainThreadDispatcher.Enqueue(async () =>
                {
                    await handler.HandleIncomingMessage(topic, payload);
                });
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MQTT] Erreur dans le handler de message : {ex.Message}");
            }
            await Task.CompletedTask;
        };
        mqttOptions = new MqttClientOptionsBuilder()
            .WithTcpServer(brokerIp, port)
            .WithCredentials(userName, password)
            .Build();

        if (Indicator != null)
        {
            mqttClient.ConnectedAsync += async e =>
            {
                Debug.Log("MQTT connecté");
                UnityMainThreadDispatcher.Enqueue(() => Indicator.UpdateConnectionIndicator(true));
                UnityMainThreadDispatcher.Enqueue(() => connectionPanel.SetActive(false));
                UnityMainThreadDispatcher.Enqueue(() => keyboard.SetActive(false));
                await Task.CompletedTask;
            };

            mqttClient.DisconnectedAsync += async e =>
            {
                Debug.Log("MQTT déconnecté");
                UnityMainThreadDispatcher.Enqueue(() => Indicator.UpdateConnectionIndicator(false));
                UnityMainThreadDispatcher.Enqueue(() => connectionPanel.SetActive(true));
                UnityMainThreadDispatcher.Enqueue(() => keyboard.SetActive(true));
                await Task.CompletedTask;
            };
        }
        try
        {
            await mqttClient.ConnectAsync(mqttOptions, CancellationToken.None);
            Debug.Log("[MQTT] Connecté au broker.");
            Indicator.UpdateConnectionIndicator(true);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[MQTT] Erreur de connexion : {ex.Message}");
        }
    }

    public async Task ConnectTLSAsync(string brokerIp = "192.168.7.13", int port = 1883, string userName = "", string password = "")
    {
        Debug.Log($"Tentative de connexion à {brokerIp}");
        var mqttFactory = new MqttFactory();
        mqttClient = mqttFactory.CreateMqttClient();
        mqttClient.ApplicationMessageReceivedAsync += async e =>
        {
            try
            {
                string topic = e.ApplicationMessage.Topic;
                string payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment.ToArray());
                Debug.Log($"[MQTT] Message reçu sur {topic} : {payload}");
                // Important : traiter sur le main thread
                UnityMainThreadDispatcher.Enqueue(async () =>
                {
                    await handler.HandleIncomingMessage(topic, payload);
                });
            }
            catch (Exception ex)
            {
                Debug.LogError($"[MQTT] Erreur dans le handler de message : {ex.Message}");
            }
            await Task.CompletedTask;
        };
        mqttOptions = new MqttClientOptionsBuilder()
            .WithTcpServer(brokerIp, port)
            .WithCredentials(userName, password)
            .WithTlsOptions(
            tls =>
            {
                tls.UseTls()
                .WithSslProtocols(SslProtocols.Tls12);
            }
            )
            .Build();

        if (Indicator != null)
        {
            mqttClient.ConnectedAsync += async e =>
            {
                Debug.Log("MQTT connecté");
                UnityMainThreadDispatcher.Enqueue(() => Indicator.UpdateConnectionIndicator(true));
                UnityMainThreadDispatcher.Enqueue(() => connectionPanel.SetActive(false));
                await Task.CompletedTask;
            };

            mqttClient.DisconnectedAsync += async e =>
            {
                Debug.Log("MQTT déconnecté");
                UnityMainThreadDispatcher.Enqueue(() => Indicator.UpdateConnectionIndicator(false));
                UnityMainThreadDispatcher.Enqueue(() => connectionPanel.SetActive(true));
                await Task.CompletedTask;
            };
        }
        try
        {
            await mqttClient.ConnectAsync(mqttOptions, CancellationToken.None);
            Debug.Log("[MQTT] Connecté au broker.");
            Indicator.UpdateConnectionIndicator(true);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[MQTT] Erreur de connexion : {ex.Message}");
        }
    }


    public async Task SubscribeAsync(string topic = "testbed/node/183/state")
    {
        if (mqttClient == null || !mqttClient.IsConnected)
        {
            Debug.LogWarning("[MQTT] Le client n'est pas connecté.");
            return;
        }

        var mqttFactory = new MqttFactory();
        var subscribeOptions = mqttFactory.CreateSubscribeOptionsBuilder()
        .WithTopicFilter(topic)
        .Build();

        await mqttClient.SubscribeAsync(subscribeOptions, CancellationToken.None);
        Debug.Log($"[MQTT] Abonné au topic : {topic}");
    }
    public async Task DisconnectAsync()
    {
        if (mqttClient != null && mqttClient.IsConnected)
        {
            await mqttClient.DisconnectAsync();
            Debug.Log("[MQTT] Déconnecté du broker.");
        }
    }
    public async Task PublishAsync(string topic, string message)
    {
        Debug.Log($"Publishing : {message}");
        if (mqttClient == null || !mqttClient.IsConnected)
        {
            Debug.Log("[MQTT] Déconnecté du broker.");
            return;
        }
        var payload = Encoding.UTF8.GetBytes(message);
        var Publish = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(payload)
            .Build();
        await mqttClient.PublishAsync(Publish);
        Debug.Log($"Message : {message} envoyé au topic:{topic}");
    }
    private async void OnApplicationQuit()
    {
        await DisconnectAsync();
    }
}
