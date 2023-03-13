using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CollectiblePlacementType { Any, Ground, Ceiling }

public class Collectible : MonoBehaviour
{
    public CollectibleSettings collectibleSettings;

    protected Vector3 contactPoint;

    protected float placementFactor;

    protected bool isCollected;

    public CollectiblePlacementType Placement => collectibleSettings.placement;

    public int RangeNumber => collectibleSettings.rangeNumber;

    public bool IsCollected => isCollected;

    public virtual void Initialize() { }

    public void Collect()
    {
        collectibleSettings.collider.enabled = false;

        isCollected = true;

        StartCoroutine(CollectingCoroutine());
    }

    public virtual void SetPlacement(BlockPair blockPair, float placementFactor)
    {
        switch (collectibleSettings.placement)
        {
            case CollectiblePlacementType.Any: this.placementFactor = placementFactor; break;
            case CollectiblePlacementType.Ground: this.placementFactor = 0; break;
            case CollectiblePlacementType.Ceiling: this.placementFactor = 1f; break;
        }

        transform.position = new Vector3(blockPair.floorBlock.transform.position.x, Mathf.Lerp(blockPair.floorBlock.transform.position.y, blockPair.ceilBlock.transform.position.y, this.placementFactor));
    }

    public virtual void UpdatePlacement(BlockPair blockPair)
    {
        SetPlacement(blockPair, placementFactor);
    }

    protected virtual IEnumerator CollectingCoroutine()
    {
        return null;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 8)
        {
            contactPoint = collectibleSettings.collider.ClosestPoint(other.transform.position);

            Collect();
        }
    }

    [System.Serializable]
    public class CollectibleSettings
    {
        public CollectiblePlacementType placement;
        public int rangeNumber;
        [Space]
        public Collider collider;
    }
}
