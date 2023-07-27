using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattlePathGenerator : MonoBehaviour
{
    public static BattlePathGenerator Instance;

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

    private List<BattlePathCell> stageGroundCells;

    private BattlePathCellularStage actualStage;

    private BattlePathCell stageGridPlacementCell;

    private BattleUnit battleUnitInstance;

    private GameObject stageContainer;

    private GameObject entryCellInstance;
    private GameObject bossCellInstance;

    private Vector3 stagePosition;

    private Vector3 gridCellSize;
    private Vector3 gridCellOffset;

    private Vector2 perlinNoiseOrigin;

    private float perlinValue;

    public BattlePathCellularStage ActualStage => actualStage;

    private void Awake()
    {
        Instance = this;

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

            GenerateEnemyBattleUnits(stages[i], 1f, 2);
        }

        GenerateAllieBattleUnits(stages[0], 1);

        actualStage = stages[0];
    }

    private BattlePathCellularStage GenerateStage(BattlePathCellType[,] stageTypeMap)
    {
        stageContainer = new GameObject($"Stage[{transform.childCount}]");

        BattlePathCellularStage newBattlePathStage = new BattlePathCellularStage();

        stageContainer.transform.SetParent(transform);
        stageContainer.transform.position = stagePosition;

        stageGridCells = new BattlePathCell[stageTypeMap.GetLength(0), stageTypeMap.GetLength(1)];

        entryCellInstance = Instantiate(entryCellPrefab, stagePosition, Quaternion.identity, stageContainer.transform);

        gridCellSize = groundCellPrefab.transform.localScale;

        gridCellOffset = new Vector3((entryCellInstance.transform.localScale.x + gridCellSize.x) / 2f, 0, (stageWidth - 1) * gridCellSize.x / 2f);

        for (int y = 0; y < stageLength; y++)
        {
            for (int x = 0; x < stageWidth; x++)
            {
                if (GetCellPrefab(stageTypeMap[x, y]))
                {
                    stageGridCells[x, y] = new BattlePathCell(Instantiate(GetCellPrefab(stageTypeMap[x, y]), stagePosition + new Vector3(y * gridCellSize.x, 0, x * -gridCellSize.z) + gridCellOffset, Quaternion.identity, stageContainer.transform), new BattlePathCellAddress(newBattlePathStage, x, y), stageTypeMap[x, y]);
                }
            }
        }

        bossCellInstance = Instantiate(bossCellPrefabs.GetRandom(), stageContainer.transform);

        bossCellInstance.transform.position = stagePosition + new Vector3(stageLength * gridCellSize.x + (entryCellInstance.transform.localScale.x + bossCellInstance.transform.localScale.x) / 2f, 0, 0);

        stagePosition = bossCellInstance.transform.position + new Vector3((entryCellInstance.transform.localScale.x + bossCellInstance.transform.localScale.x) / 2f, 0, 0);

        newBattlePathStage.AddCells(stageGridCells);

        return newBattlePathStage;
    }

    private void GenerateAllieBattleUnits(BattlePathCellularStage stage, int count)
    {
        stageGroundCells = new List<BattlePathCell>(stage.GetCells(BattlePathCellType.Ground, 0, 1f, 0, 0.25f));

        for (int i = 0; i < count; i++)
        {
            battleUnitInstance = Instantiate(battleUnitPrefab, stageContainer.transform);

            battleUnitInstance.PlaceAt(stageGroundCells.CutRandom());

            battleUnitInstance.GenerateGarrison(HumanTeam.Yellow, 5);

            stage.AddBattleUnit(battleUnitInstance);
        }
    }

    private void GenerateEnemyBattleUnits(BattlePathCellularStage stage, float power, int count)
    {
        stageGroundCells = new List<BattlePathCell>(stage.GetCells(BattlePathCellType.Ground, 0, 1f, 0.5f, 1f));

        for (int i = 0; i < count; i++)
        {
            battleUnitInstance = Instantiate(battleUnitPrefab, stageContainer.transform);

            stageGridPlacementCell = stageGroundCells.CutRandom();

            battleUnitInstance.PlaceAt(stageGridPlacementCell);

            battleUnitInstance.GenerateGarrison(HumanTeam.Red, 5);

            battleUnitInstance.DrawRange(stageGridPlacementCell);
            battleUnitInstance.SetRangeVisible(false);

            stage.AddBattleUnit(battleUnitInstance);
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
