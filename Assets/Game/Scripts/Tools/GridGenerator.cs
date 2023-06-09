using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    public List<GameObject> cellPrefabs;
    [Space]
    public Vector2 metricSize;
    public Vector2Int numericSize;
    public bool controlScale;
    [Range(0, 1f)]
    public float horizontalSpacing = 1f;
    [Range(0, 1f)]
    public float verticalSpacing = 1f;
    [Space]
    public bool randomizeOffset;
    public Vector3 minOffset;
    public Vector3 maxOffset;
    [Space]
    public bool randomizeRotation;
    public Vector3 rotationStep;
    [Space]
    public bool randomizeScale;
    public Vector3 minScale = Vector3.one;
    public Vector3 maxScale = Vector3.one;
    [Space]
    public bool generateOnAwake;

    private GameObject cellInstance;

    private GameObject[] elements;

    private Vector2 cellSize;

    private Vector3Int rotationStepLimit;

    private float horizontalOffset;
    private float verticalOffset;

    public GameObject[] Elements => elements;

    private void Awake()
    {
        if (generateOnAwake)
        {
            Generate();
        }
    }

    public void Generate()
    {
        if (cellPrefabs.Count > 0)
        {
            elements = new GameObject[numericSize.x * numericSize.y];

            cellSize = new Vector2(metricSize.x / numericSize.x, metricSize.y / numericSize.y);

            horizontalOffset = (numericSize.x * cellSize.x - cellSize.x) / 2f;
            verticalOffset = (numericSize.y * cellSize.y - cellSize.y) / 2f;

            rotationStepLimit = new Vector3Int(Mathf.FloorToInt(rotationStep.x > 0 ? 360f / rotationStep.x : 0), Mathf.FloorToInt(rotationStep.y > 0 ? 360f / rotationStep.y : 0), Mathf.FloorToInt(rotationStep.z > 0 ? 360f / rotationStep.z : 0));

            for (int y = 0; y < numericSize.y; y++)
            {
                for (int x = 0; x < numericSize.x; x++)
                {
                    cellInstance = Instantiate(cellPrefabs.GetRandom(), transform);

                    cellInstance.transform.localPosition = new Vector3(cellSize.x * x - horizontalOffset, cellSize.y * y - verticalOffset, 0);

                    if (controlScale)
                    {
                        cellInstance.transform.localScale = new Vector3(cellSize.x * horizontalSpacing, cellSize.y * verticalSpacing, 1f);
                    }

                    if (randomizeOffset)
                    {
                        cellInstance.transform.localPosition += new Vector3(Random.Range(minOffset.x, maxOffset.x), Random.Range(minOffset.y, maxOffset.y), Random.Range(minOffset.z, maxOffset.z));
                    }

                    if (randomizeRotation)
                    {
                        cellInstance.transform.localEulerAngles = new Vector3(rotationStep.x * Random.Range(1, rotationStepLimit.x + 1), rotationStep.y * Random.Range(1, rotationStepLimit.y + 1), rotationStep.z * Random.Range(1, rotationStepLimit.z + 1));
                    }

                    if (randomizeScale)
                    {
                        cellInstance.transform.localScale = new Vector3(Random.Range(minScale.x, maxScale.x), Random.Range(minScale.y, maxScale.y), Random.Range(minScale.z, maxScale.z));
                    }

                    elements[y * numericSize.x + x] = cellInstance;
                }
            }

            print($" Elements generated: {elements.Length}");
        }
        else
        {
            Debug.Log(" --- Cell prefab is unset");
        }
    }

    public void Clear()
    {
        foreach (Transform cell in transform.GetChildren())
        {
            DestroyImmediate(cell.gameObject);
        }
    }
}
