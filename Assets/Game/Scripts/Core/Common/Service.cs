using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Service<T> : MonoSingleton<T>, IService
{
    private bool _isInitialized;

    public bool IsInitialized => _isInitialized;

    public static Action OnInitialized;

    public override void Initialize()
    {
        base.Initialize();

        transform.SetParent(null);

        DontDestroyOnLoad(gameObject);

        _isInitialized = true;

        if (OnInitialized != null)
        {
            OnInitialized();
        }

        print($" - Service initialized: {GetType()}");
    }
}
