using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO
//
// Level
//  - Adaptive cave width [D2]
//  - Add Collectible types (Building, Baloon, Humanball, Multiplier) [D3]
//  - Cave correction for Collectibles [D4]
//  - Collectibles generation pattern [D4]
//
// Character
//  - AI for BattlePath [D2]
//  - Camera scale-by-size adaptation [D2]
//
// Upgrades [D5]
//  - Health
//  - Population
//  - Weapon
//
// Polishing [D5] 

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

        PlaceCollectibles();
    }

    private void Start()
    {
        PlayerController.Instance.Initialize();

        humanballTransform = PlayerController.Instance.Ball.Transform;
    }

    public void Generate()
    {
        GenerateBlocks();

        GenerateBattlePath(10);
    }

    private void LateUpdate()
    {
        if (!isCavePassed)
        {
            CheckForBattlePath();
        }
    }

    private void GenerateBlocks()
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
            InstantiateBlockPair(new Vector3(i * blockSettings.blockLength, offsetMap[i]));
        }

        blockSettings.blocksContainer.position = new Vector3(-blockSettings.blockLength * 3f, -offsetMap[3], 0);
    }

    private void GenerateBattlePath(int stagesCount)
    {
        if (battlePathSettings.stagesContainer.childCount > 0)
        {
            battlePathSettings.stagesContainer.RemoveChildrenImmediate();
        }

        battlePath = new BattlePath(battlePathSettings.pathContainer.gameObject);

        battlePath.transform.position = newBlockPair.floorBlockPosition + new Vector3(blockSettings.blockLength / 2f, 0, 0);

        for (int i = 0; i < stagesCount; i++)
        {
            newBattlePathStage = new BattlePathStage(Instantiate(battlePathSettings.stagePrefab, battlePathSettings.stagesContainer), i % 2 == 0);

            newBattlePathStage.Initialize(battlePath.position + new Vector3(battlePathSettings.baseStageTransform.localScale.x + i * newBattlePathStage.size.x, 0, 0), 100f + i * 100f, null);

            battlePath.stages.Add(newBattlePathStage);
        }
    }

    private void PlaceCollectibles()
    {
        for (int i = 0; i < blockPairs.Count; i++)
        {
            if (i > 5)
            {
                if (Random.Range(0, 101) < 80)
                {
                    newCollectible = Instantiate(collectibleSettings.humanCollectiblePrefabs[0], blockPairs[i].container.transform);

                    newCollectible.Initialize(blockPairs[i]);
                }
            }
        }
    }

    private BlockPair InstantiateBlockPair(Vector3 origin)
    {
        newBlockPairContainer = new GameObject("BlockPair");

        newBlockPairContainer.transform.position = origin;
        newBlockPairContainer.transform.SetParent(blockSettings.blocksContainer);

        newBlockPair = new BlockPair(Instantiate(blockSettings.ceilBlockPrefabs.GetRandom()), Instantiate(blockSettings.floorBlockPrefabs.GetRandom()), newBlockPairContainer);

        newBlockPair.ceilBlock.transform.localPosition = new Vector3(0, blockSettings.caveHeightRange.x / 2f + Random.Range(-blockSettings.blockDisplacementLimit, blockSettings.blockDisplacementLimit), 0);
        newBlockPair.floorBlock.transform.localPosition = new Vector3(0, -blockSettings.caveHeightRange.x / 2f + Random.Range(-blockSettings.blockDisplacementLimit, blockSettings.blockDisplacementLimit), 0);

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
        public float blockDisplacementLimit;
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
}