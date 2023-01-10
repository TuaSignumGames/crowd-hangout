using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatrixGenerator : MonoBehaviour
{
    public GameObject[] contentPrefabs;
    [Space]
    public Vector3 metricSize;
    public Vector3Int numericSize;
    public bool controlScale;
    [Space]
    public Vector3 contentSpacing = new Vector3(0.5f, 0.5f, 0.5f);
    public Vector3Bool centralize;

    private GameObject cellInstance;

    private Vector3 cellSize;
    private Vector3 cellQuarterSize;

    private Vector3 contentOffset;

    private float contentOffsetX;
    private float contentOffsetY;
    private float contentOffsetZ;

    public void Generate()
    {
        if (contentPrefabs.Length > 0)
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

            cellSize = new Vector3(metricSize.x / numericSize.x, metricSize.y / numericSize.y, metricSize.z / numericSize.z);
            cellQuarterSize = cellSize / 2f;

            contentOffsetX = centralize.x ? (numericSize.x * cellSize.x - cellSize.x) / 2f : 0;
            contentOffsetY = centralize.y ? (numericSize.y * cellSize.y - cellSize.y) / 2f : 0;
            contentOffsetZ = centralize.z ? (numericSize.z * cellSize.z - cellSize.z) / 2f : 0;

            contentOffset = new Vector3(contentOffsetX, contentOffsetY, contentOffsetZ);

            for (int z = 0; z < numericSize.z; z++)
            {
                for (int y = 0; y < numericSize.y; y++)
                {
                    for (int x = 0; x < numericSize.x; x++)
                    {
                        cellInstance = Instantiate(contentPrefabs.GetRandom(), transform);

                        cellInstance.transform.localPosition = new Vector3(cellSize.x * x, cellSize.y * y, cellSize.z * z) - contentOffset;

                        if (controlScale)
                        {
                            cellInstance.transform.localScale = new Vector3(cellSize.x * contentSpacing.x, cellSize.y * contentSpacing.y, cellSize.z * contentSpacing.z);
                        }
                        else
                        {
                            cellInstance.transform.localPosition += new Vector3(Random.Range(-cellQuarterSize.x, cellQuarterSize.x) * contentSpacing.x, Random.Range(-cellQuarterSize.y, cellQuarterSize.y) * contentSpacing.y, Random.Range(-cellQuarterSize.z, cellQuarterSize.z) * contentSpacing.z);
                        }
                    }
                }
            }
        }
        else
        {
            Debug.Log(" - Content list is empty!");
        }
    }
}
