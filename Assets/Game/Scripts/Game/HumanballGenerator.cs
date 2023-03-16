using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HumanballGenerator
{
    public Pointer cellPointer;
    public Transform cellsContainer;
    [Space]
    public Vector2 cellSize;
    public float baseLayerRadius;
    public float layerWidth;

    private List<HumanballCell> humanballCells;
    private List<HumanballLayer> humanballLayers;

    private HumanballCell newHumanballCell;
    private HumanballLayer newHumanballLayer;

    private GameObject newStageContainer;
    private GameObject newLayerContainer;

    private Vector2 cellAngularSize;
    private Vector2 pointerEulerAngles;

    private float verticalAngularStep;
    private float horizontalAngularStep;

    private int stagesCount;
    private int stageSize;

    public List<HumanballLayer> GenerateProceduralLayers(int count)
    {
        humanballLayers = new List<HumanballLayer>();

        for (int i = 0; i < count; i++)
        {
            CreateLayerContainer(i.ToString());

            humanballLayers.Add(GenerateProceduralLayer(baseLayerRadius + layerWidth * i));

            //Debug.Log($" - Layer[{i}]: {humanballLayers[i].AvailableCellsCount}");
        }

        return humanballLayers;
    }

    public List<HumanballLayer> GenerateProceduralCells(int count)
    {
        humanballLayers = new List<HumanballLayer>();

        while (humanballCells.Count < count)
        {
            CreateLayerContainer(humanballLayers.Count.ToString());

            humanballLayers.Add(GenerateProceduralLayer(baseLayerRadius + layerWidth * humanballLayers.Count));
        }

        return humanballLayers;
    }

    public HumanballLayer GenerateLayer(IList<HumanballCell> layerCells, float radius, string layerTag)
    {
        humanballCells = new List<HumanballCell>(layerCells);

        CreateLayerContainer(layerTag);

        for (int i = 0; i < humanballCells.Count; i++)
        {
            humanballCells[i].transform.SetParent(newLayerContainer.transform);
        }

        return new HumanballLayer(newLayerContainer, humanballCells, radius, true);
    }

    private HumanballLayer GenerateProceduralLayer(float radius)
    {
        humanballCells = new List<HumanballCell>();

        cellAngularSize = new Vector2(cellSize.x * 360f / (6.2832f * radius), cellSize.y * 180f / (6.2832f * radius));

        stagesCount = Mathf.RoundToInt(180f / cellAngularSize.y) + 1;

        verticalAngularStep = 180f / (stagesCount - 1);

        cellPointer.pivotTransform.localPosition = new Vector3(0, -radius * 1.2f, 0);

        for (int i = 0; i < stagesCount; i++)
        {
            pointerEulerAngles.x = (-90f + verticalAngularStep * i).ToSignedAngle();

            stageSize = Mathf.FloorToInt(6.2832f * radius * Mathf.Abs(Mathf.Sin((90f + pointerEulerAngles.x) * Mathf.Deg2Rad)) / cellSize.x);

            if (stageSize > 0)
            {
                CreateStageContainer(i);

                horizontalAngularStep = 360f / stageSize;

                for (int j = 0; j < stageSize; j++)
                {
                    pointerEulerAngles.y = (90f + horizontalAngularStep * j).ToSignedAngle();

                    cellPointer.SetPlacement(pointerEulerAngles.y, pointerEulerAngles.x, radius);

                    humanballCells.Add(InstantiateHumanballCell());
                }
            }
        }

        return new HumanballLayer(newLayerContainer, humanballCells, radius, false);
    }

    private HumanballCell InstantiateHumanballCell()
    {
        newHumanballCell = new HumanballCell(new GameObject("HumanCell"));

        newHumanballCell.transform.SetParent(newStageContainer.transform);

        newHumanballCell.transform.position = cellPointer.Placement.position;
        newHumanballCell.transform.forward = cellPointer.Placement.direction;

        return newHumanballCell;
    }

    private void CreateStageContainer(int stageIndex)
    {
        newStageContainer = new GameObject($"Stage[{stageIndex}]");

        newStageContainer.transform.SetParent(newLayerContainer.transform);
    }

    private void CreateLayerContainer(string layerTag)
    {
        newLayerContainer = new GameObject($"Layer[{layerTag}]");

        newLayerContainer.transform.SetParent(cellsContainer);

        //Debug.Log($" - Humanball[x:{Mathf.RoundToInt(cellsContainer.position.x)}] layer created: {layerTag}");
    }

    [System.Serializable]
    public class Pointer
    {
        public Transform pivotTransform;
        public Transform pointTransform;

        private PointerData placement;

        public PointerData Placement => placement;

        public void SetPlacement(float yaw, float pitch, float distance)
        {
            pivotTransform.localEulerAngles = new Vector3(pitch, yaw, 0);
            pointTransform.localPosition = new Vector3(0, 0, distance);

            placement = new PointerData(pointTransform.position, pointTransform.forward);
        }
    }

    public struct PointerData
    {
        public Vector3 position;
        public Vector3 direction;

        public PointerData(Vector3 position, Vector3 direction)
        {
            this.position = position;
            this.direction = direction;
        }
    }
}
