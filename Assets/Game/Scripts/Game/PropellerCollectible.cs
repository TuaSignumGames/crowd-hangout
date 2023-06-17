using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropellerCollectible : Collectible
{
    protected override IEnumerator CollectingCoroutine()
    {
        yield return base.CollectingCoroutine();

        PlayerController.Instance.SetPropeller(true);
    }
}
