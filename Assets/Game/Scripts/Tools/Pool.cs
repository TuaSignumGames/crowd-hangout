using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pool<T>
{
    protected Queue<T> elementsQueue;

    protected List<T> shuffleCollection;

    protected T pooledElement;

    protected T[] pooledRange;

    protected bool consumable;

    public int Count => elementsQueue.Count;

    public Pool(IList<T> elements, bool consumable = false, bool shuffle = false)
    {
        if (shuffle)
        {
            elementsQueue = new Queue<T>();

            shuffleCollection = new List<T>(elements);

            while (shuffleCollection.Count > 0)
            {
                elementsQueue.Enqueue(shuffleCollection.CutRandom());
            }
        }
        else
        {
            elementsQueue = new Queue<T>(elements);
        }

        this.consumable = consumable;
    }

    public virtual T Peek()
    {
        if (elementsQueue.Count > 0)
        {
            return elementsQueue.Peek();
        }

        throw new Exception("Requested pool is empty.");
    }

    public virtual T Eject()
    {
        if (elementsQueue.Count > 0)
        {
            pooledElement = elementsQueue.Dequeue();

            if (!consumable)
            {
                elementsQueue.Enqueue(pooledElement);
            }

            return pooledElement;
        }

        throw new Exception("Requested pool is empty.");
    }

    public virtual T[] EjectRange(int count)
    {
        pooledRange = new T[count];

        for (int i = 0; i < count; i++)
        {
            pooledRange[i] = Eject();
        }

        return pooledRange;
    }
}
