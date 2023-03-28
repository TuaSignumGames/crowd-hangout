using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO Design progression (Base table -> multiplying) 

public class WorldManager : MonoBehaviour
{
    public static HumanController humanPrefab;

    public static List<Weapon> weaponAssortment;

    public static UpgradeSettings weaponUpgradeSettings;
    public static UpgradeSettings populationUpgradeSettings;

    public HumanController _humanPrefab;
    [Space]
    public UpgradeSettings _weaponUpgradeSettings;
    public UpgradeSettings _populationUpgradeSettings;

    private void Awake()
    {
        humanPrefab = _humanPrefab;

        weaponAssortment = new List<Weapon>(humanPrefab.weaponSettings);

        weaponUpgradeSettings = _weaponUpgradeSettings;
        populationUpgradeSettings = _populationUpgradeSettings;
    }

    public static float GetWeaponPower(int weaponID)
    {
        return humanPrefab.weaponSettings[weaponID].Power;
    }

    public static int GetWeaponID(float power)
    {
        //print($" Get Weapon ID -- Power: {power}");

        for (int i = 0; i < humanPrefab.weaponSettings.Count; i++)
        {
            if (humanPrefab.weaponSettings[i].Power > power)
            {
                return i - 1;
            }
        }

        return humanPrefab.weaponSettings.Count - 1;
    }
}
