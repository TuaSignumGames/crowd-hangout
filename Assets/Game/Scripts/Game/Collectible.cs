using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Collectible : MonoBehaviour
{
    public new Collider collider;

    protected BlockPair blockPair;

    protected float placementFactor;

    protected bool isCollected;

    public bool IsCollected => isCollected;

    public virtual void Initialize(BlockPair blockPair, float placementFactor = 0.5f)
    {
        this.blockPair = blockPair;
        this.placementFactor = placementFactor;

        UpdatePlacement();
    }

    public virtual void Collect()
    {
        collider.enabled = false;

        isCollected = true;
    }

    public virtual void UpdatePlacement()
    {
        transform.position = new Vector3(blockPair.floorBlock.transform.position.x, Mathf.Lerp(blockPair.floorBlock.transform.position.y, blockPair.ceilBlock.transform.position.y, placementFactor));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 8)
        {
            Collect();
        }
    }
}
