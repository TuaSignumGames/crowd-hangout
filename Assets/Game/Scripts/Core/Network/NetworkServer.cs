using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class NetworkServer
{
    private TcpListener _tcpListener;

    private List<NetworkConnection> _connections;

    private Thread _listeningThread;

    private bool _isLaunched;

    public Action<NetworkConnection, NetworkEntity> OnDataReceived;

    public IPAddress IP { get; private set; }
    public int Port { get; private set; }

    public bool IsLaunched => _isLaunched;

    public NetworkServer(IPAddress ip, int port)
    {
        IP = ip;
        Port = port;
    }

    public NetworkServer(string ip, int port)
    {
        IP = IPAddress.Parse(ip);
        Port = port;
    }

    public void Launch()
    {
        _listeningThread = new Thread(() => ListenToConnections());

        _listeningThread.Start();
    }

    public void Shutdown()
    {
        if (_isLaunched)
        {
            _tcpListener.Stop();

            for (int i = 0; i < _connections.Count; i++)
            {
                _connections[i].Close();
            }

            _isLaunched = false;

            OnDataReceived = null;

            _listeningThread.Abort();

            NetworkManager.Log($" - Server: successfully stopped.".Colorize(Color.cyan));
        }
    }

    public void BroadcastData<T>(T entity, string tag = "")
    {
        NetworkPackage package = new NetworkData().CreatePackage(entity, tag);

        for (int i = 0; i < _connections.Count; i++)
        {
            _connections[i].SendData(package);
        }
    }

    public async Task BroadcastDataAsync<T>(T entity, string tag = "")
    {
        NetworkPackage package = new NetworkData().CreatePackage(entity, tag);

        for (int i = 0; i < _connections.Count; i++)
        {
            await _connections[i].SendDataAsync(package);
        }
    }

    public void SendData<T>(NetworkConnection connection, T entity, string tag = "")
    {
        connection.SendData(entity, tag);
    }

    public async Task SendDataAsync<T>(NetworkConnection connection, T entity, string tag = "")
    {
        await connection.SendDataAsync(entity, tag);
    }

    public void AddConnection(NetworkConnection connection)
    {
        _connections.Add(connection);

        NetworkManager.Log($" - Server: new client connected (UNID: {connection.UNID} / Address: {connection.ClientAddress})".Colorize(Color.cyan));
    }

    public void RemoveConnection(NetworkConnection connection)
    {
        _connections.Remove(connection);

        NetworkManager.Log($" - Server: client (UNID: {connection.UNID}) disconnected.".Colorize(Color.cyan));
    }

    private void ListenToConnections()
    {
        _tcpListener = new TcpListener(IP, Port);
        _connections = new List<NetworkConnection>();

        _tcpListener.Start();

        _isLaunched = true;

        NetworkManager.Log($" - Server: launched, waiting for connections... ({IP} : {Port})".Colorize(Color.cyan));

        try
        {
            while (_isLaunched)
            {
                NetworkConnection newConnection = new NetworkConnection(this, _tcpListener.AcceptTcpClient());

                newConnection.StartProcessing();
            }
        }
        catch (Exception ex)
        {
            if (ex.Message.Length > 0)
            {
                NetworkManager.LogError($" - Server: {ex.Message}");
            }
        }
    }
}

public class NetworkConnection
{
    private TcpClient _tcpClient;

    private NetworkServer _server;

    private NetworkStream _networkStream;
    private NetworkData _networkData;

    private Thread _processingThread;

    private bool _isEstablished;

    public int UNID { get; private set; }

    public EndPoint ClientAddress => _tcpClient.Client.RemoteEndPoint;

    public NetworkConnection(NetworkServer server, TcpClient tcpClient)
    {
        _server = server;
        _tcpClient = tcpClient;

        _networkStream = _tcpClient.GetStream();

        _networkData = new NetworkData(_networkStream);

        UNID = NetworkManager.GetUNID();

        server.AddConnection(this);

        _isEstablished = true;
    }

    public void StartProcessing()
    {
        _processingThread = new Thread(() => ListenToClient());

        _processingThread.Start();
    }

    public void Close()
    {
        if (_isEstablished)
        {
            _server.RemoveConnection(this);

            if (_networkData != null)
            {
                _networkData = null;
            }

            if (_networkStream != null)
            {
                _networkStream.Close();
            }

            if (_tcpClient != null)
            {
                _tcpClient.Close();
            }

            _isEstablished = false;

            _processingThread.Abort();

            //Debug.Log($" - Server: client {ID} processing thread status - {_processingThread.ThreadState}");
        }
    }

    public void SendData<T>(T entity, string tag ="")
    {
        try
        {
            _networkData.SendEntity(entity, tag);
        }
        catch (Exception ex)
        {
            NetworkManager.LogError($" - Server: {ex.Message}");
        }
    }

    public void SendData(NetworkPackage package)
    {
        try
        {
            if (package.size > 0)
            {
                _networkData.SendPackage(package);
            }
            else
            {
                NetworkManager.Log($" --- Server: outcoming data package is empty.".Colorize(Color.cyan));
            }
        }
        catch (Exception ex)
        {
            NetworkManager.LogError($" - Server: {ex.Message}");
        }
    }

    public async Task SendDataAsync<T>(T entity, string tag = "")
    {
        try
        {
            await _networkData.SendEntityAsync(entity, tag);
        }
        catch (Exception ex)
        {
            NetworkManager.LogError($" - Server: {ex.Message}");
        }
    }

    public async Task SendDataAsync(NetworkPackage package)
    {
        try
        {
            if (package.size > 0)
            {
                await _networkData.SendPackageAsync(package);
            }
            else
            {
                NetworkManager.Log($" --- Server: outcoming data package is empty.".Colorize(Color.cyan));
            }
        }
        catch (Exception ex)
        {
            NetworkManager.LogError($" - Server: {ex.Message}");
        }
    }

    private void ListenToClient()
    {
        try
        {
            NetworkPackage receivedPackage = new NetworkPackage();

            while (_tcpClient.Connected && _networkData != null)
            {
                receivedPackage = _networkData.ReceivePackage();

                if (receivedPackage.size > 0)
                {
                    //Debug.Log($" - Server: package ({receivedPackage.size} bytes) received from {ClientAddress}".Colorize(Color.cyan));

                    _server.OnDataReceived(this, _networkData.Unpack(receivedPackage));
                }
            }
        }
        catch (Exception ex)
        {
            if (ex.Message.Length > 0)
            {
                NetworkManager.Log($" --- Server: unexpected connection loss. Client UNID: {UNID} ({ClientAddress}) // Error: {ex.Message}");
            }
        }
        finally
        {
            Close();
        }
    }
}
