using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class UpgradeInfo
{
    [HideInInspector]
    public string title;

    public float value;
    public float price;

    public override string ToString()
    {
        return $"[{value}] : [${price}]";
    }
}
