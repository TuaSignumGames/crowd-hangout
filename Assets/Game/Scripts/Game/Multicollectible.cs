using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Multicollectible : Collectible
{
    public MulticollectibleSettings multicollectibleSettings;

    protected MulticollectibleElement[] elements;

    public virtual void Initialize(int elementsCount = 1)
    {
        base.Initialize();

        elements = new MulticollectibleElement[elementsCount];
    }

    public virtual void FixedUpdate()
    {
        if (isCollected)
        {
            ProcessCollecting();

            for (int i = 0; i < multicollectibleSettings.capsules.Length; i++)
            {
                if (multicollectibleSettings.capsules[i].IsBroken)
                {
                    multicollectibleSettings.capsules[i].Update();
                }
            }
        }
    }

    protected virtual void ProcessCollecting() { }

    [System.Serializable]
    public class MulticollectibleSettings
    {
        public MulticollectibleCapsule[] capsules;
        public ScatterData capsuleScatteringSettings;
        [Space]
        public float collectiblePullingSpeed;
        public float collectiblePullingDelay;
        public ScatterData collectibleScatteringSettings;
    }
}