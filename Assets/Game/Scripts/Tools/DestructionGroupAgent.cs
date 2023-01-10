using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructionGroupAgent : DestructibleEntity
{
    [Space]
    public DestructionAgent[] destructionAgents;
    [Space]
    public Rigidbody groupRigidbody;
    public Collider groupCollider;
    public GameObject[] originalElements;
    [Space]
    public Vector2 fractureImpulseRange;
    public Vector2 disappearanceDelayRange;

    private void Awake()
    {
        Initialize();
    }

    public override void Initialize()
    {
        for (int i = 0; i < destructionAgents.Length; i++)
        {
            destructionAgents[i].fractureImpulseRange = fractureImpulseRange;
            destructionAgents[i].disappearanceDelayRange = disappearanceDelayRange;

            destructionAgents[i].Initialize();
        }
    }

    public override void Destruct(Vector3 externalImpulse = default)
    {
        if (groupCollider)
        {
            groupCollider.enabled = false;
        }

        if (groupRigidbody)
        {
            groupRigidbody.isKinematic = true;
        }

        for (int i = 0; i < originalElements.Length; i++)
        {
            originalElements[i].SetActive(false);
        }

        for (int i = 0; i < destructionAgents.Length; i++)
        {
            destructionAgents[i].Destruct(externalImpulse);
        }

        isDestroyed = true;
    }
}
