using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanCollectible : SuspendedCollectible
{
    public HumanController collectableHuman;

    public override void Collect()
    {
        base.Collect();

        PlayerController.Instance.Ball.StickHuman(collectableHuman);
    }
}
