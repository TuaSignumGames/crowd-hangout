using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

public class BinaryConverter
{
    private BinaryFormatter _binaryFormatter;
    private MemoryStream _memoryStream;

    private object _serializableObject;

    public byte[] GetBytes<T>(object entityObject)
    {
        _binaryFormatter = new BinaryFormatter();

        _serializableObject = TrySerialize(entityObject);

        if (_serializableObject != null)
        {
            using (_memoryStream = new MemoryStream())
            {
                _binaryFormatter.Serialize(_memoryStream, _serializableObject);

                return _memoryStream.ToArray();
            }
        }

        throw new Exception();
    }

    public object GetObject(byte[] data)
    {
        _binaryFormatter = new BinaryFormatter();

        using (_memoryStream = new MemoryStream(data))
        {
            return TryDeserialize(_binaryFormatter.Deserialize(_memoryStream));
        }
    }

    private object TrySerialize(object entityObject)
    {
        //Debug.Log($" - BinaryConverter: serializing - {entityObject.GetType()}");

        if (Serializables.IsDataSerializable(entityObject))
        {
            if (entityObject.GetType().IsPrimitive())
            {
                return entityObject;
            }
            else
            {
                if (entityObject.GetType() == typeof(Vector3))
                {
                    return new SerializableVector3((Vector3)entityObject);
                }
                if (entityObject.GetType() == typeof(AnimationCurve))
                {
                    return new SerializableAnimationCurve((AnimationCurve)entityObject);
                }
            }
        }
        else
        {
            throw new Exception($" - BinaryConverter: attempt to serialize non-serializable data ({entityObject.GetType()})");
        }

        return null;
    }

    private object TryDeserialize(object entityObject)
    {
        //Debug.Log($" - BinaryConverter: deserializing - {entityObject.GetType()}");

        try
        {
            if (entityObject.GetType().IsPrimitive())
            {
                return entityObject;
            }
            else
            {
                if (entityObject.GetType() == typeof(SerializableVector3))
                {
                    return ((SerializableVector3)entityObject).Deserialize();
                }
                if (entityObject.GetType() == typeof(SerializableAnimationCurve))
                {
                    return ((SerializableAnimationCurve)entityObject).Deserialize();
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($" - BinaryConverter: deserialization issue - {ex.Message} ({entityObject.GetType()})");
        }

        return null;
    }
}
