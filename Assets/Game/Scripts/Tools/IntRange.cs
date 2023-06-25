using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct IntRange
{
    public int min;
    public int max;

    public int length;

    public int Value => Random.Range(min, max);

    public IntRange(int min, int max)
    {
        this.min = min;
        this.max = max;

        length = max - min;
    }
}
