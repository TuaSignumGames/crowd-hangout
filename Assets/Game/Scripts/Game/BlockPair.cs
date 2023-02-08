using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockPair
{
    public GameObject ceilBlock;
    public GameObject floorBlock;

    public GameObject container;

    public BlockPair(GameObject ceilBlock, GameObject floorBlock, GameObject container)
    {
        this.ceilBlock = ceilBlock;
        this.floorBlock = floorBlock;

        this.container = container;

        this.ceilBlock.transform.SetParent(container.transform);
        this.floorBlock.transform.SetParent(container.transform);
    }
}
