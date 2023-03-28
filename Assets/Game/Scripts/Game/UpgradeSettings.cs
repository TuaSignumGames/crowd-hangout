using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UpgradeSettings
{
    public List<UpgradeInfo> upgradeTable;
    public UpgradeInfo upgradeMultipliers;

    public UpgradeInfo GetUpgradeInfo(int upgradeIndex)
    {
        if (upgradeIndex < upgradeTable.Count)
        {
            return upgradeTable[upgradeIndex];
        }
        else
        {
            return new UpgradeInfo(upgradeTable.GetLast().value * Mathf.Pow(upgradeMultipliers.value, upgradeIndex - upgradeTable.Count + 1), upgradeTable.GetLast().price * Mathf.Pow(upgradeMultipliers.price, upgradeIndex - upgradeTable.Count + 1));
        }
    }
}
