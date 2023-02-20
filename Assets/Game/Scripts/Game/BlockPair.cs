using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockPair
{
    public GameObject ceilBlock;
    public GameObject floorBlock;

    public GameObject container;

    private float height;
    private float halfHeight;

    private int orderIndex;

    public Vector3 position => container.transform.position;

    public Vector3 ceilBlockPosition => ceilBlock.transform.position;
    public Vector3 floorBlockPosition => floorBlock.transform.position;

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
    }
}