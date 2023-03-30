using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BattlePathProgressionSettings
{
    public List<BattlePathStageInfo> stages;
    [Space]
    public bool infiniteProgression;
    public Vector2 guardIncrementationFactors;
    public Vector2 rewardIncrementationFactors;
    [Space]
    public int guardiansCountLimit;
    public float rewardRoundingOrder = 1f;

    private int outrangeStageIndex;

    private int stageGuardiansCount;

    private float stageReward;

    private float guardiansIncrement;
    private float rewardIncrement;

    public BattlePathStageInfo GetStageInfo(int stageIndex)
    {
        if (stageIndex < stages.Count)
        {
            return stages[stageIndex];
        }
        else if (infiniteProgression)
        {
            outrangeStageIndex = stageIndex - stages.Count;

            guardiansIncrement = stages.GetLast().guardiansCount * guardIncrementationFactors.x - stages.GetLast().guardiansCount;
            rewardIncrement = stages.GetLast().reward * rewardIncrementationFactors.x - stages.GetLast().reward;

            stageGuardiansCount = Mathf.RoundToInt(stages.GetLast().guardiansCount + guardiansIncrement * (outrangeStageIndex + 1) * (1f + guardIncrementationFactors.y * outrangeStageIndex));
            stageReward = stages.GetLast().reward + rewardIncrement * (outrangeStageIndex + 1) * (1f + rewardIncrementationFactors.y * outrangeStageIndex);

            return new BattlePathStageInfo(Mathf.Clamp(stageGuardiansCount, 1, guardiansCountLimit), Mathf.Round(stageReward / rewardRoundingOrder) * rewardRoundingOrder);
        }

        return null;
    }
}
