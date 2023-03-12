using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct FloatRange
{
    public float minValue;
    public float maxValue;

    public float Value => Random.Range(minValue, maxValue);

    public FloatRange(float minValue, float maxValue)
    {
        this.minValue = minValue;
        this.maxValue = maxValue;
    }
}
