using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanMulticollectible : Multicollectible
{
    [Space]
    public Transform humanContainer;
    public HumanController humanPrefab;

    private HumanController humanInstance;

    protected List<MulticollectibleEntity<HumanController>> humanCollectibles;

    protected Pool<MulticollectibleEntity<HumanController>> humanCollectiblesPool;

    protected Vector3 humanMidpoint;

    public override void Initialize(int elementsCount = 1)
    {
        base.Initialize(elementsCount);

        humanCollectibles = new List<MulticollectibleEntity<HumanController>>();

        for (int i = 0; i < elementsCount; i++)
        {
            humanInstance = Instantiate(humanPrefab, humanContainer);

            humanInstance.Initialize();

            humanInstance.components.collider.enabled = false;

            elements[i] = new MulticollectibleElement(humanInstance.MotionSimulator, multicollectibleSettings.collectiblePullingSpeed, multicollectibleSettings.collectiblePullingDelay);

            humanCollectibles.Add(new MulticollectibleEntity<HumanController>(humanInstance, elements[i]));
        }



        humanCollectiblesPool = new Pool<MulticollectibleEntity<HumanController>>(humanCollectibles);
    }

    protected override void ProcessCollecting()
    {
        for (int i = 0; i < humanCollectibles.Count; i++)
        {
            if (humanCollectibles[i].IsCollecting)
            {
                if (humanCollectibles[i].Element.Pull(PlayerController.Humanball.Transform))
                {
                    PlayerController.Humanball.StickHuman(humanCollectibles[i].Entity);
                }
            }
        }
    }

    protected override IEnumerator CollectingCoroutine()
    {
        for (int i = 0; i < humanCollectibles.Count; i++)
        {
            humanMidpoint += humanCollectibles[i].Entity.transform.position;
        }

        humanMidpoint /= humanCollectibles.Count;

        for (int i = 0; i < humanCollectibles.Count; i++)
        {
            humanCollectibles[i].Collect();

            humanCollectibles[i].Entity.Drop((humanCollectibles[i].Entity.transform.position - humanMidpoint).normalized.Multiplied(multicollectibleSettings.collectibleScatteringSettings.impulseRatio) + PlayerController.Humanball.Velocity * multicollectibleSettings.collectibleScatteringSettings.externalImpulseFactor, Random.insideUnitSphere.normalized * multicollectibleSettings.collectibleScatteringSettings.angularMomentumRange.Value);
        }

        yield return base.CollectingCoroutine();
    }
}