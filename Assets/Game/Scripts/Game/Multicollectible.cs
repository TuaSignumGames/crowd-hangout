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

        GenerateElements(elementsCount);
    }

    protected virtual void GenerateElements(int count)
    {
        elements = new MulticollectibleElement[count];
    }

    public virtual void FixedUpdate()
    {
        if (isCollected)
        {
            PullElements();

            for (int i = 0; i < multicollectibleSettings.capsules.Length; i++)
            {
                if (multicollectibleSettings.capsules[i].IsBroken)
                {
                    multicollectibleSettings.capsules[i].Update();
                }
            }
        }
    }

    public override void Collect()
    {
        base.Collect();

        StartCoroutine(CollectingCoroutine());
    }

    protected virtual void BreakCapsules() { }

    protected virtual void PullElements() { }

    protected virtual IEnumerator CollectingCoroutine()
    {
        if (multicollectibleSettings.capsules.Length > 0)
        {
            BreakCapsules();
        }

        yield return null;
    }

    [System.Serializable]
    public class MulticollectibleSettings
    {
        public MulticollectibleCapsule[] capsules;
        public float destructionPower;
        [Space]
        public Vector2 collectibleSpeedRange;
        public Vector2 collectibleAccelerationRange;
        public Vector2 collectiblePullingDelayRange;

        public float CollectibleSpeed => Random.Range(collectibleSpeedRange.x, collectibleSpeedRange.y);
        public float CollectibleAcceleration => Random.Range(collectibleAccelerationRange.x, collectibleAccelerationRange.y);
        public float CollectiblePullingDelay => Random.Range(collectiblePullingDelayRange.x, collectiblePullingDelayRange.y);
    }
}