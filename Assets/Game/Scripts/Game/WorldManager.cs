using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public static HumanController humanPrefab;

    public HumanController _humanPrefab;

    private void Awake()
    {
        humanPrefab = _humanPrefab;
    }

    /*
    public static float GetWeaponPower(int weaponID)
    {
        return humanPrefab.weaponSettings[weaponID].Power;
    }
    */

    public static int GetWeaponID(float damageRate)
    {
        for (int i = 0; i < humanPrefab.weaponSettings.Count; i++)
        {
            if (humanPrefab.weaponSettings[i].damageRate > damageRate)
            {
                return i - 1;
            }
        }

        return humanPrefab.weaponSettings.Count - 1;
    }
}
