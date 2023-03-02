using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuspendedHumanMulticollectible : SuspendedMulticollectible
{
    [Space]
    public Transform humanContainer;
    public HumanController humanPrefab;
    [Space]
    public List<Transform> humanballBaseCells;
    public HumanballGenerator humanballGenerator;

    protected List<HumanController> humans;

    protected Humanball humanball;

    protected override void GenerateElements(int count)
    {
        base.GenerateElements(count);

        humans = new List<HumanController>();

        GenerateBallCells(count);
    }

    public override void Collect()
    {
        base.Collect();

        // TODO Drop and collect humans 
    }

    protected void GenerateBallCells(int count)
    {
        List<HumanballCell> baseLayerCells = new List<HumanballCell>();

        for (int i = 0; i < humanballBaseCells.Count; i++)
        {
            baseLayerCells.Add(new HumanballCell(humanballBaseCells[i].gameObject));

            if (i == 0)
            {
                baseLayerCells[i].TrySavePose();
            }
            else
            {
                baseLayerCells[i].TryCropPose();
            }
        }

        List<HumanballLayer> structureLayers = new List<HumanballLayer>
        {
            humanballGenerator.GenerateLayer(baseLayerCells, 0.2f, "B")
        };

        structureLayers.AddRange(humanballGenerator.GenerateProceduralCells(count - baseLayerCells.Count));

        baseLayerCells[0].Human.isFree = false;

        // TODO Fill cells by humans 
    }
}
