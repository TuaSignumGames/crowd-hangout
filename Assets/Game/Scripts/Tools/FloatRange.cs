using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct FloatRange
{
    public float min;
    public float max;

    [HideInInspector]
    public float length;

    public float Value => Random.Range(min, max);

    public FloatRange(float min, float max)
    {
        this.min = min;
        this.max = max;

        length = max - min;
    }
}
