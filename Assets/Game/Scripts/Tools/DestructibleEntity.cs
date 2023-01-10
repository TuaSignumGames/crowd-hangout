using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DestructibleEntity : MonoBehaviour
{
    public float destructionImpulse;

    protected bool isDestroyed;

    public abstract void Initialize();
    public abstract void Destruct(Vector3 externalImpulse = default);
}
