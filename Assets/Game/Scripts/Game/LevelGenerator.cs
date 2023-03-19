using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO
//
// -> [Implement humanball returning after collision (RopeLength/DistanceToNextFloorBlock) / Continue main scope] <- 
//
// Level
//  - Add Collectible types (Multiplier, WeaponBox) 
//  - Collectibles generation pattern
//
// Upgrades
//  - Health
//  - Population
//  - Weapon
//
// Polishing

public class LevelGenerator : MonoBehaviour
{
    public static LevelGenerator Instance;

    public BlockSettings blockSettings;
    public List<WavePatternInfo> wavePatternSettings;
    [Space]
    public CollectibleSettings collectibleSettings;
    [Space]
    public BattlePathSettings battlePathSettings;
    [Space]
    public float levelLength;

    private List<BlockPair> blockPairs;

    private BattlePath battlePath;

    private Transform humanballTransform;

    private BlockPair newBlockPair;
    private BattlePathStage newBattlePathStage;

    private Multicollectible collectiblePrefab;
    private Multicollectible collectibleInstance;

    private GameObject newBlockPairContainer;

    private Vector2 perlinNoiseOrigin;

    private List<float> offsetMap;

    private float perlinValue;

    private int blockPairsCount;
    private int totalHumansCount;

    private bool isCavePassed;

    public BattlePath BattlePath => battlePath;

    public int TotalHumansCount => totalHumansCount;

    private void Awake()
    {
        Instance = this;

        Generate();
    }

    private void Start()
    {
        PlayerController.Instance.Initialize();

        PlayerController.Humanball.Structure.OnLayerIncremented += UpdateLevelConfiguration;

        humanballTransform = PlayerController.Humanball.Transform;
    }

    public void Generate()
    {
        GenerateBlocks();

        GenerateBattlePath(6);

        PlaceCollectibles();
    }

    public void GenerateFromEditor(bool collectibles, bool battlePath)
    {
        GenerateBlocks();

        if (collectibles)
        {
            PlaceCollectibles();
        }

        if (battlePath)
        {
            GenerateBattlePath(10);
        }
    }

    private void LateUpdate()
    {
        if (!isCavePassed)
        {
            CheckForBattlePath();
        }
        else
        {
            battlePath.Update();
        }
    }

    public void GenerateBlocks()
    {
        if (blockSettings.blocksContainer.childCount > 0)
        {
            blockSettings.blocksContainer.RemoveChildrenImmediate();

            blockSettings.blocksContainer.position = Vector3.zero;
        }

        blockPairs = new List<BlockPair>();

        blockPairsCount = Mathf.RoundToInt(levelLength / blockSettings.blockLength);

        perlinNoiseOrigin = new Vector2(Random.Range(-500f, 500f), Random.Range(-500f, 500f));

        offsetMap = new List<float>(new float[blockPairsCount]);

        for (int patternIndex = 0; patternIndex < wavePatternSettings.Count; patternIndex++)
        {
            for (int blockIndex = 0; blockIndex < blockPairsCount; blockIndex++)
            {
                perlinValue = Mathf.PerlinNoise(perlinNoiseOrigin.x + (float)blockIndex / blockPairsCount * wavePatternSettings[patternIndex].waveFrequency, perlinNoiseOrigin.y);

                offsetMap[blockIndex] += (perlinValue - 0.5f) * 2f * wavePatternSettings[patternIndex].waveHeight;
            }
        }

        for (int i = 0; i < offsetMap.Count; i++)
        {
            offsetMap[i] = Mathf.Round(offsetMap[i] / blockSettings.thresholdValue) * blockSettings.thresholdValue;

            InstantiateBlockPair(new Vector3(i * blockSettings.blockLength, offsetMap[i]), i);
        }

        blockSettings.blocksContainer.position = new Vector3(-blockSettings.blockLength * 3f, -offsetMap[3], 0);
    }

