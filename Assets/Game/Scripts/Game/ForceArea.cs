using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceArea
{
    private ForceAreaData data;

    private Collider collider;

    private Vector3 force;
    private Vector3 direction;

    public ForceAreaData Data => data;

    public Collider Collider => collider;

    public Vector3 Force => force;
    public Vector3 Direction => direction;

    public ForceArea(ForceAreaData data, Collider collider)
    {
        this.data = data;
        this.collider = collider;

        direction = collider.transform.forward;
        force = direction * data.forceMagnitude;
    }
}
