using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockPair
{
    public GameObject ceilBlock;
    public GameObject floorBlock;

    public GameObject container;

    public List<Transform> ceilBlockContent;
    public List<Transform> floorBlockContent;

    public Transform landscapeContainer;

    private Collectible collectible;

    private List<Transform> landscapeBlocks;

    private float height;
    private float halfHeight;

    private float thresholdValue;

    private int orderIndex;

    private bool isVisible;

    public Collectible Collectible => collectible;

    public Vector3 Position => container.transform.position;

    public Vector3 CeilBlockPosition => ceilBlock.transform.position;
    public Vector3 FloorBlockPosition => floorBlock.transform.position;

    public float Height => height;

    public int OrderIndex => orderIndex;

    public BlockPair(GameObject ceilBlock, GameObject floorBlock, GameObject container, int orderIndex, float thresholdValue)
    {
        this.ceilBlock = ceilBlock;
        this.floorBlock = floorBlock;

        this.container = container;

        ceilBlockContent = ceilBlock.transform.GetChildren();
        floorBlockContent = floorBlock.transform.GetChildren();

        this.ceilBlock.transform.SetParent(container.transform);
        this.floorBlock.transform.SetParent(container.transform);

        this.orderIndex = orderIndex;
        this.thresholdValue = thresholdValue;

        isVisible = true;

        //DeformLandscape();
    }

    public void SetHeight(float height)
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

        collectible.SetPlacement(this, placementFactor);
    }

    public void FitCollectible()
    {
        collectible.UpdatePlacement(this);
    }

    public void SetVisible(bool isVisible)
    {
        if (this.isVisible != isVisible)
        {
            ceilBlock.SetActive(isVisible);
            floorBlock.SetActive(isVisible);

            /*
            for (int i = 0; i < ceilBlockContent.Count; i++)
            {
                ceilBlockContent[i].gameObject.SetActive(isVisible);
            }

            for (int i = 0; i < floorBlockContent.Count; i++)
            {
                floorBlockContent[i].gameObject.SetActive(isVisible);
            }
            */

            if (collectible && !collectible.IsCollected)
            {
                collectible.SetVisible(isVisible);
            }

            this.isVisible = isVisible;
        }
    }

    private void DeformLandscape()
    {
        landscapeContainer = floorBlock.transform.GetChildren().GetLast();
        landscapeBlocks = landscapeContainer.GetChildren();

        for (int i = 0; i < landscapeBlocks.Count; i++)
        {
            landscapeBlocks[i].localPosition += new Vector3(0, thresholdValue * Random.Range(-1, 1), 0);
        }
    }
}