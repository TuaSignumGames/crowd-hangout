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

        Initialize();
    }

    public void Initialize()
    {
        debrisMotionSimulators = new List<MotionSimulator>();
    }

    public void Update()
    {
        if (debrisMotionSimulators != null)
        {
            for (int i = 0; i < debrisMotionSimulators.Count; i++)
            {
                debrisMotionSimulators[i].Update();
            }
        }
    }

    public void Break()
    {
        Break(scatterData);
    }

    public void Break(Vector3 externalImpulse)
    {
        Break(scatterData, externalImpulse);
    }

    public void BreakPartially(Vector3 externalImpulse, Vector3 destructionOrigin, float destructionRadius)
    {
        BreakPartially(scatterData, externalImpulse, destructionOrigin, destructionRadius);
    }

    public void Break(ScatterData scatterData)
    {
        if (capsule)
        {
            capsule.SetActive(false);
        }

        for (int i = 0; i < debris.Length; i++)
        {
            debris[i].SetActive(true);

            debrisMotionSimulators.Add(new MotionSimulator(debris[i].transform, MonoUpdateType.FixedUpdate, scatterData.gravityModifier));

            fractureVector = debrisMotionSimulators[i].Transform.position - capsule.transform.position;

            debrisMotionSimulators[i].velocity = fractureVector.normalized.Multiplied(scatterData.impulseRatio) * scatterData.impulseMagnitudeRange.Value;
            debrisMotionSimulators[i].angularVelocity = Random.insideUnitSphere.normalized * scatterData.angularMomentumRange.Value;
        }

        if (destructionVFX)
        {
            destructionVFX.Play(true);
        }

        AppManager.Instance.PlayHaptic(MoreMountains.NiceVibrations.HapticTypes.LightImpact);

        isBroken = true;
    }

    public void Break(ScatterData scatterData, Vector3 externalImpulse)
    {
        if (capsule)
        {
            capsule.SetActive(false);
        }

        for (int i = 0; i < debris.Length; i++)
        {
            debris[i].SetActive(true);

            debrisMotionSimulators.Add(new MotionSimulator(debris[i].transform, MonoUpdateType.FixedUpdate, scatterData.gravityModifier));

            fractureVector = debrisMotionSimulators[i].Transform.position - capsule.transform.position;

            Debug.Log($" Impulse: {externalImpulse}");

            // TODO Check scatterData values for zero 

            debrisMotionSimulators[i].velocity = externalImpulse * scatterData.externalImpulseFactor + fractureVector.normalized.Multiplied(scatterData.impulseRatio) * scatterData.impulseMagnitudeRange.Value;
            debrisMotionSimulators[i].angularVelocity = Random.insideUnitSphere.normalized * scatterData.angularMomentumRange.Value;

            Debug.Log(debrisMotionSimulators[i].velocity);
        }

        if (destructionVFX)
        {
            destructionVFX.Play(true);
        }

        AppManager.Instance.PlayHaptic(MoreMountains.NiceVibrations.HapticTypes.LightImpact);

        isBroken = true;
    }

    public void BreakPartially(ScatterData scatterData, Vector3 externalImpulse, Vector3 destructionOrigin, float destructionRadius)
    {
        if (capsule)
        {
            capsule.SetActive(false);
        }

        float sqrDestructionRadius = destructionRadius * destructionRadius;
        float sqrDistanceToFracture = 0;

        for (int i = 0; i < debris.Length; i++)
        {
            debris[i].SetActive(true);

            sqrDistanceToFracture = (debris[i].transform.position - destructionOrigin).sqrMagnitude;

            if (sqrDistanceToFracture < sqrDestructionRadius && Random.Range(0, 1f) < (1f - (sqrDistanceToFracture / sqrDestructionRadius)))
            {
                debrisMotionSimulators.Add(new MotionSimulator(debris[i].transform, MonoUpdateType.FixedUpdate, scatterData.gravityModifier));

                fractureVector = debrisMotionSimulators.GetLast().Transform.position - capsule.transform.position;

                debrisMotionSimulators.GetLast().velocity = externalImpulse * scatterData.externalImpulseFactor + fractureVector.normalized.Multiplied(scatterData.impulseRatio) * scatterData.impulseMagnitudeRange.Value;
                debrisMotionSimulators.GetLast().angularVelocity = Random.insideUnitSphere.normalized * scatterData.angularMomentumRange.Value;
            }
        }

        if (destructionVFX)
        {
            destructionVFX.Play(true);
        }

        AppManager.Instance.PlayHaptic(MoreMountains.NiceVibrations.HapticTypes.LightImpact);

        isBroken = true;
    }
}
