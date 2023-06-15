using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagnetCollectible : Collectible
{
    protected override IEnumerator CollectingCoroutine()
    {
        yield return base.CollectingCoroutine();

        // Apply magnet 
    }
}
