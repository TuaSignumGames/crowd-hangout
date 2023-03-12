using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct IntRange
{
    public int minValue;
    public int maxValue;

    public int Value => Random.Range(minValue, maxValue);

    public IntRange(int minValue, int maxValue)
    {
        this.minValue = minValue;
        this.maxValue = maxValue;
    }
}
