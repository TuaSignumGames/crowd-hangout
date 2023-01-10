using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructionAgent : DestructibleEntity
{
    [Space]
    public GameObject originalContainer;
    public GameObject fracturesContainer;
    [Space]
    public Rigidbody originalRigidbody;
    public Rigidbody[] fractureRigidbodies;
    [Space]
    public Collider originalCollider;
    public ParticleSystem destructionVFX;
    [Space]
    public Vector2 fractureImpulseRange;
    public Vector2 disappearanceDelayRange;
    [Space]
    public bool autoInitialize = true;

    private GameObject actualFracture;

    private TransformEvaluator[] fractureEvaluators;

    private Vector3[] fractureImpulses;
    private Vector3[] fractureAngularImpulses;

    private float[] disappearanceDelays;

    private void Awake()
    {
        if (autoInitialize)
        {
            Initialize();
        }
    }

    public override void Initialize()
    {
        fractureEvaluators = new TransformEvaluator[fractureRigidbodies.Length];

        fractureImpulses = new Vector3[fractureRigidbodies.Length];
        fractureAngularImpulses = new Vector3[fractureRigidbodies.Length];

        disappearanceDelays = new float[fractureRigidbodies.Length];

        for (int i = 0; i < fractureRigidbodies.Length; i++)
        {
            fractureEvaluators[i] = new TransformEvaluator(fractureRigidbodies[i].transform, MonoUpdateType.FixedUpdate);

            fractureEvaluators[i].disableOnZeroScale = true;

            fractureImpulses[i] = (fractureRigidbodies[i].transform.position - fracturesContainer.transform.position).normalized * Random.Range(fractureImpulseRange.x, fractureImpulseRange.y);
            fractureAngularImpulses[i] = Random.insideUnitSphere * Random.Range(fractureImpulseRange.x, fractureImpulseRange.y);

            disappearanceDelays[i] = Random.Range(disappearanceDelayRange.x, disappearanceDelayRange.y);
        }
    }

    private void FixedUpdate()
    {
        if (isDestroyed)
        {
            for (int i = 0; i < fractureEvaluators.Length; i++)
            {
                if (fractureRigidbodies[i].gameObject.activeSelf)
                {
                    fractureEvaluators[i].Iterate();
                }
            }
        }
    }

    public override void Destruct(Vector3 externalImpulse = default)
    {
        if (originalCollider)
        {
            originalCollider.enabled = false;
        }

        if (originalRigidbody)
        {
            originalRigidbody.isKinematic = true;
        }

        originalContainer.SetActive(false);
        fracturesContainer.SetActive(true);

        destructionVFX.Play();

        AppManager.Instance.PlayHaptic(MoreMountains.NiceVibrations.HapticTypes.MediumImpact);

        for (int i = 0; i < fractureRigidbodies.Length; i++)
        {
            fractureRigidbodies[i].velocity = fractureImpulses[i] + externalImpulse;
            fractureRigidbodies[i].angularVelocity = fractureAngularImpulses[i];

            actualFracture = fractureRigidbodies[i].gameObject;

            fractureEvaluators[i].SetDelay(disappearanceDelays[i]);
            fractureEvaluators[i].Scale(Vector3.zero, 0.5f, EvaluationType.Linear);
        }

        isDestroyed = true;
    }
}
