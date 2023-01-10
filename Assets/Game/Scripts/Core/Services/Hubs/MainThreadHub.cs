using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class MainThreadHub : Service<MainThreadHub>
{
    private Queue<Action> _executionQueue;

    public override void Initialize()
    {
        _executionQueue = new Queue<Action>();

        base.Initialize();
    }

    private void EnqueueAction(Action action)
    {
        _executionQueue.Enqueue(action);
    }

    private void Update()
    {
        if (IsInitialized)
        {
            while (_executionQueue.Count > 0)
            {
                _executionQueue.Dequeue().Invoke();
            }
        }
    }

    public static void InvokeMethod(Action action)
    {
        if (Instance)
        {
            Instance.EnqueueAction(action);
        }
        else
        {
            throw new Exception("MainThreadHub is Play Mode only available");
        }
    }

    public static Coroutine InvokeCoroutine(IEnumerator routine)
    {
        if (Instance)
        {
            return Instance.StartCoroutine(routine);
        }
        else
        {
            throw new Exception("MainThreadHub is Play Mode only available");
        }
    }
}
