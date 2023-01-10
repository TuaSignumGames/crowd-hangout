using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonoSingleton<T> : MonoBehaviour, IInitializable
{
    private static object _instance;

    public static T Instance { get { return (T)_instance; } }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            Initialize();
        }
    }

    public virtual void Initialize()
    {
        _instance = this;
    }

    public virtual void Deinitialize()
    {
        _instance = null;
    }
}