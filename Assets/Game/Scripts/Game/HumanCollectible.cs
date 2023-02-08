using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanCollectible : SuspendedCollectible
{
    [Space]
    public HumanController collectableHuman;

    public override void Collect()
    {
        PlayerController.Instance.Ball.StickHuman(collectableHuman);

        gameObject.SetActive(false);
    }
}
