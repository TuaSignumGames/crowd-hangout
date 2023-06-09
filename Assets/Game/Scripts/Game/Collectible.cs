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

    public int RangeNumber => collectibleSettings.fittingRangeNumber;

    public bool IsCollected => isCollected;

    public virtual void Initialize() { }

    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            collectibleSettings.collider.enabled = !collectibleSettings.collider.enabled;
        }
    }

    public void Collect()
    {
        collectibleSettings.collider.enabled = false;

        isCollected = true;

        StartCoroutine(CollectingCoroutine());

        AppManager.Instance.PlayHaptic(MoreMountains.NiceVibrations.HapticTypes.LightImpact);
    }

    public virtual void SetPlacement(BlockPair blockPair, float placementFactor)
    {
        switch (collectibleSettings.placement)
        {
            case CollectiblePlacementType.Any: this.placementFactor = placementFactor; break;
            case CollectiblePlacementType.Ground: this.placementFactor = 0; break;
            case CollectiblePlacementType.Ceiling: this.placementFactor = 1f; break;
        }

        transform.position = new Vector3(blockPair.groundBlock.transform.position.x, Mathf.Lerp(blockPair.groundBlock.transform.position.y, blockPair.ceilingBlock.transform.position.y, this.placementFactor));
    }

    public virtual void UpdatePlacement(BlockPair blockPair)
    {
        SetPlacement(blockPair, placementFactor);
    }

    public virtual void SetVisible(bool isVisible)
    {
        gameObject.SetActive(isVisible);
    }

    protected virtual IEnumerator CollectingCoroutine()
    {
        return null;
    }

    protected virtual void OnTriggerEnter(Collider other)
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
        public FloatRange placementRange;
        [Space]
        public int fittingRangeNumber;
        public float verticalShift;
        [Space]
        public Collider collider;
    }
}
