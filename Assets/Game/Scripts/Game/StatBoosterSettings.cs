using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StatBoosterSettings
{
    public StatBoostingData motionSpeedBoosterData;
    public StatBoostingData damageRateBoosterData;
    [Space]
    public float incrementationTimeout;
    public float decrementationSpeed;
}

[System.Serializable]
public struct StatBoostingData
{
    public int stepsCount;
    public float multiplier;
    public AnimationCurve multiplicationCurve;

    public float GetMultiplier(float step)
    {
        return 1f + multiplicationCurve.Evaluate(step / stepsCount) * (multiplier - 1f);
    }
}