    private void GenerateBattlePath(int stagesCount)
    {
        battlePathSettings.pathContainer.gameObject.SetActive(true);

        if (battlePathSettings.stagesContainer.childCount > 0)
        {
            battlePathSettings.stagesContainer.RemoveChildrenImmediate();
        }

        battlePath = new BattlePath(battlePathSettings.pathContainer.gameObject);

        battlePath.transform.position = newBlockPair.FloorBlockPosition + new Vector3(blockSettings.blockLength / 2f, 0, 0);

        for (int i = 0; i < stagesCount; i++)
        {
            newBattlePathStage = new BattlePathStage(Instantiate(battlePathSettings.stagePrefab, battlePathSettings.stagesContainer), i % 2 == 0);

            newBattlePathStage.Initialize(battlePath.position + new Vector3(battlePathSettings.baseStageTransform.localScale.x + i * newBattlePathStage.size.x, 0, 0), 100f + i * 100f, battlePathSettings.guardPrefabs[0]);

            battlePath.stages.Add(newBattlePathStage);
        }
    }

    private void PlaceCollectibles()
    {
        int generatedHumansCount = 0;
        int nextAvailableBlockPairIndex = 0;

        float progressFactor = 0;

        for (int i = 0; i < blockPairs.Count; i++)
        {
            if (i > 5 && i < blockPairs.Count - 3 && i % 4 == 0)//(i == 10)//(i > 5)
            {
                if (i >= nextAvailableBlockPairIndex)
                {
                    progressFactor = i / (float)blockPairs.Count;

                    collectiblePrefab = collectibleSettings.collectibles.GetRandom().prefab;

                    collectibleInstance = Instantiate(collectiblePrefab, blockPairs[i + collectiblePrefab.RangeNumber].container.transform);

                    generatedHumansCount = Random.Range((int)Mathf.Lerp(1, 16, progressFactor), (int)Mathf.Lerp(5, 26, progressFactor));

                    blockPairs[i].AddCollectible(collectibleInstance, generatedHumansCount);

                    if (collectibleInstance.RangeNumber > 0)
                    {
                        AlignBlocksForCollectible(collectibleInstance, i);
                    }

                    nextAvailableBlockPairIndex = i + 1 + collectiblePrefab.RangeNumber * 2;

                    totalHumansCount += generatedHumansCount;

                    //return;
                }
            }
        }
    }

    private BlockPair InstantiateBlockPair(Vector3 origin, int orderIndex)
    {
        newBlockPairContainer = new GameObject("BlockPair");

        newBlockPairContainer.transform.position = origin;
        newBlockPairContainer.transform.SetParent(blockSettings.blocksContainer);

        newBlockPair = new BlockPair(Instantiate(blockSettings.ceilBlockPrefabs.GetRandom()), Instantiate(blockSettings.floorBlockPrefabs.GetRandom()), newBlockPairContainer, orderIndex);

        newBlockPair.SetHeight(Random.Range(blockSettings.caveHeightRange.x, blockSettings.caveHeightRange.y), blockSettings.thresholdValue);

        blockPairs.Add(newBlockPair);

        return newBlockPair;
    }

    private void CheckForBattlePath()
    {
        if (humanballTransform.position.x > battlePath.position.x)
        {
            isCavePassed = true;

            SwitchToBattleMode();
        }
    }

    private void SwitchToBattleMode()
    {
        PlayerController.Instance.SwitchToBattleMode();

        CameraController.Instance.Translate(battlePathSettings.viewLocalOffset, battlePathSettings.translationDuration, Space.Self);
        CameraController.Instance.Rotate(battlePathSettings.viewEulerAngles, battlePathSettings.rotationDuration, Space.World);
    }

    private void SetBlocksHeightIncrement(int startOrderIndex, HeightIncrementData incrementData)
    {
        if (startOrderIndex < blockPairs.Count - incrementData.transitionLength - 1)
        {
            float transitionLength = incrementData.transitionLength;

            for (int i = startOrderIndex; i < blockPairs.Count; i++)
            {
                blockPairs[i].SetHeight(blockPairs[i].Height + incrementData.heightIncrement * Mathf.Clamp01((i - startOrderIndex) / transitionLength), blockSettings.thresholdValue);
            }
        }
    }

