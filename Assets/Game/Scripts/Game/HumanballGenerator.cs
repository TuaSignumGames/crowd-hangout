using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HumanballGenerator
{
    public Pointer cellPointer;
    public Transform cellsContainer;
    [Space]
    public GameObject cellPrefab;
    public Vector2 cellSize;
    public LayerData[] layers;

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

    public List<HumanballLayer> GenerateLayers()
    {
        humanballLayers = new List<HumanballLayer>();

        for (int i = 0; i < layers.Length; i++)
        {
            CreateLayerContainer(i);

            humanballLayers.Add(GenerateLayer(layers[i]));

            Debug.Log($" - Layer[{i}]: {humanballLayers[i].AvailableCellsCount}");
        }

        return humanballLayers;
    }

    private HumanballLayer GenerateLayer(LayerData layerData)
    {
        humanballCells = new List<HumanballCell>();

        cellAngularSize = new Vector2(cellSize.x * 360f / (6.2832f * layerData.radius), cellSize.y * 180f / (6.2832f * layerData.radius));

        stagesCount = Mathf.RoundToInt(180f / cellAngularSize.y) + 1;

        verticalAngularStep = 180f / (stagesCount - 1);

        for (int i = 0; i < stagesCount; i++)
        {
            pointerEulerAngles.x = (-90f + verticalAngularStep * i).ToSignedAngle();

            stageSize = Mathf.FloorToInt(6.2832f * layerData.radius * Mathf.Abs(Mathf.Sin((90f + pointerEulerAngles.x) * Mathf.Deg2Rad)) / cellSize.x);

            if (stageSize > 0)
            {
                CreateStageContainer(i);

                horizontalAngularStep = 360f / stageSize;

                for (int j = 0; j < stageSize; j++)
                {
                    pointerEulerAngles.y = (90f + horizontalAngularStep * j).ToSignedAngle();

                    cellPointer.SetPlacement(pointerEulerAngles.y, pointerEulerAngles.x, layerData.radius);

                    humanballCells.Add(InstantiateHumanballCell());
                }
            }
        }

        return new HumanballLayer(newLayerContainer, humanballCells);
    }

    private HumanballCell InstantiateHumanballCell()
    {
        newHumanballCell = new HumanballCell(GameObject.Instantiate(cellPrefab, newStageContainer.transform));

        newHumanballCell.transform.position = cellPointer.Placement.position;
        newHumanballCell.transform.forward = cellPointer.Placement.direction;

        return newHumanballCell;
    }

    private void CreateStageContainer(int stageIndex)
    {
        newStageContainer = new GameObject($"Stage[{stageIndex}]");

        newStageContainer.transform.SetParent(newLayerContainer.transform);
    }

    private void CreateLayerContainer(int layerIndex)
    {
        newLayerContainer = new GameObject($"Layer[{layerIndex}]");

        newLayerContainer.transform.SetParent(cellsContainer);
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

    [System.Serializable]
    public struct LayerData
    {
        public float radius;
    }
}
