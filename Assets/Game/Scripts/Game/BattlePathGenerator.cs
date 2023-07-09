using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattlePathGenerator : MonoBehaviour
{
    public GameObject groundCellPrefab;
    public GameObject[] obstacleCellPrefabs;
    [Space]
    public int stageWidth;
    public int stageLength;
    [Space]
    public List<TypeMapLayer> terrainLayers;

    private BattlePathCellType[,] typeMap;

    private Vector2 perlinNoiseOrigin;

    private float perlinValue;

    private void Awake()
    {
        Generate();
    }

    public void Generate()
    {
        if (transform.childCount > 0)
        {
            foreach (Transform child in transform.GetChildren())
            {
                DestroyImmediate(child.gameObject);
            }
        }

        GenerateStageMap(stageWidth, stageLength);

        Vector3 cellSize = groundCellPrefab.transform.localScale;

        float horizontalOffset = (stageWidth - 1) * cellSize.x / 2f;

        for (int i = 0; i < stageLength; i++)
        {
            for (int j = 0; j < stageWidth; j++)
            {
                if (GetCellPrefab(typeMap[j, i]))
                {
                    Instantiate(GetCellPrefab(typeMap[j, i]), transform.position + new Vector3(i * cellSize.x, 0, j * -cellSize.z + horizontalOffset), Quaternion.identity, transform);
                }
            }
        }
    }

    private void GenerateStageMap(int width, int length)
    {
        typeMap = new BattlePathCellType[width, length];

        float floatWidth = width;
        float floatLength = length;

        foreach (TypeMapLayer layer in terrainLayers)
        {
            perlinNoiseOrigin = new Vector2(Random.Range(-500f, 500f), Random.Range(-500f, 500f));

            for (int i = 0; i < length; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    perlinValue = Mathf.PerlinNoise(perlinNoiseOrigin.x + j / floatWidth * layer.frequency.x, perlinNoiseOrigin.y + i / floatLength * layer.frequency.y); //* horizontalVolumeModifier.Evaluate(j / floatWidth);

                    if (perlinValue < layer.volume)
                    {
                        typeMap[j, i] = layer.cellType;
                    }
                }
            }
        }
    }

    private GameObject GetCellPrefab(BattlePathCellType cellType)
    {
        switch (cellType)
        {
            case BattlePathCellType.Ground: return groundCellPrefab;
            case BattlePathCellType.Obstacle: return obstacleCellPrefabs[0];
        }

        return null;
    }
}

[System.Serializable]
public struct TypeMapLayer
{
    public BattlePathCellType cellType;
    [Range(0, 1f)]
    public float volume;
    public Vector2 frequency;
    //public AnimationCurve horizontalVolumeModifier;
}
