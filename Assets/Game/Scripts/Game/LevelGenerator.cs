using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO
//
// -> [HumanballProcessor --> UpdateContainerOrientation(p) issue: sliding along Z axis] <- 
//
// Level
//  - Add Collectible types (Building, Baloon, Humanball, Multiplier) 
//  - Cave correction for Collectibles
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
    private Collectible newCollectible;

    private GameObject newBlockPairContainer;

    private Vector2 perlinNoiseOrigin;

    private List<float> offsetMap;

    private float perlinValue;

    private int blockPairsCount;

    private bool isCavePassed;

    public BattlePath BattlePath => battlePath;

    private void Awake()
    {
        Instance = this;

        HumanController.defaultPose = collectibleSettings.humanPrefab.PeekPose();

        Generate();
    }

    private void Start()
    {
        PlayerController.Instance.Initialize();

        PlayerController.Instance.Ball.Structure.OnLayerIncremented += SetHeightIncrementLevel;

        humanballTransform = PlayerController.Instance.Ball.Transform;
    }

    public void Generate()
    {
        GenerateBlocks();

        PlaceCollectibles(100);

        GenerateBattlePath(6);
    }

    public void GenerateFromEditor(bool collectibles, bool battlePath)
    {
        GenerateBlocks();

        if (collectibles)
        {
            PlaceCollectibles(50);
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

        battlePath.transform.position = newBlockPair.floorBlockPosition + new Vector3(blockSettings.blockLength / 2f, 0, 0);

        for (int i = 0; i < stagesCount; i++)
        {
            newBattlePathStage = new BattlePathStage(Instantiate(battlePathSettings.stagePrefab, battlePathSettings.stagesContainer), i % 2 == 0);

            newBattlePathStage.Initialize(battlePath.position + new Vector3(battlePathSettings.baseStageTransform.localScale.x + i * newBattlePathStage.size.x, 0, 0), 100f + i * 100f, battlePathSettings.guardPrefabs[0]);

            battlePath.stages.Add(newBattlePathStage);
        }
    }

    private void PlaceCollectibles(int probability)
    {
        for (int i = 0; i < blockPairs.Count; i++)
        {
            if (i > 5)
            {
                if (Random.Range(0, 101) < Mathf.Clamp(probability, 0, 100))
                {
                    newCollectible = Instantiate(collectibleSettings.humanCollectiblePrefabs[0], blockPairs[i].container.transform);

                    newCollectible.Initialize(blockPairs[i]);
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

    public void SetHeightIncrementLevel(int incrementLevel)
    {
        HeightIncrementData incrementData = blockSettings.heightIncrementLevels[Mathf.Clamp(incrementLevel, 0, blockSettings.heightIncrementLevels.Length)];

        SetBlocksHeightIncrement(GetBlockPair(humanballTransform.position).OrderIndex + incrementData.transitionShift, incrementData);
    }

    public void SetBlocksHeightIncrement(int startOrderIndex, HeightIncrementData incrementData)
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

    public BlockPair GetBlockPair(Vector3 position)
    {
        for (int i = 0; i < blockPairs.Count; i++)
        {
            if (blockPairs[i].position.x > position.x)
            {
                return blockPairs[i].position.x - position.x < blockSettings.blockLength / 2f ? blockPairs[i] : blockPairs[Mathf.Clamp(i, 0, blockPairs.Count)];
            }
        }

        return null;
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
        public HeightIncrementData[] heightIncrementLevels;
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
        // TODO 
        //
        // Human prefab to bake default pose
        //
        // Collectible prefabs
        // HumanCollectible prefabs
        // Crowd(?)Collectible prefabs
        // AmmoCollectible prefabs
        // Multiplier prefabs

        public HumanController humanPrefab;
        [Space]
        public List<HumanCollectible> humanCollectiblePrefabs;
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
    public struct HeightIncrementData
    {
        public float heightIncrement;
        [Space]
        public int transitionShift;
        public int transitionLength;
    }
}