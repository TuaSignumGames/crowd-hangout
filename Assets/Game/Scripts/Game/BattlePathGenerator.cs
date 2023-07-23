using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattlePathGenerator : MonoBehaviour
{
    public BattleUnit battleUnitPrefab;
    [Space]
    public GameObject entryCellPrefab;
    public GameObject groundCellPrefab;
    [Space]
    public GameObject[] obstacleCellPrefabs;
    public GameObject[] bossCellPrefabs;
    [Space]
    public int stageWidth;
    public int stageLength;
    [Space]
    public int stagesCount;
    [Space]
    public List<TypeMapLayer> terrainLayers;

    private BattlePathCellularStage[] stages;

    private BattlePathCellType[,] stageTypeMap;

    private BattlePathCell[,] stageGridCells;

    private GameObject stageContainer;

    private GameObject entryCellInstance;
    private GameObject bossCellInstance;

    private Vector3 stagePosition;

    private Vector3 gridCellSize;
    private Vector3 gridCellOffset;

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

        stages = new BattlePathCellularStage[stagesCount];

        stagePosition = transform.position;

        for (int i = 0; i < stagesCount; i++)
        {
            stages[i] = GenerateStage(GenerateStageMap(stageWidth, stageLength));
        }
    }

    private BattlePathCellularStage GenerateStage(BattlePathCellType[,] stageTypeMap)
    {
        stageContainer = new GameObject($"Stage[{transform.childCount}]");

        stageContainer.transform.SetParent(transform);
        stageContainer.transform.position = stagePosition;

        stageGridCells = new BattlePathCell[stageTypeMap.GetLength(0), stageTypeMap.GetLength(1)];

        entryCellInstance = Instantiate(entryCellPrefab, stagePosition, Quaternion.identity, stageContainer.transform);

        gridCellSize = groundCellPrefab.transform.localScale;

        gridCellOffset = new Vector3(entryCellInstance.transform.localScale.x, 0, (stageWidth - 1) * gridCellSize.x / 2f);

        for (int y = 0; y < stageLength; y++)
        {
            for (int x = 0; x < stageWidth; x++)
            {
                if (GetCellPrefab(stageTypeMap[x, y]))
                {
                    stageGridCells[x, y] = new BattlePathCell(Instantiate(GetCellPrefab(stageTypeMap[x, y]), transform.position + new Vector3(y * gridCellSize.x, 0, x * -gridCellSize.z) + gridCellOffset, Quaternion.identity, stageContainer.transform), new Vector2Int(x, y), stageTypeMap[x, y]);
                }
            }
        }

        bossCellInstance = Instantiate(bossCellPrefabs.GetRandom(), stageContainer.transform);

        bossCellInstance.transform.position = stagePosition + new Vector3(stageLength * gridCellSize.x + (entryCellInstance.transform.localScale.x + bossCellInstance.transform.localScale.x) / 2f, 0, 0);

        stagePosition += new Vector3((entryCellInstance.transform.localScale.x + bossCellInstance.transform.localScale.x) / 2f, 0, 0);

        return new BattlePathCellularStage(stageGridCells);
    }

    private void PlaceBattleUnits()
    {
        BattlePathCell[] stageGroundCells = new BattlePathCell[0];

        for (int i = 0; i < stages.Length; i++)
        {
            stageGroundCells = stages[i].GetCells(BattlePathCellType.Ground);

            for (int j = 0; j < 3; j++)
            {

            }
        }
    }

    private BattlePathCellType[,] GenerateStageMap(int width, int length)
    {
        stageTypeMap = new BattlePathCellType[width, length];

        float floatWidth = width;
        float floatLength = length;

        foreach (TypeMapLayer layer in terrainLayers)
        {
            perlinNoiseOrigin = new Vector2(Random.Range(-500f, 500f), Random.Range(-500f, 500f));

            for (int y = 0; y < length; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    perlinValue = Mathf.PerlinNoise(perlinNoiseOrigin.x + x / floatWidth * layer.frequency.x, perlinNoiseOrigin.y + y / floatLength * layer.frequency.y); //* horizontalVolumeModifier.Evaluate(j / floatWidth);

                    if (perlinValue < layer.volume)
                    {
                        stageTypeMap[x, y] = layer.cellType;
                    }
                }
            }
        }

        return stageTypeMap;
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
