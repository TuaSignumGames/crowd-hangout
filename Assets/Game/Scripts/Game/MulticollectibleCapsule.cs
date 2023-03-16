using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MulticollectibleCapsule
{
    public GameObject capsule;
    public GameObject[] debris;
    public ParticleSystem destructionVFX;

    private ScatterData scatterData;

    private List<MotionSimulator> debrisMotionSimulators;

    private Vector3 fractureVector;

    private bool isBroken;

    public bool IsBroken => isBroken;

    public MulticollectibleCapsule(GameObject capsule, GameObject[] debris, ParticleSystem destructionVFX, ScatterData scatterData)
    {
        this.capsule = capsule;
        this.debris = debris;
        this.destructionVFX = destructionVFX;
        this.scatterData = scatterData;

        debrisMotionSimulators = new List<MotionSimulator>();
    }

    public void Update()
    {
        for (int i = 0; i < debrisMotionSimulators.Count; i++)
        {
            debrisMotionSimulators[i].Update();
        }
    }

    public void Break()
    {
        capsule.SetActive(false);

        for (int i = 0; i < debris.Length; i++)
        {
            debris[i].SetActive(true);

            debrisMotionSimulators.Add(new MotionSimulator(debris[i].transform, MonoUpdateType.FixedUpdate));

            fractureVector = debrisMotionSimulators[i].Transform.position - capsule.transform.position;

            debrisMotionSimulators[i].velocity = fractureVector.normalized.Multiplied(scatterData.impulseRatio);
            debrisMotionSimulators[i].angularVelocity = Random.insideUnitSphere.normalized * scatterData.angularMomentumRange.Value;
        }

        if (destructionVFX)
        {
            destructionVFX.Play(true);
        }

        isBroken = true;
    }

    public void Break(Vector3 externalImpulse)
    {
        capsule.SetActive(false);

        for (int i = 0; i < debris.Length; i++)
        {
            debris[i].SetActive(true);

            debrisMotionSimulators.Add(new MotionSimulator(debris[i].transform, MonoUpdateType.FixedUpdate));

            fractureVector = debrisMotionSimulators[i].Transform.position - capsule.transform.position;

            debrisMotionSimulators[i].velocity = externalImpulse * scatterData.externalImpulseFactor + fractureVector.normalized.Multiplied(scatterData.impulseRatio);
            debrisMotionSimulators[i].angularVelocity = Random.insideUnitSphere.normalized * scatterData.angularMomentumRange.Value;
        }

        if (destructionVFX)
        {
            destructionVFX.Play(true);
        }

        isBroken = true;
    }

    public void BreakPartially(Vector3 externalImpulse, Vector3 destructionOrigin, float destructionRadius)
    {
        capsule.SetActive(false);

        float sqrDestructionRadius = destructionRadius * destructionRadius;
        float sqrDistanceToFracture = 0;

        for (int i = 0; i < debris.Length; i++)
        {
            debris[i].SetActive(true);

            sqrDistanceToFracture = (debris[i].transform.position - destructionOrigin).sqrMagnitude;

            if (sqrDistanceToFracture < sqrDestructionRadius && Random.Range(0, 1f) < (1f - (sqrDistanceToFracture / sqrDestructionRadius)))
            {
                debrisMotionSimulators.Add(new MotionSimulator(debris[i].transform, MonoUpdateType.FixedUpdate));

                fractureVector = debrisMotionSimulators.GetLast().Transform.position - capsule.transform.position;

                debrisMotionSimulators.GetLast().velocity = externalImpulse * scatterData.externalImpulseFactor + fractureVector.normalized.Multiplied(scatterData.impulseRatio);
                debrisMotionSimulators.GetLast().angularVelocity = Random.insideUnitSphere.normalized * scatterData.angularMomentumRange.Value;
            }
        }

        if (destructionVFX)
        {
            destructionVFX.Play(true);
        }

        isBroken = true;
    }
}
