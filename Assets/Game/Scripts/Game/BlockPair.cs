using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BlockPair
{
    public GameObject ceilingBlock;
    public GameObject groundBlock;

    public GameObject container;

    public List<Transform> ceilBlockContent;
    public List<Transform> floorBlockContent;

    public Transform landscapeContainer;

    private Collectible collectible;

    private List<Transform> landscapeBlocks;

    private float height;
    private float halfHeight;

    private float thresholdValue;

    private bool isVisible;

    public Collectible Collectible => collectible;

    public Vector3 Position => container.transform.position;

    public Vector3 CeilBlockPosition => ceilingBlock.transform.position;
    public Vector3 FloorBlockPosition => groundBlock.transform.position;

    public int orderIndex;

    public float Height => ceilingBlock.transform.position.y - groundBlock.transform.position.y;

    public BlockPair(GameObject ceilBlock, GameObject floorBlock, GameObject container, int orderIndex, float thresholdValue)
    {
        this.ceilingBlock = ceilBlock;
        this.groundBlock = floorBlock;

        this.container = container;

        ceilBlockContent = ceilBlock.transform.GetChildren();
        floorBlockContent = floorBlock.transform.GetChildren();

        this.ceilingBlock.transform.SetParent(container.transform);
        this.groundBlock.transform.SetParent(container.transform);

        this.orderIndex = orderIndex;
        this.thresholdValue = thresholdValue;

        isVisible = true;

        //DeformLandscape();
    }

    public void SetHeight(float height)
    {
        this.height = height;

        halfHeight = Mathf.Round(height / 2f / thresholdValue) * thresholdValue;

        ceilingBlock.transform.localPosition = new Vector3(0, halfHeight, 0);
        groundBlock.transform.localPosition = new Vector3(0, -halfHeight, 0);

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
            ceilingBlock.SetActive(isVisible);
            groundBlock.SetActive(isVisible);

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
        landscapeContainer = groundBlock.transform.GetChildren().GetLast();
        landscapeBlocks = landscapeContainer.GetChildren();

        for (int i = 0; i < landscapeBlocks.Count; i++)
        {
            landscapeBlocks[i].localPosition += new Vector3(0, thresholdValue * Random.Range(-1, 1), 0);
        }
    }
}