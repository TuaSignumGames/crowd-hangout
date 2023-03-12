using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuspendedHumanMulticollectible : SuspendedMulticollectible
{
    [Space]
    public HumanController humanPrefab;
    [Space]
    public List<Transform> humanballBaseCells;
    public HumanballGenerator humanballGenerator;

    protected List<HumanController> humans;

    protected HumanController humanInstance;

    protected HumanballCell targetCell;

    protected Humanball structure;

    protected override void GenerateElements(int count)
    {
        base.GenerateElements(count);

        GenerateHumanball(count);
    }

    public override void Collect()
    {
        for (int i = 0; i < humans.Count; i++)
        {
            humans[i].Drop((humans[i].transform.position - PlayerController.Humanball.Transform.position).normalized * PlayerController.Humanball.Velocity.magnitude, Random.insideUnitSphere.normalized * Random.Range(30f, 120f));
        }

        base.Collect();
    }

    protected override void PullElements()
    {
        base.PullElements();

        for (int i = 0; i < elements.Length; i++)
        {
            if (!elements[i].IsCollected)
            {
                if (elements[i].Pull(PlayerController.Humanball.Transform))
                {
                    PlayerController.Humanball.StickHuman(humans[i], false);
                }
            }
        }
    }

    protected void GenerateHumanball(int count)
    {
        humans = new List<HumanController>();

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
            structureLayers.AddRange(humanballGenerator.GenerateProceduralCells(count - baseLayerCells.Count));
        }

        structure = new Humanball(structureLayers);

        for (int i = 0; i < count; i++)
        {
            humanInstance = Instantiate(humanPrefab);

            humanInstance.Initialize();

            structure.AddHuman(humanInstance, i < baseLayerCells.Count);

            //humanInstance.enabled = false;
            humanInstance.components.collider.enabled = false;

            humans.Add(humanInstance);
        }

        for (int i = 0; i < humans.Count; i++)
        {
            elements[i] = new MulticollectibleElement(humans[i].MotionSimulator, multicollectibleSettings.CollectibleSpeed, multicollectibleSettings.CollectibleAcceleration, multicollectibleSettings.CollectiblePullingDelay, 1f);
        }

        //humans[count / 2].MotionSimulator.instanceID = 1;
    }
}
