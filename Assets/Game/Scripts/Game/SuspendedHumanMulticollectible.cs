using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SuspendedCollectible;

public class SuspendedHumanMulticollectible : HumanMulticollectible
{
    [Space]
    public SuspensionSettings suspensionSettings;
    [Space]
    public List<Transform> humanballBaseCells;
    public HumanballGenerator humanballGenerator;

    protected HumanballCell targetCell;

    protected Humanball structure;

    protected ConnectedRope rope;

    protected float swingPeriod;
    protected float swingPeriodOffset;

    public override void Initialize(int elementsCount = 1)
    {
        base.Initialize(elementsCount);

        rope = suspensionSettings.rope;

        swingPeriod = Random.Range(rope.swingPeriodRange.x, rope.swingPeriodRange.y);
        swingPeriodOffset = Random.Range(0, swingPeriod);

        GenerateHumanball(elementsCount);
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

        rope.Connect(transform, blockPair.ceilBlock.transform.position);
    }

    public override void UpdatePlacement(BlockPair blockPair)
    {
        SetPlacement(blockPair, placementFactor);
    }

    protected void GenerateHumanball(int count)
    {
        List<HumanballCell> baseLayerCells = new List<HumanballCell>();

        for (int i = 0; i < humanballBaseCells.Count; i++)
        {
            baseLayerCells.Add(new HumanballCell(humanballBaseCells[i].gameObject));

            baseLayerCells[i].TryCropPose();
        }

        List<HumanballLayer> structureLayers = new List<HumanballLayer>
        {
            humanballGenerator.GenerateLayer(baseLayerCells, 0.2f, "B")
        };

        if (count > baseLayerCells.Count)
        {
            Debug.Log($" - Generating procedural cells: {count - baseLayerCells.Count}");

            structureLayers.AddRange(humanballGenerator.GenerateProceduralCells(count));
        }

        structure = new Humanball(structureLayers);

        for (int i = 0; i < count; i++)
        {
            structure.AddHuman(humanCollectibles[i].Entity, i < baseLayerCells.Count);
        }
    }

    protected override IEnumerator CollectingCoroutine()
    {
        rope.Release();

        yield return base.CollectingCoroutine();
    }
}
