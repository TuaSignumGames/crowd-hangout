using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    public GameObject cellPrefab;
    [Space]
    public Vector2 metricSize;
    public Vector2Int numericSize;
    [Range(0, 1f)]
    public float horizontalSpacing = 1f;
    [Range(0, 1f)]
    public float verticalSpacing = 1f;
    public Vector2 heightRange;

    private GameObject cellInstance;

    private Vector2 cellSize;

    private float horizontalOffset;

    public void Generate()
    {
        if (cellPrefab)
        {

#if UNITY_EDITOR

            if (transform.childCount > 0)
            {
                foreach (Transform cell in transform.GetChildren())
                {
                    DestroyImmediate(cell.gameObject);

                }
            }

#endif

            cellSize = new Vector2(metricSize.x / numericSize.x, metricSize.y / numericSize.y);

            horizontalOffset = (numericSize.x * cellSize.x - cellSize.x) / 2f;

            for (int y = 0; y < numericSize.y; y++)
            {
                for (int x = 0; x < numericSize.x; x++)
                {
                    cellInstance = Instantiate(cellPrefab, transform);

                    cellInstance.transform.position = new Vector3(cellSize.x * x - horizontalOffset, 0, cellSize.y * y);
                    cellInstance.transform.localScale = new Vector3(cellSize.x * horizontalSpacing, Random.Range(heightRange.x, heightRange.y), cellSize.y * verticalSpacing);
                }
            }
        }
        else
        {
            Debug.Log(" --- Cell prefab is unset");
        }
    }
}
