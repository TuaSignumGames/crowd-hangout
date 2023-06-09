using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CollectibleType { Human, Weapon }

[System.Serializable]
public class CollectibleSettings
{
    public List<CollectibleData> humanCollectibles;
    public List<WeaponMulticollectible> weaponCollectibles;
    public AnimationCurve populationCurve;

    private List<HumanMulticollectible> availableHumanCollectiblePrefabs;

    public List<HumanMulticollectible> GetAvailableHumanCollectiblePrefabs(float populationValue)
    {
        availableHumanCollectiblePrefabs = new List<HumanMulticollectible>();

        for (int i = 0; i < humanCollectibles.Count; i++)
        {
            if (populationValue >= humanCollectibles[i].populationValue)
            {
                availableHumanCollectiblePrefabs.Add(humanCollectibles[i].prefab);
            }
        }

        return availableHumanCollectiblePrefabs;
    }
}

[System.Serializable]
public struct CollectibleData
{
    public float populationValue;
    public HumanMulticollectible prefab;
}
