using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    public new Collider collider;

    protected float placementFactor;

    protected bool isCollected;

    public bool IsCollected => isCollected;

    public virtual void Initialize() { }

    public virtual void Collect()
    {
        collider.enabled = false;

        isCollected = true;
    }

    public virtual void SetPlacement(BlockPair blockPair, float placementFactor)
    {
        this.placementFactor = placementFactor;

        transform.position = new Vector3(blockPair.floorBlock.transform.position.x, Mathf.Lerp(blockPair.floorBlock.transform.position.y, blockPair.ceilBlock.transform.position.y, placementFactor));
    }

    public virtual void UpdatePlacement(BlockPair blockPair)
    {
        SetPlacement(blockPair, placementFactor);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 8)
        {
            Collect();
        }
    }
}
