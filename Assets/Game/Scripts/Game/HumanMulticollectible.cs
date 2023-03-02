using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanMulticollectible : Multicollectible
{
    [Space]
    public Transform humanContainer;
    public HumanController humanPrefab;

    protected List<HumanController> humans;

    protected override void GenerateElements(int count)
    {
        base.GenerateElements(count);

        humans = new List<HumanController>();

        // TODO Generate humans 
    }

    public override void Collect()
    {
        base.Collect();

        // TODO Drop and collect humans 
    }
}