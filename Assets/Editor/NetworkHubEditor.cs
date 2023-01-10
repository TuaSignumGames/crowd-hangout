using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using UnityEngine;
using UnityEditor;
using System.Threading.Tasks;

[CustomEditor(typeof(NetworkHub))]
public class NetworkHubEditor : Editor
{
    private static string hubServerAddress;
    private static int hubServerPort;

    public static NetworkServer _hubServer;
    private static IPAddress localServerAddress;

    private NetworkHub _networkHub;

    private string _connectionStatusLabelText;
    private string _connectionButtonLabelText;

    private Color _connectionStatusLabelColor;

    private GUIStyle _connectionSettingsLabelStyle;
    private GUIStyle _connectionStatusLabelStyle;

    private GUIStyle _defaultButtonStyle;

    private bool _guiButtonPressed;
    private bool _isHubOnline;

    private bool IsHubOnline => _hubServer != null && _hubServer.IsLaunched;

    private Color _defaultLabelColor = new Color(0.8235294f, 0.8235294f, 0.8235294f);

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        _networkHub = (NetworkHub)target;

        if (localServerAddress == null)
        {
            localServerAddress = NetworkManager.GetLocalAddress();
        }

        hubServerAddress = localServerAddress.ToString();

        if (EditorPrefs.HasKey(NetworkHub.previousHubServerPortKey))
        {
            hubServerPort = EditorPrefs.GetInt(NetworkHub.previousHubServerPortKey);
        }

        _isHubOnline = IsHubOnline;

        _connectionStatusLabelText = _isHubOnline ? "Online" : "Offline";
        _connectionButtonLabelText = _isHubOnline ? "Disconnect" : "Connect";
        _connectionStatusLabelColor = _isHubOnline ? new Color(0, 0.9f, 0) : _defaultLabelColor;

        //Debug.Log($" --- Hub server: {(_hubServer == null ? "Null" : _hubServer.ToString())} / Status: {_connectionStatusLabelText} / Network servers count: {NetworkManager.ServersCount}");

        if (DrawButton("Push", enabled: _isHubOnline))
        {
            PushPropertiesAsync();
        }

        GUILayout.Space(6f);

        DrawLine();

        DrawConnectionSettings();
    }

    private void LaunchHubServer()
    {
        _hubServer = NetworkManager.LaunchServer(localServerAddress, hubServerPort);

        _hubServer.OnDataReceived += HandleIncomingData;

        EditorPrefs.SetString(NetworkHub.previousHubServerIPAddressKey, hubServerAddress);
        EditorPrefs.SetInt(NetworkHub.previousHubServerPortKey, hubServerPort);
    }

    private void ShutdownHubServer()
    {
        if (IsHubOnline)
        {
            NetworkManager.ShutdownServer(_hubServer);

            _hubServer = null;

            OnInspectorGUI();
        }
    }

    private void HandleIncomingData(NetworkConnection connection, NetworkEntity receivedEntity)
    {
        if (receivedEntity.Tag == NetworkHub.networkHubMessageTag)
        {
            NetworkManager.Log($" - Message from UNID[{connection.UNID}]: \"{receivedEntity.Object}\"".Colorize(Color.cyan));
        }
        else
        {
            NetworkManager.Log($" - Hub Server: data from client (UNID:{connection.UNID}) -- \"{receivedEntity.Object}\"".Colorize(Color.cyan));
        }
    }

    private async Task PushPropertiesAsync()
    {
        NetworkHubPropertyData propertyData;

        for (int i = 0; i < _networkHub.properties.Length; i++)
        {
            propertyData = _networkHub.properties[i].GetData();

            await _hubServer.BroadcastDataAsync(propertyData.value, propertyData.tag);
        }

        await _hubServer.BroadcastDataAsync(NetworkHub.updateRequestMessage, NetworkHub.networkHubMessageTag);
    }

    private void DrawConnectionSettings()
    {
        EditorGUILayout.LabelField("Connection settings", EditorStyles.largeLabel);

        GUILayout.BeginHorizontal();

        GUILayout.Space(3f);

        _connectionSettingsLabelStyle = new GUIStyle();

        _connectionSettingsLabelStyle.normal.textColor = _defaultLabelColor;

        GUILayout.Label("Status: ", _connectionSettingsLabelStyle);

        _connectionStatusLabelStyle = new GUIStyle();

        _connectionStatusLabelStyle.normal.textColor = _connectionStatusLabelColor;
        _connectionStatusLabelStyle.fontStyle = FontStyle.Bold;

        GUILayout.Label(_connectionStatusLabelText, _connectionStatusLabelStyle);

        GUILayout.FlexibleSpace();

        GUILayout.EndHorizontal();

        EditorGUILayout.Space();

        GUILayout.BeginHorizontal();

        GUI.enabled = !_isHubOnline;

        EditorGUIUtility.labelWidth = GUI.skin.label.CalcSize(new GUIContent("IP:")).x;

        hubServerAddress = EditorGUILayout.TextField("IP:", hubServerAddress);

        GUILayout.Space(10f);

        EditorGUIUtility.labelWidth = GUI.skin.label.CalcSize(new GUIContent("Port:")).x;

        hubServerPort = EditorGUILayout.IntField("Port:", hubServerPort);

        GUI.enabled = true;

        GUILayout.EndHorizontal();

        EditorGUILayout.Space();

        if (DrawButton("Reset Port", enabled: !_isHubOnline))
        {
            hubServerPort = 0;

            EditorPrefs.DeleteKey(NetworkHub.previousHubServerPortKey);
        }

        if (DrawButton(_connectionButtonLabelText))
        {
            if (!_isHubOnline)
            {
                LaunchHubServer();
            }
            else
            {
                ShutdownHubServer();
            }
        }
    }

    private bool DrawButton(string text, float height = 25f, bool enabled = true)
    {
        GUI.enabled = enabled;

        _guiButtonPressed = GUILayout.Button(text, GUILayout.Height(height));

        GUI.enabled = true;

        return _guiButtonPressed;
    }

    private bool DrawButton(string text, float width, float height, bool enabled = true)
    {
        GUILayout.BeginHorizontal();

        GUILayout.FlexibleSpace();

        GUI.enabled = enabled;

        _guiButtonPressed = GUILayout.Button(text, GUILayout.Width(width), GUILayout.Height(height));

        GUI.enabled = true;

        GUILayout.FlexibleSpace();

        GUILayout.EndHorizontal();

        return _guiButtonPressed;
    }

    private void DrawLine()
    {
        EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 1), new Color(0.078f, 0.078f, 0.078f));
    }
}
