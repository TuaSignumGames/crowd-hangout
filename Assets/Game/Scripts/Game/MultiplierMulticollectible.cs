using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static BuildingMulticollectible;

public class MultiplierMulticollectible : HumanMulticollectible
{
    public override void Initialize(int elementsCount = 1)
    {
        base.Initialize(elementsCount);

        for (int i = 0; i < humanCollectibles.Count; i++)
        {
            humanCollectibles[i].Element.Transform.gameObject.SetActive(false);
        }

        for (int i = 0; i < multicollectibleSettings.capsules.Length; i++)
        {
            multicollectibleSettings.capsules[i].Initialize();
        }
    }

    protected override IEnumerator CollectingCoroutine()
    {
        yield return base.CollectingCoroutine();

        for (int i = 0; i < humanCollectibles.Count; i++)
        {
            humanCollectibles[i].Element.Transform.gameObject.SetActive(true);
        }

        for (int i = 0; i < multicollectibleSettings.capsules.Length; i++)
        {
            multicollectibleSettings.capsules[i].Break(PlayerController.Humanball.Velocity);
        }
    }
}
