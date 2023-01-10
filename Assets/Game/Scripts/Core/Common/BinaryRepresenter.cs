using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BinaryRepresenter
{
    private static Type[] _representableTypes = new Type[] { typeof(byte), typeof(int), typeof(double), typeof(float), typeof(char), typeof(string), typeof(Vector3), typeof(AnimationCurve) };

    public static Type GetTypeByIndex(byte id)
    {
        try
        {
            return _representableTypes[id];
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);

            return null;
        }
    }

    public static byte GetIndexByType(Type type)
    {
        try
        {
            for (int i = 0; i < _representableTypes.Length; i++)
            {
                if (type == _representableTypes[i])
                {
                    return (byte)i;
                }
            }

            return 0;
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);

            return 0;
        }
    }
}
