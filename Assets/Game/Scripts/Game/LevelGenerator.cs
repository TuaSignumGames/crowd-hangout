using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public static LevelGenerator Instance;

    public BlockSettings blockSettings;
    public List<WavePatternInfo> wavePatternSettings;
    [Space]
    public float levelLength;

    private List<BlockPair> blockPairs;

    private BlockPair newBlockPair;

    private GameObject newBlockPairContainer;

    private Vector2 perlinNoiseOrigin;

    private List<float> offsetMap;

    private float perlinValue;

    private int blockPairsCount;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        Generate();
    }

    public void Generate()
    {
        if (transform.childCount > 0)
        {
            transform.RemoveChildrenImmediate();

            transform.position = Vector3.zero;
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

        transform.position = new Vector3(-blockSettings.blockLength * 3f, -offsetMap[3], 0);
    }

    private BlockPair InstantiateBlockPair(Vector3 origin)
    {
        newBlockPairContainer = new GameObject("BlockPair");

        newBlockPairContainer.transform.position = origin;
        newBlockPairContainer.transform.SetParent(transform);

        newBlockPair = new BlockPair(Instantiate(blockSettings.ceilBlockPrefabs.GetRandom()), Instantiate(blockSettings.floorBlockPrefabs.GetRandom()), newBlockPairContainer);

        newBlockPair.ceilBlock.transform.localPosition = new Vector3(0, blockSettings.caveHeightRange.x / 2f + Random.Range(-blockSettings.blockDisplacementLimit, blockSettings.blockDisplacementLimit), 0);
        newBlockPair.floorBlock.transform.localPosition = new Vector3(0, -blockSettings.caveHeightRange.x / 2f + Random.Range(-blockSettings.blockDisplacementLimit, blockSettings.blockDisplacementLimit), 0);

        return newBlockPair;
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
        public GameObject[] ceilBlockPrefabs;
        public GameObject[] floorBlockPrefabs;
        public float blockLength;
        [Space]
        public Vector2 caveHeightRange;
        public float blockDisplacementLimit;
    }

    [System.Serializable]
    public class WavePatternInfo
    {
        [HideInInspector]
        public string title;

        public float waveHeight;
        public float waveFrequency;
    }

    public class BlockPair
    {
        public GameObject ceilBlock;
        public GameObject floorBlock;

        public GameObject container;

        public BlockPair(GameObject ceilBlock, GameObject floorBlock, GameObject container)
        {
            this.ceilBlock = ceilBlock;
            this.floorBlock = floorBlock;

            this.container = container;

            this.ceilBlock.transform.SetParent(container.transform);
            this.floorBlock.transform.SetParent(container.transform);
        }
    }
}