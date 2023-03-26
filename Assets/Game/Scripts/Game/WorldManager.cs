using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public static HumanController humanPrefab;

    public HumanController _humanPrefab;
    [Space]
    public List<UpgradeInfo> weaponUpgradeTable;
    public List<UpgradeInfo> populationUpgradeTable;

    private void Awake()
    {
        humanPrefab = _humanPrefab;
    }

    public static float GetWeaponPower(int weaponID)
    {
        return humanPrefab.weaponSettings[weaponID].Power;
    }

    public static int GetWeaponID(float power)
    {
        for (int i = 0; i < humanPrefab.weaponSettings.Count; i++)
        {
            if (humanPrefab.weaponSettings[i].Power > power)
            {
                return i - 1;
            }
        }

        return humanPrefab.weaponSettings.Count - 1;
    }

    private void OnValidate()
    {
        if (weaponUpgradeTable.Count > 0)
        {
            for (int i = 0; i < weaponUpgradeTable.Count; i++)
            {
                weaponUpgradeTable[i].title = weaponUpgradeTable[i].ToString();
            }
        }

        if (populationUpgradeTable.Count > 0)
        {
            for (int i = 0; i < populationUpgradeTable.Count; i++)
            {
                populationUpgradeTable[i].title = populationUpgradeTable[i].ToString();
            }
        }
    }
}
