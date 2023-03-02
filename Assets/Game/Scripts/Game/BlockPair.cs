using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockPair
{
    public GameObject ceilBlock;
    public GameObject floorBlock;

    public GameObject container;

    private Collectible collectible;

    private float height;
    private float halfHeight;

    private int orderIndex;

    public Collectible Collectible => collectible;

    public Vector3 Position => container.transform.position;

    public Vector3 CeilBlockPosition => ceilBlock.transform.position;
    public Vector3 FloorBlockPosition => floorBlock.transform.position;

    public float Height => height;

    public int OrderIndex => orderIndex;

    public BlockPair(GameObject ceilBlock, GameObject floorBlock, GameObject container, int orderIndex)
    {
        this.ceilBlock = ceilBlock;
        this.floorBlock = floorBlock;

        this.container = container;

        this.ceilBlock.transform.SetParent(container.transform);
        this.floorBlock.transform.SetParent(container.transform);

        this.orderIndex = orderIndex;
    }

    public void SetHeight(float height, float thresholdValue)
    {
        this.height = height;

        halfHeight = Mathf.Round(height / 2f / thresholdValue) * thresholdValue;

        ceilBlock.transform.localPosition = new Vector3(0, halfHeight, 0);
        floorBlock.transform.localPosition = new Vector3(0, -halfHeight, 0);

        if (collectible)
        {
            FitCollectible();
        }
    }

    public void AddCollectible(Collectible collectible, float placementFactor = 0.5f)
    {
        this.collectible = collectible;

        collectible.Initialize();
        collectible.SetPlacement(this, placementFactor);
    }

    public void FitCollectible()
    {
        collectible.UpdatePlacement(this);
    }
}