using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    private BattlePathCell stageEntryCell;
    private BattlePathCell stageBossCell;

    private BattlePathCellularStage activeStage;

    private BattlePathStageInfo stageInfo;

    private BattlePathCell stageGridPlacementCell;

    private BattleUnit battleUnitInstance;

    private GameObject stageContainer;

    private Vector3 stagePosition;

    private Vector3 gridCellSize;
    private Vector3 gridCellOffset;

    private Vector2 perlinNoiseOrigin;

    private float perlinValue;

    private int previousStageIndex = -1;

    public BattlePathCellularStage ActiveStage => activeStage;

    public Vector3 Position => transform.position;
    public Vector3 GridCellSize => gridCellSize;

    private void Awake()
    {
        Instance = this;
    }

    public void Generate(Vector3 position)
    {
        if (transform.childCount > 0)
        {
            foreach (Transform child in transform.GetChildren())
            {
                DestroyImmediate(child.gameObject);
            }
        }

        transform.position = position;

        stages = new BattlePathCellularStage[stagesCount];

        stagePosition = position;

        for (int i = 0; i < stagesCount; i++)
        {
            stages[i] = GenerateStage(GenerateStageMap(stageWidth, stageLength), WorldManager.battlePathProgressionSettings.GetStageInfo(i), i);

            print($" - Stage position: {stagePosition}");

            GenerateEnemyBattleUnits(stages[i], 1f, 2);

            previousStageIndex++;
        }

        activeStage = stages[0];
    }

    public void Generate()
    {
        Generate(transform.position);
    }

    public void UpdateVisibility(int pivotStageIndex, Vector2Int visibilityRange)
    {
        for (int i = 0; i < stages.Length; i++)
        {
            stages[i].SetVisible(i >= Mathf.Clamp(pivotStageIndex + visibilityRange.x, 0, stages.Length - 1) && i <= Mathf.Clamp(pivotStageIndex + visibilityRange.y, 0, stages.Length - 1));
        }
    }

    public void EnterBattle(Crowd[] crews)
    {
        GenerateAllieBattleUnits(activeStage, crews);
    }

    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    public BattlePathCellularStage DefineStage(Vector3 position)
    {
        for (int i = 0; i < stages.Length; i++)
        {
            if (position.x < stages[i].Position.x)
            {
                return stages[Mathf.Clamp(i - 1, 0, stages.Length - 1)];
            }
        }

        return null;
    }

    private BattlePathCellularStage GenerateStage(BattlePathCellType[,] stageTypeMap, BattlePathStageInfo stageInfo, int orderIndex)
    {
        stageContainer = new GameObject($"Stage[{transform.childCount}]");

        stageInfo = WorldManager.battlePathProgressionSettings.GetStageInfo(orderIndex);

        activeStage = new BattlePathCellularStage(stageContainer, stageInfo.reward, orderIndex);

        stageContainer.transform.SetParent(transform);
        stageContainer.transform.position = stagePosition;

        stageGridCells = new BattlePathCell[stageTypeMap.GetLength(0), stageTypeMap.GetLength(1)];

        stageEntryCell = new BattlePathCell(Instantiate(entryCellPrefab, stagePosition, Quaternion.identity, stageContainer.transform), new BattlePathCellAddress(activeStage, 0, -1), BattlePathCellType.Ground);

        gridCellSize = groundCellPrefab.transform.localScale;

        gridCellOffset = new Vector3((stageEntryCell.Transform.localScale.x + gridCellSize.x) / 2f, 0, (stageWidth - 1) * gridCellSize.x / 2f);

        for (int y = 0; y < stageLength; y++)
        {
            for (int x = 0; x < stageWidth; x++)
            {
                if (GetCellPrefab(stageTypeMap[x, y]))
                {
                    stageGridCells[x, y] = new BattlePathCell(Instantiate(GetCellPrefab(stageTypeMap[x, y]), stagePosition + new Vector3(y * gridCellSize.x, 0, x * -gridCellSize.z) + gridCellOffset, Quaternion.identity, stageContainer.transform), new BattlePathCellAddress(activeStage, x, y), stageTypeMap[x, y]);
                }
            }
        }

        stageBossCell = new BattlePathCell(Instantiate(bossCellPrefabs.GetRandom(), stageContainer.transform), new BattlePathCellAddress(activeStage, 0, stageLength), BattlePathCellType.Ground);

        stageBossCell.Transform.position = stagePosition + new Vector3(stageLength * gridCellSize.x + (stageEntryCell.Transform.localScale.x + stageBossCell.Transform.localScale.x) / 2f, 0, 0);

        stagePosition = stageBossCell.Position + new Vector3((stageEntryCell.Transform.localScale.x + stageBossCell.Transform.localScale.x) / 2f, 0, 0);

        activeStage.AddCells(stageEntryCell, stageBossCell, stageGridCells);

        return activeStage;
    }

    private void GenerateAllieBattleUnits(BattlePathCellularStage stage, Crowd[] crews)
    {
        stageGroundCells = new List<BattlePathCell>(stage.GetCells(BattlePathCellType.Ground, 0, 1f, 0, 0.25f));

        for (int i = 0; i < crews.Length; i++)
        {
            battleUnitInstance = Instantiate(battleUnitPrefab, stageContainer.transform);

            battleUnitInstance.PlaceAt(stageGroundCells.CutRandom());

            battleUnitInstance.ApplyGarrison(crews[i]);

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

            battleUnitInstance.GenerateGarrison(HumanTeam.Red, 0);

            battleUnitInstance.UpdateRange(stageGridPlacementCell);
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
