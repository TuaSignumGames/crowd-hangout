using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CollectibleType { None, Human, Weapon }

[System.Serializable]
public class CollectibleSettings
{
    public List<CollectibleData> humanCollectibles;
    public List<WeaponMulticollectible> weaponCollectibles;
    public AnimationCurve populationCurve;
}

[System.Serializable]
public struct CollectibleData
{
    public float populationValue;
    public Multicollectible prefab;
}
