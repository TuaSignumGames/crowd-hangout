using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Text;
using UnityEngine;

public class NetworkData
{
    private NetworkStream _connectedStream;

    private BinaryConverter _binaryConverter;

    private byte[] _bufferData;

    private int _bufferSize;

    public NetworkData()
    {
        _binaryConverter = new BinaryConverter();
    }

    public NetworkData(NetworkStream stream, int bufferSize = 4096)
    {
        _binaryConverter = new BinaryConverter();

        _connectedStream = stream;

        _bufferSize = bufferSize;
    }

    public NetworkPackage CreatePackage<T>(T entity, string tag = "")
    {
        NetworkPackage package = new NetworkPackage();

        package.tagBytes = Encoding.Unicode.GetBytes(tag);
        package.tagIndexerByte = (byte)(package.tagBytes.Length + 2);
        package.entityTypeByte = BinaryRepresenter.GetIndexByType(entity.GetType());

        package.entityBytes = new BinaryConverter().GetBytes<T>(entity);

        package.size = 2 + package.tagBytes.Length + package.entityBytes.Length;

        package.streamData = new byte[package.size];

        package.streamData[0] = package.entityTypeByte;
        package.streamData[1] = package.tagIndexerByte;

        for (int i = 0; i < package.tagBytes.Length; i++)
        {
            package.streamData[i + 2] = package.tagBytes[i];
        }

        for (int i = 0; i < package.entityBytes.Length; i++)
        {
            package.streamData[i + package.tagBytes.Length + 2] = package.entityBytes[i];
        }

        #region - Debugging -
        /*
        string tagBytesString = "";
        string entityBytesString = "";
        string packageStreamString = "";

        for (int i = 0; i < _package.tagBytes.Length; i++)
        {
            tagBytesString += $"{_package.tagBytes[i]} ";
        }

        for (int i = 0; i < _package.entityBytes.Length; i++)
        {
            entityBytesString += $"{_package.entityBytes[i]} ";
        }

        for (int i = 0; i < _package.streamData.Length; i++)
        {
            packageStreamString += $"{_package.streamData[i]} ";
        }

        Debug.Log($" - Package created: [{_package.entityTypeByte}]+[{_package.tagIndexerByte}]+{_package.tagBytes.Length}[{tagBytesString}]+{_package.entityBytes.Length}[{entityBytesString}] --- Stream size - {_package.size} bytes");
        Debug.Log($" - Package data stream: {packageStreamString}");
        */
        #endregion

        return package;
    }

    public NetworkEntity Unpack(NetworkPackage package)
    {
        return new NetworkEntity(Encoding.Unicode.GetString(package.tagBytes), BinaryRepresenter.GetTypeByIndex(package.entityTypeByte), _binaryConverter.GetObject(package.entityBytes));
    }

    public NetworkPackage ReceivePackage()
    {
        NetworkPackage package = new NetworkPackage();

        _bufferData = new byte[_bufferSize];

        try
        {
            do
            {
                package.size = _connectedStream.Read(_bufferData, 0, _bufferSize);
            }
            while (_connectedStream.DataAvailable);

            if (package.size > 0)
            {
                package.streamData = new byte[package.size];

                for (int i = 0; i < package.size; i++)
                {
                    package.streamData[i] = _bufferData[i];
                }

                package.entityTypeByte = package.streamData[0];
                package.tagIndexerByte = package.streamData[1];

                package.tagBytes = new List<byte>(package.streamData).GetRange(2, package.tagIndexerByte - 2).ToArray();
                package.entityBytes = new List<byte>(package.streamData).GetRange(package.tagIndexerByte, package.size - package.tagIndexerByte).ToArray();
            }
        }
        catch (Exception ex)
        {
            if (ex.Message.Length > 0)
            {
                NetworkManager.LogError($" - Package receiving issue: {ex.Message}");
            }

            Close();
        }

        return package;
    }

    public async Task<NetworkPackage> ReceivePackageAsync()
    {
        NetworkPackage package = new NetworkPackage();

        _bufferData = new byte[_bufferSize];

        try
        {
            do
            {
                package.size = await _connectedStream.ReadAsync(_bufferData, 0, _bufferSize);
            }
            while (_connectedStream.DataAvailable);

            if (package.size > 0)
            {
                package.streamData = new byte[package.size];

                for (int i = 0; i < package.size; i++)
                {
                    package.streamData[i] = _bufferData[i];
                }

                package.entityTypeByte = package.streamData[0];
                package.tagIndexerByte = package.streamData[1];

                package.tagBytes = new List<byte>(package.streamData).GetRange(2, package.tagIndexerByte - 2).ToArray();
                package.entityBytes = new List<byte>(package.streamData).GetRange(package.tagIndexerByte, package.size - package.tagIndexerByte).ToArray();
            }
        }
        catch (Exception ex)
        {
            if (ex.Message.Length > 0)
            {
                NetworkManager.LogError($" - Package receiving issue: {ex.Message}");
            }

            Close();
        }

        return package;
    }

    public void SendPackage(NetworkPackage package)
    {
        _connectedStream.Write(package.streamData, 0, package.size);
    }

    public async Task SendPackageAsync(NetworkPackage package)
    {
        await _connectedStream.WriteAsync(package.streamData, 0, package.size);
    }

    public void SendEntity<T>(T entity, string tag = "")
    {
        SendPackage(CreatePackage(entity, tag));
    }

    public async Task SendEntityAsync<T>(T entity, string tag = "")
    {
        await SendEntityAsync(CreatePackage(entity, tag));
    }

    public void Close()
    {
        _connectedStream.Close();
    }
}

public struct NetworkEntity
{
    private string _tag;
    private object _obj;

    private Type _type;

    public string Tag => _tag;
    public object Object => _obj;

    public Type Type => _type;

    public NetworkEntity(string tag, Type type, object obj)
    {
        _tag = tag;
        _type = type;
        _obj = obj;
    }
}

// - Package format: [dataTypeByte]+[tagIndexerByte]+N[tagBytes]+M[entityBytes]

public struct NetworkPackage
{
    public byte entityTypeByte;
    public byte tagIndexerByte;

    public byte[] tagBytes;
    public byte[] entityBytes;

    public byte[] streamData;

    public int size;
}
