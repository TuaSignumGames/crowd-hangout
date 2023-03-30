using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UpgradeSettings
{
    public List<UpgradeInfo> upgradeTable;
    [Space]
    public bool infiniteProgression;
    public Vector2 valueIncrementationFactors;
    public Vector2 priceIncrementationFactors;
    [Space]
    public float valueRoundingOrder = 1f;
    public float priceRoundingOrder = 1f;

    private int outrangeUpgradeIndex;

    private float upgradeValue;
    private float upgradePrice;

    private float upgradeValueIncrement;
    private float upgradePriceIncrement;

    public UpgradeInfo GetUpgradeInfo(int upgradeIndex)
    {
        if (upgradeIndex < upgradeTable.Count)
        {
            return upgradeTable[upgradeIndex];
        }
        else if (infiniteProgression)
        {
            outrangeUpgradeIndex = upgradeIndex - upgradeTable.Count;

            upgradeValueIncrement = upgradeTable.GetLast().value * valueIncrementationFactors.x - upgradeTable.GetLast().value;
            upgradePriceIncrement = upgradeTable.GetLast().price * priceIncrementationFactors.x - upgradeTable.GetLast().price;

            upgradeValue = upgradeTable.GetLast().value + upgradeValueIncrement * (outrangeUpgradeIndex + 1) * (1f + valueIncrementationFactors.y * outrangeUpgradeIndex);
            upgradePrice = upgradeTable.GetLast().price + upgradePriceIncrement * (outrangeUpgradeIndex + 1) * (1f + priceIncrementationFactors.y * outrangeUpgradeIndex);

            return new UpgradeInfo(Mathf.Round(upgradeValue / valueRoundingOrder) * valueRoundingOrder, Mathf.Round(upgradePrice / priceRoundingOrder) * priceRoundingOrder);
        }

        return null;
    }

    public void SimulateUpgradeCycle(int iterations)
    {
        UpgradeInfo upgradeInfo = null;

        for (int i = 0; i < iterations; i++)
        {
            upgradeInfo = GetUpgradeInfo(i);

            if (upgradeInfo == null)
            {
                return;
            }

            Debug.Log($" Upgrade[{i}] - value: {upgradeInfo.value} / price: {upgradeInfo.price}");
        }
    }
}
