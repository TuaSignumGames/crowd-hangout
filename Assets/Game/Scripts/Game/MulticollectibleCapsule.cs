using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MulticollectibleCapsule
{
    public GameObject capsule;
    public GameObject[] debris;
    public ParticleSystem destructionVFX;

    private List<MotionSimulator> debrisMotionSimulators;

    private bool isBroken;

    public bool IsBroken => isBroken;

    public MulticollectibleCapsule(GameObject capsule, GameObject[] debris, ParticleSystem destructionVFX)
    {
        this.capsule = capsule;
        this.debris = debris;
        this.destructionVFX = destructionVFX;

        debrisMotionSimulators = new List<MotionSimulator>();
    }

    public void Update()
    {
        for (int i = 0; i < debrisMotionSimulators.Count; i++)
        {
            debrisMotionSimulators[i].Update();
        }
    }

    public void Break(FloatRange impulseRange)
    {
        capsule.SetActive(false);

        for (int i = 0; i < debris.Length; i++)
        {
            debris[i].SetActive(true);

            debrisMotionSimulators.Add(new MotionSimulator(debris[i].transform, MonoUpdateType.FixedUpdate));

            debrisMotionSimulators[i].velocity = (debris[i].transform.position - capsule.transform.position).normalized * impulseRange.Value;
            debrisMotionSimulators[i].angularVelocity = Random.insideUnitSphere.normalized * Random.Range(120f, 720f);
        }

        if (destructionVFX)
        {
            destructionVFX.Play(true);
        }

        isBroken = true;
    }

    public void Break(Vector3 commonImpulse, FloatRange selfImpulseRange)
    {
        capsule.SetActive(false);

        for (int i = 0; i < debris.Length; i++)
        {
            debris[i].SetActive(true);

            debrisMotionSimulators.Add(new MotionSimulator(debris[i].transform, MonoUpdateType.FixedUpdate));

            debrisMotionSimulators[i].velocity = commonImpulse + (debris[i].transform.position - capsule.transform.position).normalized * selfImpulseRange.Value;
            debrisMotionSimulators[i].angularVelocity = Random.insideUnitSphere.normalized * Random.Range(120f, 720f);
        }

        if (destructionVFX)
        {
            destructionVFX.Play(true);
        }

        isBroken = true;
    }

    public void BreakPartially(Vector3 commonImpulse, FloatRange selfImpulseRange, Vector3 destructionOrigin, float destructionRadius)
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

                Debug.Log($" - Simulator {i} / {debris.Length} :: Fracture {i} / {debris.Length}");

                debrisMotionSimulators[i].velocity = commonImpulse + (debris[i].transform.position - capsule.transform.position).normalized * selfImpulseRange.Value;
                debrisMotionSimulators[i].angularVelocity = Random.insideUnitSphere.normalized * Random.Range(120f, 720f);
            }
        }

        if (destructionVFX)
        {
            destructionVFX.Play(true);
        }

        isBroken = true;
    }
}
