using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

public class NetworkingTester : MonoBehaviour
{
    private NetworkServer _server;

    private NetworkClient _client;

    private void Start()
    {
        IPAddress hostAddress = IPAddress.Parse("192.168.1.6");
        int hostPort = 8585;

        if (Application.isEditor)
        {
            _server = NetworkManager.LaunchServer(hostAddress, hostPort);
            _server.OnDataReceived += HandleDataFromClients;
        }
        //else
        {
            _client = NetworkManager.LaunchClient(hostAddress, hostPort, NetworkProtocolType.TCP, 3f);
            _client.OnDataReceived += HandleDataFromServer;

            _client.SendData("Hi server :)", "MSG");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _server.Shutdown();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            _client.SendData("Hi server :)", "MSG");
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            _server.BroadcastData("Hi there guys!", "MSG");
        }
    }

    private void HandleDataFromServer(NetworkEntity receivedEntity)
    {
        switch (receivedEntity.Tag)
        {
            case "MSG":
                Debug.Log($" - Message from Server: '{Convert.ChangeType(receivedEntity.Object, receivedEntity.Type)}'");
                break;
        }
    }

    private void HandleDataFromClients(NetworkConnection connection, NetworkEntity receivedEntity)
    {
        switch (receivedEntity.Tag)
        {
            case "MSG":
                Debug.Log($" - Message from Client (UNID: {connection.UNID}): '{Convert.ChangeType(receivedEntity.Object, receivedEntity.Type)}'");

                _server.SendData(connection, $"Welcome to server, client with UNID '{connection.UNID}'!", "MSG");

                break;
        }
    }

    private void OnApplicationQuit()
    {
        if (_server != null)
        {
            _server.Shutdown();
        }

        if (_client != null)
        {
            _client.Disconnect();
        }

        NetworkManager.DisconnectAll();
    }
}