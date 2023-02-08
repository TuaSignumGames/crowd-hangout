using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Collectible : MonoBehaviour
{
    public virtual void Initialize(BlockPair blockPair, float placementFactor = 0.5f)
    {
        transform.position = new Vector3(blockPair.floorBlock.transform.position.x, Mathf.Lerp(blockPair.floorBlock.transform.position.y, blockPair.ceilBlock.transform.position.y, placementFactor));
    }

    public abstract void Collect();

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 8)
        {
            Collect();
        }
    }
}