    private void AlignBlocks(BlockPair referenceBlockPair, int lineIndex, int fromIndex, int toIndex)
    {
        for (int i = Mathf.Clamp(fromIndex, 0, blockPairs.Count - 1); i <= Mathf.Clamp(toIndex, 0, blockPairs.Count - 1); i++)
        {
            if (lineIndex == 0 || lineIndex == 1)
            {
                blockPairs[i].ceilBlock.transform.SetCoordinate(TransformComponent.Position, Axis.Y, Space.World, referenceBlockPair.CeilBlockPosition.y);
            }

            if (lineIndex == 0 || lineIndex == -1)
            {
                blockPairs[i].floorBlock.transform.SetCoordinate(TransformComponent.Position, Axis.Y, Space.World, referenceBlockPair.FloorBlockPosition.y);
            }
        }
    }

    private void AlignBlocksForCollectible(Collectible collectible, int pivotBlockPairIndex)
    {
        AlignBlocks(blockPairs[pivotBlockPairIndex], collectible.Placement == CollectiblePlacementType.Any ? 0 : (collectible.Placement == CollectiblePlacementType.Ceiling ? 1 : -1), pivotBlockPairIndex - collectible.RangeNumber, pivotBlockPairIndex + collectible.RangeNumber);
    }

    public void UpdateLevelConfiguration(int humanballLayerIndex)
    {
        SetBlocksHeightIncrement(GetBlockPair(humanballTransform.position).OrderIndex + blockSettings.heightIncrementSettings.transitionShift, blockSettings.heightIncrementSettings);

        for (int i = 0; i < blockPairs.Count; i++)
        {
            if (blockPairs[i].Collectible)
            {
                if (!blockPairs[i].Collectible.IsCollected)
                {
                    blockPairs[i].FitCollectible();
                }
            }
        }
    }

    public BlockPair GetBlockPair(Vector3 position)
    {
        for (int i = 0; i < blockPairs.Count; i++)
        {
            if (blockPairs[i].Position.x > position.x)
            {
                return blockPairs[i].Position.x - position.x < blockSettings.blockLength / 2f ? blockPairs[i] : blockPairs[Mathf.Clamp(i - 1, 0, blockPairs.Count - 1)];
            }
        }

        return blockPairs.GetLast();
    }

    private void OnValidate()
    {
        if (wavePatternSettings.Count > 0)
        {
            for (int i = 0; i < wavePatternSettings.Count; i++)
            {
                wavePatternSettings[i].title = i == 0 ? "Major Pattern" : $"Minor Pattern {i}";
            }
        }
    }

    [System.Serializable]
    public class BlockSettings
    {
        public Transform blocksContainer;
        public GameObject[] ceilBlockPrefabs;
        public GameObject[] floorBlockPrefabs;
        public float blockLength;
        [Space]
        public Vector2 caveHeightRange;
        public float thresholdValue;
        [Space]
        public HeightIncrementData heightIncrementSettings;
    }

    [System.Serializable]
    public class BattlePathSettings
    {
        public Transform pathContainer;
        public Transform stagesContainer;
        [Space]
        public Transform baseStageTransform;
        [Space]
        public GameObject stagePrefab;
        public List<GameObject> guardPrefabs;
        [Space]
        public Vector3 viewLocalOffset;
        public float translationDuration;
        [Space]
        public Vector3 viewEulerAngles;
        public float rotationDuration;
    }

    [System.Serializable]
    public class CollectibleSettings
    {
        public List<CollectibleData> collectibles;
        public AnimationCurve populationCurve;
    }

    [System.Serializable]
    public class WavePatternInfo
    {
        [HideInInspector]
        public string title;

        public float waveHeight;
        public float waveFrequency;
    }

    [System.Serializable]
    public struct CollectibleData
    {
        public float populationValue;
        public Multicollectible prefab;
    }

    [System.Serializable]
    public struct HeightIncrementData
    {
        public float heightIncrement;
        [Space]
        public int transitionShift;
        public int transitionLength;
    }
}