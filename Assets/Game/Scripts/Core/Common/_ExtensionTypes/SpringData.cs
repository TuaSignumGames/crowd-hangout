using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SpringData
{
    public float springFactor;
    public float dampingFactor;

    public SpringData(float springFactor, float dampingFactor)
    {
        this.springFactor = springFactor;
        this.dampingFactor = dampingFactor;
    }
}
