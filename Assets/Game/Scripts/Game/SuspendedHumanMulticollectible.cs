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

    protected Humanball structure;

    protected override void GenerateElements(int count)
    {
        base.GenerateElements(count);

        GenerateHumanball(count);
    }

    public override void Collect()
    {
        base.Collect();

        StartCoroutine(CollectingCoroutine());
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

        humans[0].MotionSimulator.instanceID = 1;
    }

    protected IEnumerator CollectingCoroutine()
    {
        for (int i = 0; i < humans.Count; i++)
        {
            humans[i].DropFromCell((humans[i].transform.position - PlayerController.Instance.Ball.Transform.position).normalized * PlayerController.Instance.Ball.Velocity.magnitude, Random.insideUnitSphere.normalized * Random.Range(30f, 120f));
        }

        yield return new WaitForSeconds(2f);

        for (int i = 0; i < humans.Count; i++)
        {
            PlayerController.Instance.Ball.StickHuman(humans[i]);
        }
    }
}
