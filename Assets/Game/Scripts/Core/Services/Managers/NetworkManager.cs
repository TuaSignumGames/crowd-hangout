using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;

public enum NetworkProtocolType { TCP, UDP }

public static class NetworkManager
{
    private static List<NetworkServer> servers;
    private static List<NetworkClient> clients;

    private static int UniqueNetworkID;

    public static bool logging;

    public static int ServersCount => servers == null ? 0 : servers.Count;
    public static int ClientsCount => clients == null ? 0 : clients.Count;

    public static NetworkServer LaunchServer(IPAddress ip, int port)
    {
        NetworkServer newServer = null;

        if (servers == null)
        {
            servers = new List<NetworkServer>();
        }

        newServer = servers.Find((s) => s.IP == ip && s.Port == port);

        if (newServer == null)
        {
            newServer = new NetworkServer(ip, port);

            newServer.Launch();

            servers.Add(newServer);
        }
        else
        {
            Log($" - NetworkManager: requested server is already launched ({newServer.IP} : {newServer.Port})".Colorize(Color.cyan));

            return null;
        }

        return newServer;
    }

    public static NetworkClient LaunchClient(IPAddress serverAddress, int serverPort, NetworkProtocolType protocolType, float connectionTimeout)
    {
        NetworkClient newClient = new NetworkClient();

        newClient.Connect(serverAddress, serverPort, connectionTimeout);

        if (clients == null)
        {
            clients = new List<NetworkClient>();
        }

        clients.Add(newClient);

        return newClient;
    }

    public static void ShutdownServer(NetworkServer server)
    {
        if (servers != null && servers.Contains(server))
        {
            server.Shutdown();

            servers.Remove(server);
        }
    }

    public static void DisconnectClient(NetworkClient client)
    {
        if (clients != null && clients.Contains(client))
        {
            client.Disconnect();

            clients.Remove(client);
        }
    }

    public static void DisconnectAll()
    {
        if (clients != null)
        {
            foreach (NetworkClient client in clients)
            {
                client.Disconnect();
            }

            clients.Clear();
        }

        if (servers != null)
        {
            foreach (NetworkServer server in servers)
            {
                server.Shutdown();
            }

            servers.Clear();
        }
    }

    public static void Log(object message, bool forced = false)
    {
        try
        {
            if (logging || forced)
            {
                if (MainThreadHub.Instance)
                {
                    MainThreadHub.InvokeMethod(() => Debug.Log(message));
                }
                else
                {
                    Debug.Log(message);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($" - NetworkManager: {ex}");
        }
    }

    public static void LogError(object message)
    {
        try
        {
            if (MainThreadHub.Instance != null)
            {
                MainThreadHub.InvokeMethod(() => Debug.LogError(message));
            }
            else
            {
                Debug.LogError(message);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($" - NetworkManager: {ex}");
        }
    }

    public static IPAddress GetLocalAddress()
    {
        IPAddress localAddress = null;

        IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());

        foreach (IPAddress address in hostEntry.AddressList)
        {
            if (address.AddressFamily == AddressFamily.InterNetwork)
            {
                localAddress = address;

                break;
            }
        }

        return localAddress;
    }

    public static int GetUNID()
    {
        return UniqueNetworkID++;
    }
}
