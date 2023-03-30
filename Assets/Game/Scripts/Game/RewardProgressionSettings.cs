using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RewardProgressionSettings
{
    public List<float> stageRewards;
    public float rewardMultipier;

    public float GetReward(int upgradeIndex)
    {
        if (upgradeIndex < stageRewards.Count)
        {
            return stageRewards[upgradeIndex];
        }
        else
        {
            return stageRewards.GetLast() * Mathf.Pow(rewardMultipier, upgradeIndex - stageRewards.Count + 1);
        }
    }
}
