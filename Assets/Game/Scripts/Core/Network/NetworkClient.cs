using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class NetworkClient
{
    private TcpClient _tcpClient;

    private NetworkStream _networkStream;
    private NetworkData _networkData;

    private Thread _connectingThread;
    private Thread _listeningThread;

    private bool _isConnected;

    public Action<NetworkEntity> OnDataReceived;

    public int UNID { get; private set; }

    public bool IsConnected => _isConnected;

    public void Connect(IPAddress ip, int port, float timeout)
    {
        ConnectToServerAsync(ip, port, timeout);

        /*
        _connectingThread = new Thread(() => ConnectToServer(ip, port, timeout));

        _connectingThread.Start();
        */
    }

    public void Connect(string ip, int port, float timeout)
    {
        Connect(IPAddress.Parse(ip), port, timeout);
    }

    public void SendData<T>(T entity, string tag = "")
    {
        try
        {
            if (_networkData != null)
            {
                _networkData.SendEntity(entity, tag);
            }
        }
        catch (Exception ex)
        {
            NetworkManager.LogError($" - Client: {ex.Message}");
        }
    }

    public void Disconnect()
    {
        if (_isConnected)
        {
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

            _isConnected = false;

            OnDataReceived = null;

            _listeningThread.Abort();

            NetworkManager.Log($" - Client: disconnected".Colorize(Color.white));
        }
    }

    private void ConnectToServer(IPAddress ip, int port, float timeout)
    {
        _tcpClient = new TcpClient();
        _listeningThread = new Thread(() => ListenToServer());

        try
        {
            IAsyncResult connectionResult = _tcpClient.BeginConnect(ip, port, null, null);

            if (connectionResult.AsyncWaitHandle.WaitOne(Mathf.RoundToInt(timeout * 1000)))
            {
                _tcpClient.EndConnect(connectionResult);
            }

            _networkStream = _tcpClient.GetStream();

            _networkData = new NetworkData(_networkStream);

            _listeningThread.Start();

            _isConnected = true;
        }
        catch (Exception ex)
        {
            NetworkManager.LogError($" - Client: {ex.Message}");

            Disconnect();
        }
        finally
        {
            _connectingThread.Abort();
        }
    }

    private async Task ConnectToServerAsync(IPAddress ip, int port, float timeout)
    {
        _tcpClient = new TcpClient();
        _listeningThread = new Thread(() => ListenToServer());

        try
        {
            await _tcpClient.ConnectAsync(ip, port);

            _networkStream = _tcpClient.GetStream();

            _networkData = new NetworkData(_networkStream);

            _listeningThread.Start();

            _isConnected = true;
        }
        catch (Exception ex)
        {
            NetworkManager.LogError($" - Client: {ex.Message}");

            Disconnect();
        }
        finally
        {
            _connectingThread.Abort();
        }
    }

    private void ListenToServer()
    {
        try
        {
            NetworkManager.Log($" - Client: connection success ({_tcpClient.Client.RemoteEndPoint})".Colorize(Color.white));

            NetworkPackage receivedPackage = new NetworkPackage();

            while (_networkData != null)
            {
                receivedPackage = _networkData.ReceivePackage();

                if (receivedPackage.size > 0)
                {
                    NetworkManager.Log($" - Client: package ({receivedPackage.size} bytes) received from server.".Colorize(Color.white));

                    if (OnDataReceived != null)
                    {
                        OnDataReceived(_networkData.Unpack(receivedPackage));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            if (ex.Message.Length > 0)
            {
                NetworkManager.LogError($" - Client: connection lost ({ex.Message})");
            }

            Disconnect();
        }
    }
}
