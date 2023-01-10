using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class NetworkHub : Service<NetworkHub>
{
    public NetworkHubProperty[] properties;

    public Action OnPropertiesUpdated;

    private NetworkClient _hubClient;

    [HideInInspector] public string hubServerAddress;
    [HideInInspector] public int hubServerPort;

    public const string networkHubMessageTag = "MSG.NetHub";

    public const string updateRequestMessage = "UPD";

    public const string previousHubServerIPAddressKey = "NetHub.NS.IP";
    public const string previousHubServerPortKey = "NetHub.NS.Port";

    public void Connect(string serverAddress, int serverPort, Action onConnectedCallback = null, Action onFailedCallback = null)
    {
        hubServerAddress = serverAddress;
        hubServerPort = serverPort;

        StartCoroutine(ConnectingCoroutine(hubServerAddress, hubServerPort, 3f, 1f, onConnectedCallback: onConnectedCallback, onFailedCallback: onFailedCallback));
    }

    public void Disconnect(Action onDisconnectedCallback = null)
    {
        NetworkManager.DisconnectClient(_hubClient);

        if (onDisconnectedCallback != null)
        {
            onDisconnectedCallback();
        }

        NetworkManager.Log($" - Hub Client: disconnected".Colorize(Color.cyan), true);
    }

    public NetworkHubProperty GetProperty(string tag)
    {
        for (int i = 0; i < properties.Length; i++)
        {
            if (properties[i].tag == tag)
            {
                return properties[i];
            }
        }

        throw new Exception($"Property with tag '{tag}' is not found");
    }

    public void SendMessageToServer(string text)
    {
        _hubClient.SendData(text, networkHubMessageTag);
    }

    private void HandleIncomingData(NetworkEntity receivedEntity)
    {
        NetworkManager.Log($" - Hub Client: data received - tag: {receivedEntity.Tag}, type: {receivedEntity.Type}, value: {receivedEntity.Object}".Colorize(Color.cyan), true);

        if (receivedEntity.Tag == networkHubMessageTag)
        {
            if ((string)receivedEntity.Object == updateRequestMessage)
            {
                OnPropertiesUpdated();
            }
        }
        else
        {
            for (int i = 0; i < properties.Length; i++)
            {
                if (properties[i].tag == receivedEntity.Tag)
                {
                    properties[i].SetValue(receivedEntity.Object);
                }
            }
        }

        SendMessageToServer($"I've received the value: '{receivedEntity.Object}' (tag: '{receivedEntity.Tag}')");
    }

    private IEnumerator ServerSearchingCoroutine(int attemptsLimit, float connectionTimeout)
    {
        _hubClient = new NetworkClient();

        if (PlayerPrefs.HasKey(previousHubServerIPAddressKey))
        {
            yield return StartCoroutine(ConnectingCoroutine(PlayerPrefs.GetString(previousHubServerIPAddressKey), hubServerPort, connectionTimeout));
        }

        string hubClientAddress = NetworkManager.GetLocalAddress().ToString();

        string hubServerAddress = "";
        string hubServerAddressBlank = hubClientAddress.Remove(hubClientAddress.Length - 1);

        int attemptsCounter = 0;

        while (!_hubClient.IsConnected && attemptsCounter < attemptsLimit)
        {
            hubServerAddress = hubServerAddressBlank + attemptsCounter++;

            NetworkManager.Log($" - Hub Client: searching for server - attempt {attemptsCounter}/{attemptsLimit}:".Colorize(Color.cyan), true);

            yield return StartCoroutine(ConnectingCoroutine(hubServerAddress, hubServerPort, connectionTimeout));
        }

        if (_hubClient.IsConnected)
        {

        }
        else
        {
            NetworkManager.Log($" - Hub Client: Hub Server is not found".Colorize(Color.cyan), true);
        }

        yield return null;
    }

    private IEnumerator ConnectingCoroutine(string serverAddress, int serverPort, float connectionTimeout, float connectionDelay = 0, Action onConnectedCallback = null, Action onFailedCallback = null)
    {
        if (connectionDelay > 0)
        {
            yield return new WaitForSeconds(connectionDelay);
        }

        NetworkManager.Log($" - Hub Client: connecting to {serverAddress} : {serverPort} ...".Colorize(Color.cyan), true);

        try
        {
            _hubClient = NetworkManager.LaunchClient(IPAddress.Parse(serverAddress), serverPort, NetworkProtocolType.TCP, connectionTimeout);
        }
        catch (Exception ex)
        {
            NetworkManager.LogError($" - Hub Client: {ex.Message}");
        }

        float connectionAttemptTime = Time.time;

        while (!_hubClient.IsConnected)
        {
            if (Time.time - connectionAttemptTime >= connectionTimeout)
            {
                NetworkManager.Log($" - Hub Client: connection timeout ({serverAddress} : {serverPort})".Colorize(Color.cyan), true);

                NetworkManager.DisconnectClient(_hubClient);

                if (onFailedCallback != null)
                {
                    onFailedCallback();
                }

                yield break;
            }

            yield return null;
        }

        if (_hubClient.IsConnected)
        {
            _hubClient.OnDataReceived += HandleIncomingData;

            if (onConnectedCallback != null)
            {
                onConnectedCallback();
            }

            NetworkManager.Log($" - Hub Client: connection success ({serverAddress} : {serverPort})".Colorize(Color.cyan), true);
        }

        yield return null;
    }

    private void OnApplicationQuit()
    {
        NetworkManager.DisconnectClient(_hubClient);
    }
}

public enum PropertyType { Int, Float, String, AnimationCurve }

[Serializable]
public class NetworkHubProperty
{
    public string tag;
    public PropertyType targetValue;
    [Space]
    public int intValue;
    public float floatValue;
    public string stringValue;
    public AnimationCurve animationCurve;

    public void SetValue(object value)
    {
        switch (targetValue)
        {
            case PropertyType.Int:
                intValue = (int)value;
                break;

            case PropertyType.Float:
                floatValue = (float)value;
                break;

            case PropertyType.String:
                stringValue = (string)value;
                break;

            case PropertyType.AnimationCurve:
                animationCurve = (AnimationCurve)value;
                break;
        }
    }

    public NetworkHubPropertyData GetData()
    {
        switch (targetValue)
        {
            case PropertyType.Int:
                return new NetworkHubPropertyData(tag, intValue);

            case PropertyType.Float:
                return new NetworkHubPropertyData(tag, floatValue);

            case PropertyType.String:
                return new NetworkHubPropertyData(tag, stringValue);

            case PropertyType.AnimationCurve:
                return new NetworkHubPropertyData(tag, animationCurve);
        }

        return new NetworkHubPropertyData();
    }
}

public struct NetworkHubPropertyData
{
    public string tag;
    public object value;

    public NetworkHubPropertyData(string tag, object value)
    {
        this.tag = tag;
        this.value = value;
    }
}