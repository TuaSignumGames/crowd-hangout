using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentPool<T> where T : Component
{
    protected Queue<T> elementsQueue;

    protected T pooledElement;

    public ComponentPool(T original, int capacity)
    {
        elementsQueue = new Queue<T>(capacity);

        for (int i = 0; i < capacity; i++)
        {
            elementsQueue.Enqueue(i == 0 ? original : MonoBehaviour.Instantiate(original, original.transform.parent));
        }
    }

    public virtual T Take()
    {
        if (elementsQueue.Count > 0)
        {
            pooledElement = elementsQueue.Dequeue();

            elementsQueue.Enqueue(pooledElement);

            return pooledElement;
        }

        throw new Exception("Requested pool is empty.");
    }
}