using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SuspendedCollectible;

public class SuspendedMulticollectible : Multicollectible
{
    public SuspensionSettings suspensionSettings;

    protected ConnectedRope rope;

    protected float swingPeriod;
    protected float swingPeriodOffset;

    public override void Initialize(int elementsCount = 1)
    {
        base.Initialize(elementsCount);

        rope = suspensionSettings.rope;

        swingPeriod = rope.swingPeriod;
        swingPeriodOffset = Random.Range(0, swingPeriod);
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();

        if (!suspensionSettings.rope.IsConnected)
        {
            rope.UpdateBouncing();
        }
    }

    private void LateUpdate()
    {
        if (rope.IsConnected)
        {
            rope.swingContainer.localEulerAngles = new Vector3(0, 0, Mathf.Sin(6.28f * (swingPeriodOffset + Time.timeSinceLevelLoad) / swingPeriod) * rope.swingAmplitude);
        }
    }

    public override void SetPlacement(BlockPair blockPair, float placementFactor)
    {
        transform.SetParent(null);

        base.SetPlacement(blockPair, placementFactor);

        rope.Connect(transform, blockPair.ceilingBlock.transform.position);
    }

    public override void UpdatePlacement(BlockPair blockPair)
    {
        SetPlacement(blockPair, placementFactor);
    }

    protected override IEnumerator CollectingCoroutine()
    {
        rope.Release();

        return base.CollectingCoroutine();
    }
}
