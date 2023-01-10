using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolingHub : Service<PoolingHub>
{
    public Pooler<UICurrencyIncrement> currencyIncrementPool;

    public override void Initialize()
    {
        base.Initialize();

        currencyIncrementPool.Initialize();
    }

    [System.Serializable]
    public class Pooler<T> where T : Component
    {
        public T original;
        public int capacity;

        public ComponentPool<T> pool;

        public void Initialize()
        {
            pool = new ComponentPool<T>(original, capacity);
        }

        public T Take()
        {
            return pool.Take();
        }
    }
}
