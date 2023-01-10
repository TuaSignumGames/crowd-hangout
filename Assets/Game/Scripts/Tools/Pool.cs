using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pool<T>
{
    protected Queue<T> elementsQueue;

    protected List<T> shuffleCollection;

    protected T pooledElement;

    public Pool(IList<T> elements, bool shuffle = false)
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
    }

    public virtual T Eject()
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
