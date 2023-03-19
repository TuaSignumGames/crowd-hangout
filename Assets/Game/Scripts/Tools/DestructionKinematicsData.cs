using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct ScatterData
{
    public Vector3 impulseRatio;
    public FloatRange angularMomentumRange;
    public float externalImpulseFactor;
}