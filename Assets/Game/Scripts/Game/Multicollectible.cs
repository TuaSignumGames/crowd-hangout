using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Multicollectible : Collectible
{
    public MulticollectibleSettings multicollectibleSettings;

    protected MotionSimulator[] debrisMotionSimulators;

    public virtual void Initialize(int elementsCount = 1)
    {
        base.Initialize();

        GenerateElements(elementsCount);

        debrisMotionSimulators = new MotionSimulator[multicollectibleSettings.debris.Length];
    }

    protected virtual void GenerateElements(int count) { }

    private void FixedUpdate()
    {
        if (isCollected)
        {
            for (int i = 0; i < debrisMotionSimulators.Length; i++)
            {
                debrisMotionSimulators[i].Update();
            }
        }
    }

    public override void Collect()
    {
        base.Collect();

        if (multicollectibleSettings.capsule)
        {
            multicollectibleSettings.capsule.SetActive(false);
        }

        if (multicollectibleSettings.debris.Length > 0)
        {
            DropDebris();
        }

        if (multicollectibleSettings.destructionVFX)
        {
            multicollectibleSettings.destructionVFX.gameObject.SetActive(true);

            multicollectibleSettings.destructionVFX.Play();
        }
    }

    protected virtual void DropDebris()
    {
        for (int i = 0; i < multicollectibleSettings.debris.Length; i++)
        {
            multicollectibleSettings.debris[i].SetActive(true);

            debrisMotionSimulators[i].velocity = (multicollectibleSettings.debris[i].transform.position - multicollectibleSettings.capsule.transform.position).normalized * Random.Range(1f, 3f) * multicollectibleSettings.destructionPower;
            debrisMotionSimulators[i].angularVelocity = Random.insideUnitSphere * multicollectibleSettings.destructionPower;
        }
    }

    [System.Serializable]
    public class MulticollectibleSettings
    {
        public GameObject capsule;
        public GameObject[] debris;
        public ParticleSystem destructionVFX;
        public float destructionPower;
    }
}