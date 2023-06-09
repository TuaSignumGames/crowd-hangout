using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WeaponSetInfo
{
    [HideInInspector]
    public string title;

    public WeaponType[] weapons;

    public string GetTitle()
    {
        title = string.Empty;

        for (int i = 0; i < weapons.Length; i++)
        {
            title += weapons[i] + (i < weapons.Length - 1 ? ", " : "");
        }

        return title;
    }
}
