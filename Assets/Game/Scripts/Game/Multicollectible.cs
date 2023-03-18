using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Multicollectible : Collectible
{
    public MulticollectibleSettings multicollectibleSettings;

    protected MulticollectibleElement[] elements;

    protected Vector3 elementImpulse;
    protected Vector3 elementAngularMomentum;

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

    public void DropElement(MulticollectibleElement element, Vector3 origin, Vector3 externalImpulse)
    {
        if (elements.Contain(element))
        {
            elementImpulse = (element.Transform.position - origin).normalized.Multiplied(multicollectibleSettings.collectibleScatteringSettings.impulseRatio) + new Vector3(externalImpulse.x, externalImpulse.y > 0 ? externalImpulse.y : 0, externalImpulse.z) * multicollectibleSettings.collectibleScatteringSettings.externalImpulseFactor;
            elementAngularMomentum = Random.insideUnitSphere.normalized * multicollectibleSettings.collectibleScatteringSettings.angularMomentumRange.Value;

            element.Drop(elementImpulse, elementAngularMomentum);
        }
        else
        {
            throw new UnityException("Required element is not found");
        }
    }

    public void DropElements(Vector3 origin, Vector3 externalImpulse)
    {
        for (int i = 0; i < elements.Length; i++)
        {
            elementImpulse = (elements[i].Transform.position - origin).normalized.Multiplied(multicollectibleSettings.collectibleScatteringSettings.impulseRatio) + new Vector3(externalImpulse.x, externalImpulse.y > 0 ? externalImpulse.y : 0, externalImpulse.z) * multicollectibleSettings.collectibleScatteringSettings.externalImpulseFactor;
            elementAngularMomentum = Random.insideUnitSphere.normalized * multicollectibleSettings.collectibleScatteringSettings.angularMomentumRange.Value;

            elements[i].Drop(elementImpulse, elementAngularMomentum);
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