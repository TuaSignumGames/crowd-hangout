using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// - BattlePath: increase attack rate and motion speed by tapping 
//
// - Integrate WisdomSDK 
// - Check progression events 

public enum LevelElementType
{
    None, EnvironmentLake, EnvironmentWind, ObstacleBumperStatic, ObstacleBumperDynamic, ObstacleConfusion, DangerLava, DangerUpperSpikes, DangerPendulumBlade, DangerPatrol,
    PowerUpMultiplier, PowerUpMagnet, PowerUpPropeller, CollectibleCoins, CollectibleHuman, CollectibleWeapon
}

public class LevelGenerator : MonoBehaviour
{
    public static LevelGenerator Instance;

    public BlockSettings blockSettings;
    public DangerSettings dangerSettings;
    public CollectibleSettings collectibleSettings;
    public BattlePathSettings battlePathSettings;
    [Space]
    public LevelSettings levelSettings;

    private LevelData levelData;

    private ThemeInfo currentTheme;

    private ProgressionStageInfo stageInfo;

    private List<BlockPair> blockPairs;
    private List<LandscapeSegment> segments;
    private List<Collectible> collectibles;

    private BattlePath battlePath;

    private Transform humanballTransform;

    private BlockPair newBlockPair;
    private BattlePathStage newBattlePathStage;

    private LevelElementType[] elementMap;

    private Multicollectible multicollectibleInstance;

    private HumanMulticollectible humanCollectiblePrefab;
    private HumanMulticollectible humanCollectibleInstance;

    private WeaponMulticollectible weaponCollectiblePrefab;
    private WeaponMulticollectible weaponCollectibleInstance;

    private MagnetCollectible magnetCollectibleInstance;
    private PropellerCollectible propellerCollectibleInstance;

    private GameObject newBlockPairContainer;

    private BlockPair fracturedBlockPair;

    private ParticleSystem fracturedBlockVFX;

    private Vector2 perlinNoiseOrigin;

    private List<float> offsetMap;

    private float perlinValue;

    private float humanPower;
    private float levelPower;

    private float levelLength;

    private int humanCollectiblesCount;
    private int weaponCollectiblesCount;

    private int previousCollectiblePlacementIndex;

    private int totalHumansCount;
    private int totalWeaponsCount;

    private int structureIndex = 1;
    private int landscapeIndex = 1;
    private int cryticalStageIndex = 1;

    private int populationValue = 3;

    public static int topWeaponID = 10;
    public static int targetWeaponCount = 10;

    public static int[] availableWeaponIDs = new int[] { 6 }; //{ 4, 6 };

    private bool isCavePassed;

    private bool isLevelGenerated;

    public BattlePath BattlePath => battlePath;

    public int TotalHumansCount => totalHumansCount;

    private void Awake()
    {
        Instance = this;

        currentTheme = WorldManager.CurrentTheme;

        Generate();
    }

    private void Start()
    {
        PlayerController.Instance.Initialize();

        //PlayerController.Humanball.Structure.OnLayerIncremented += UpdateLevelConfiguration;

        humanballTransform = PlayerController.Humanball.Transform;

        foreach (HumanController human in WorldManager.yellowTeamHumans)
        {
            human.components.skinRenderer.material = WorldManager.humanMaterialPool.Eject();
        }
    }

    public void Generate()
    {
        if (GameManager.Instance.creativeMode)
        {
            structureIndex = CreativeManager.Instance.LevelPatternIndex;
            landscapeIndex = CreativeManager.Instance.LevelLandscapeIndex;
        }
        else
        {
            stageInfo = WorldManager.gameProgressionSettings.GetStageOf(LevelManager.LevelNumber);

            structureIndex = 16; //stageInfo.availableStructureIndices.GetRandom(); // 16 
            landscapeIndex = stageInfo.availableLandscapeIndices.GetRandom();
        }

        levelData = levelSettings.GetConfiguration(structureIndex, landscapeIndex);

        humanPower = WorldManager.GetWeaponPower(0);
        levelPower = humanPower;

        segments = new List<LandscapeSegment>();

        collectibles = new List<Collectible>();

        /*
        WeaponSetInfo weaponSet = WorldManager.weaponSets[CreativeManager.Instance.WeaponSetIndex];

        availableWeaponIDs = new int[weaponSet.weapons.Length];

        for (int i = 0; i < availableWeaponIDs.Length; i++)
        {
            availableWeaponIDs[i] = (int)weaponSet.weapons[i];
        }
        */

        BuildElementsMap();

        GenerateBlocks();

        GenerateEnvironments();

        GenerateObstacles();

        GenerateDangers();

        GeneratePowerUps();

        GenerateCollectibles();

        GenerateBattlePath(Mathf.Clamp(GameManager.CryticalStageIndex + 6, 10, int.MaxValue), GameManager.CryticalStageIndex);

        RebuildBlockPairsList();

        print($" -- Compositions Generated: \n Population: {GameManager.PopulationValue} / Top weapon ID: {WorldManager.GetWeaponID(GameManager.TopWeaponPower)} \n\n - Multicollectibles: {humanCollectiblesCount + weaponCollectiblesCount} (Human: {humanCollectiblesCount}[{totalHumansCount}], Weapon: {weaponCollectiblesCount}[{totalWeaponsCount}]) \n - Level power: {levelPower}");

        levelLength = blockPairs.GetLast().Position.x - blockPairs[3].Position.x;

        print($" -- Level {LevelManager.LevelNumber} Generated: \n\n - Stage: {stageInfo.title} (structure: '{levelSettings.structures[structureIndex].title}', landscape: '{levelSettings.landscapes[landscapeIndex].title}')");

        if (currentTheme.useBackground)
        {
            GenerateBackgroundLandscape(currentTheme.backgroundSettings);
        }

        isLevelGenerated = true;
    }

    private void BuildElementsMap()
    {
        elementMap = new LevelElementType[levelData.blocksCount];

        elementMap[previousCollectiblePlacementIndex = levelData.structureData.startStep.blocksCount] = levelData.structureData.startStep.elementType;

        for (int i = 0; i < levelData.structureData.cyclesCount; i++)
        {
            for (int j = 0; j < levelData.structureData.cycle.Count; j++)
            {
                elementMap[previousCollectiblePlacementIndex + levelData.structureData.cycle[j].blocksCount] = levelData.structureData.cycle[j].elementType;

                previousCollectiblePlacementIndex += levelData.structureData.cycle[j].blocksCount;
            }
        }

        elementMap[previousCollectiblePlacementIndex + levelData.structureData.endStep.blocksCount] = levelData.structureData.endStep.elementType;
    }
    /*
    public void GenerateFromEditor()
    {
        levelData = levelSettings.GetConfiguration(1, 1);

        currentTheme = UnityEditor.SceneAsset.FindObjectOfType<WorldManager>()._environmentSettings.themes[0];

        GenerateBlocks();
    }
    */
    private void LateUpdate()
    {
        if (isLevelGenerated)
        {
            if (isCavePassed)
            {
                battlePath.Update();

                UpdateBattlePathVisibility(battlePath.DefineStage(PlayerController.Humanball.Transform.position).OrderIndex);
            }
            else
            {
                CheckForBattlePath();

                UpdateLevelVisibility(GetBlockPair(PlayerController.Humanball.Transform.position).orderIndex);

                UIProgressBar.Instance.SetProgressValue(Mathf.Clamp01(PlayerController.Humanball.Transform.position.x / levelLength));
            }
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

        perlinNoiseOrigin = new Vector2(Random.Range(-500f, 500f), Random.Range(-500f, 500f));

        offsetMap = new List<float>(new float[levelData.blocksCount]);

        for (int patternIndex = 0; patternIndex < levelData.landscapeData.patterns.Count; patternIndex++)
        {
            for (int blockIndex = 0; blockIndex < levelData.blocksCount; blockIndex++)
            {
                perlinValue = Mathf.PerlinNoise(perlinNoiseOrigin.x + (float)blockIndex * levelData.landscapeData.patterns[patternIndex].waveFrequency, perlinNoiseOrigin.y);

                offsetMap[blockIndex] += (perlinValue - 0.5f) * 2f * levelData.landscapeData.patterns[patternIndex].waveHeight;
            }
        }

        for (int i = 0; i < offsetMap.Count; i++)
        {
            offsetMap[i] = Mathf.Round(offsetMap[i] / blockSettings.thresholdValue) * blockSettings.thresholdValue;

            InstantiateBlockPair(currentTheme, new Vector3(i * blockSettings.blockLength, offsetMap[i]), levelData.landscapeData.caveHeightRange, i);

            blockPairs.Add(newBlockPair);
        }

        blockSettings.blocksContainer.position = new Vector3(-blockSettings.blockLength * 3, -offsetMap[3], 0);
    }

    private void GenerateEnvironments()
    {
        for (int i = 0; i < blockPairs.Count; i++)
        {
            if (elementMap[i] != LevelElementType.None)
            {
                if (elementMap[i] == LevelElementType.EnvironmentWind)
                {
                    PlaceWind(blockPairs[i]);
                }
                if (elementMap[i] == LevelElementType.EnvironmentLake)
                {
                    GenerateLake(i);
                }
            }
        }
    }

    private void GenerateObstacles()
    {
        for (int i = 0; i < blockPairs.Count; i++)
        {
            if (elementMap[i] != LevelElementType.None)
            {
                if (elementMap[i] == LevelElementType.ObstacleBumperDynamic)
                {
                    PlaceDynamicBumper(blockPairs[i]);
                }
                if (elementMap[i] == LevelElementType.ObstacleBumperStatic)
                {
                    PlaceStaticBumper(blockPairs[i]);
                }
                if (elementMap[i] == LevelElementType.ObstacleConfusion)
                {
                    GenerateConfusion(i);
                }
            }
        }
    }

    private void GenerateDangers()
    {
        for (int i = 0; i < blockPairs.Count; i++)
        {
            if (elementMap[i] != LevelElementType.None)
            {
                if (elementMap[i] == LevelElementType.DangerLava)
                {
                    GenerateLava(i);
                }
                if (elementMap[i] == LevelElementType.DangerPatrol)
                {
                    PlaceEnemyOnBlock(blockPairs[i], 6);
                }
                if (elementMap[i] == LevelElementType.DangerPendulumBlade)
                {
                    PlacePendulumBlade(blockPairs[i]);
                }
                if (elementMap[i] == LevelElementType.DangerUpperSpikes)
                {
                    PlaceSpikes(blockPairs[i]);
                }
            }
        }
    }

    private void GeneratePowerUps()
    {
        for (int i = 0; i < blockPairs.Count; i++)
        {
            if (elementMap[i] != LevelElementType.None)
            {
                if (elementMap[i] == LevelElementType.PowerUpMagnet)
                {
                    PlaceMagnet(blockPairs[i]);
                }
                if (elementMap[i] == LevelElementType.PowerUpMultiplier)
                {
                    PlaceHumanMultiplier(blockPairs[i], 2f);
                }
                if (elementMap[i] == LevelElementType.PowerUpPropeller)
                {
                    PlacePropeller(blockPairs[i]);
                }
            }
        }
    }

    private void GenerateCollectibles()
    {
        for (int i = 0; i < elementMap.Length; i++)
        {
            if (elementMap[i] != LevelElementType.None)
            {
                if (elementMap[i] == LevelElementType.CollectibleCoins)
                {
                    GenerateCoins();
                }
                if (elementMap[i] == LevelElementType.CollectibleHuman)
                {
                    humanCollectiblesCount++;
                }
                if (elementMap[i] == LevelElementType.CollectibleWeapon)
                {
                    weaponCollectiblesCount++;
                }
            }
        }

        PlaceHumanCollectibles();
        PlaceWeaponCollectibles();
    }

    private LandscapeSegment GenerateSegment(int startBlockIndex, LandscapeSegmentData segmentData)
    {
        BlockPair[] segmentBlockPairs = new BlockPair[segmentData.segmentSize];

        float placementFactor = 0;
        //float previousSegmentsLength = 0;

        float segmentLength = segmentData.segmentSize * blockSettings.blockLength;

        /*
        for (int i = 0; i < segments.Count; i++)
        {
            previousSegmentsLength += segments[i].blockPairs.Count * blockSettings.blockLength;
        }

        print(previousSegmentsLength);
        */

        for (int i = 0; i < segmentData.segmentSize; i++)
        {
            placementFactor = i / (segmentData.segmentSize - 1f);

            newBlockPair = InstantiateBlockPair(currentTheme.GetBlockPrefab(segmentData.ceilingProfile.blockType), currentTheme.GetBlockPrefab(segmentData.groundProfile.blockType), blockPairs[startBlockIndex].Position + new Vector3((i + 1) * blockSettings.blockLength, 0, 0), levelData.landscapeData.caveHeightRange, i);

            newBlockPair.ceilingBlock.transform.position += new Vector3(0, segmentData.ceilingProfile.profileCurve.Evaluate(placementFactor) * segmentData.ceilingProfile.profileAmplitude, 0);
            newBlockPair.groundBlock.transform.position += new Vector3(0, segmentData.groundProfile.profileCurve.Evaluate(placementFactor) * segmentData.groundProfile.profileAmplitude, 0);

            segmentBlockPairs[i] = newBlockPair;
        }

        for (int i = startBlockIndex + 1; i < blockPairs.Count; i++)
        {
            blockPairs[i].container.transform.position += new Vector3(segmentLength, 0, 0);
        }

        for (int i = 0; i < segments.Count; i++)
        {
            if (segments[i].blockPairs[0].Position.x > segmentBlockPairs[0].Position.x)
            {
                for (int j = 0; j < segments[i].blockPairs.Count; j++)
                {
                    segments[i].blockPairs[j].container.transform.position += new Vector3(segmentLength, 0, 0);
                }
            }
        }

        float ceilingBaseHeight = Mathf.Max(blockPairs[startBlockIndex].ceilingBlock.transform.position.y, blockPairs[startBlockIndex + 1].ceilingBlock.transform.position.y);
        float groundBaseHeight = Mathf.Min(blockPairs[startBlockIndex].groundBlock.transform.position.y, blockPairs[startBlockIndex + 1].groundBlock.transform.position.y);

        return new LandscapeSegment(segmentBlockPairs, ceilingBaseHeight, groundBaseHeight);
    }

    private void GenerateLake(int startBlockIndex)
    {
        LandscapeSegment lakeSegment = GenerateSegment(startBlockIndex, WorldManager.environmentSettings.segments.lakeSegmentData);

        lakeSegment.blockPairs.GetFirst().groundBlock.transform.SetCoordinate(TransformComponent.Position, Axis.Y, Space.World, blockPairs[startBlockIndex].groundBlock.transform.position.y);
        lakeSegment.blockPairs.GetLast().groundBlock.transform.SetCoordinate(TransformComponent.Position, Axis.Y, Space.World, blockPairs[startBlockIndex + 1].groundBlock.transform.position.y);

        lakeSegment.CalculateDepths();

        GameObject waterBlockInstance = null;

        float waterBlockHeight = 0;

        for (int i = 0; i < lakeSegment.blockPairs.Count; i++)
        {
            waterBlockInstance = Instantiate(currentTheme.forceAreaPrefabs.waterAreaPrefab, lakeSegment.blockPairs[i].groundBlock.transform);

            waterBlockHeight = lakeSegment.groundDepths[i] - 0.5f;

            if (waterBlockHeight > 0)
            {
                waterBlockInstance.transform.position = lakeSegment.blockPairs[i].groundBlock.transform.position;
                waterBlockInstance.transform.localScale = new Vector3(1f, 1f, waterBlockHeight);
            }
            else
            {
                waterBlockInstance.SetActive(false);
            }
        }

        segments.Add(lakeSegment);
    }

    private void GenerateConfusion(int startBlockIndex)
    {
        LandscapeSegment confusionSegment = GenerateSegment(startBlockIndex, WorldManager.environmentSettings.segments.confusionSegmentData);

        Transform groundBlockContentTransform = null;
        BoxCollider groundBlockCollider = null;

        for (int i = 0; i < confusionSegment.blockPairs.Count; i++)
        {
            groundBlockContentTransform = confusionSegment.blockPairs[i].floorBlockContent[0];
            groundBlockCollider = confusionSegment.blockPairs[i].groundBlock.GetComponent<BoxCollider>();

            groundBlockContentTransform.localPosition = new Vector3(0, -groundBlockContentTransform.localPosition.y, 0);
            groundBlockCollider.center = new Vector3(0, -groundBlockCollider.center.y, 0);
        }

        segments.Add(confusionSegment);
    }

    private void GenerateLava(int startBlockIndex)
    {
        LandscapeSegment lavaSegment = GenerateSegment(startBlockIndex, WorldManager.environmentSettings.segments.lavaSegmentData);

        Transform groundBlockContentTransform = null;
        BoxCollider groundBlockCollider = null;

        for (int i = 0; i < lavaSegment.blockPairs.Count; i++)
        {
            groundBlockContentTransform = lavaSegment.blockPairs[i].floorBlockContent[0];
            groundBlockCollider = lavaSegment.blockPairs[i].groundBlock.GetComponent<BoxCollider>();

            groundBlockContentTransform.localPosition = new Vector3(0, -groundBlockContentTransform.localPosition.y, 0);
            groundBlockCollider.center = new Vector3(0, -groundBlockCollider.center.y, 0);
        }

        lavaSegment.CalculateDepths();

        GameObject lavaBlockInstance = null;

        float lavaBlockHeight = 0;

        for (int i = 0; i < lavaSegment.blockPairs.Count; i++)
        {
            lavaBlockInstance = Instantiate(currentTheme.lavaBlockPrefab, lavaSegment.blockPairs[i].groundBlock.transform);

            lavaBlockHeight = lavaSegment.groundDepths[i] - 0.5f;

            if (lavaBlockHeight > 0)
            {
                lavaBlockInstance.transform.position = lavaSegment.blockPairs[i].groundBlock.transform.position;
                lavaBlockInstance.transform.localScale = new Vector3(1f, 1f, lavaBlockHeight);
            }
            else
            {
                lavaBlockInstance.SetActive(false);
            }
        }

        segments.Add(lavaSegment);
    }

    private void GenerateCoins()
    {

    }

    public void GenerateBackgroundLandscape(BackgroundSettings backgroundSettings)
    {
        if (WorldManager.environmentSettings.backgroundContainer.childCount > 0)
        {
            WorldManager.environmentSettings.backgroundContainer.RemoveChildrenImmediate();

            WorldManager.environmentSettings.backgroundContainer.position = Vector3.zero;
        }

        float[,] backgroundLandscapeMatrix = new float[backgroundSettings.backgroundWidth, offsetMap.Count];
        float[,] foregroundLandscapeMatrix = new float[backgroundSettings.foregroundWidth, offsetMap.Count];

        GameObject[,] blocks = new GameObject[backgroundSettings.backgroundWidth, offsetMap.Count];

        for (int patternIndex = 0; patternIndex < levelData.landscapeData.patterns.Count; patternIndex++)
        {
            for (int x = 0; x < backgroundLandscapeMatrix.GetLength(0); x++)
            {
                for (int y = 0; y < backgroundLandscapeMatrix.GetLength(1); y++)
                {
                    perlinValue = Mathf.PerlinNoise(perlinNoiseOrigin.x + (float)y * levelData.landscapeData.patterns[patternIndex].waveFrequency, perlinNoiseOrigin.y + (float)x * backgroundSettings.landscapeSettings.waveFrequency);

                    perlinValue *= backgroundSettings.profileCurve.Evaluate(x / (float)backgroundSettings.backgroundWidth) * backgroundSettings.heightMultiplier;

                    backgroundLandscapeMatrix[x, y] += (perlinValue - 0.5f) * 2f * levelData.landscapeData.patterns[patternIndex].waveHeight;
                }
            }

            for (int x = 0; x < foregroundLandscapeMatrix.GetLength(0); x++)
            {
                for (int y = 0; y < foregroundLandscapeMatrix.GetLength(1); y++)
                {
                    perlinValue = Mathf.PerlinNoise(perlinNoiseOrigin.x + (float)y * levelData.landscapeData.patterns[patternIndex].waveFrequency, perlinNoiseOrigin.y - (float)x * backgroundSettings.landscapeSettings.waveFrequency);

                    foregroundLandscapeMatrix[x, y] += (perlinValue - 0.5f) * 2f * levelData.landscapeData.patterns[patternIndex].waveHeight;
                }
            }
        }

        GameObject blockInstance = null;

        for (int x = 0; x < backgroundLandscapeMatrix.GetLength(0); x++)
        {
            for (int y = 0; y < backgroundLandscapeMatrix.GetLength(1); y++)
            {
                blockInstance = Instantiate(backgroundSettings.blockPrefab, WorldManager.environmentSettings.backgroundContainer);

                if (x > backgroundSettings.treeDistancingThreshold && Random.Range(0, 100f) < backgroundSettings.treePlacementProbability)
                {
                    Instantiate(backgroundSettings.treePrefab, blockInstance.transform.position, Quaternion.identity, blockInstance.transform);
                }

                backgroundLandscapeMatrix[x, y] = Mathf.Round(backgroundLandscapeMatrix[x, y] / blockSettings.thresholdValue) * blockSettings.thresholdValue;

                blockInstance.transform.position = new Vector3(blockSettings.blockLength * y, backgroundSettings.centerOffset.y + backgroundLandscapeMatrix[x, y] + blockPairs[x].groundBlock.transform.localPosition.y, backgroundSettings.centerOffset.x + blockSettings.blockLength * x);
                blockInstance.transform.eulerAngles = new Vector3(0, 90f * Random.Range(0, 4), 0);
            }
        }

        for (int x = 0; x < foregroundLandscapeMatrix.GetLength(0); x++)
        {
            for (int y = 0; y < foregroundLandscapeMatrix.GetLength(1); y++)
            {
                blockInstance = Instantiate(backgroundSettings.blockPrefab, WorldManager.environmentSettings.backgroundContainer);

                foregroundLandscapeMatrix[x, y] = Mathf.Round(foregroundLandscapeMatrix[x, y] / blockSettings.thresholdValue) * blockSettings.thresholdValue;

                blockInstance.transform.position = new Vector3(blockSettings.blockLength * y, backgroundSettings.centerOffset.y + foregroundLandscapeMatrix[x, y] + blockPairs[x].groundBlock.transform.localPosition.y, -backgroundSettings.centerOffset.x - blockSettings.blockLength * x);
                blockInstance.transform.eulerAngles = new Vector3(0, 90f * Random.Range(0, 4), 0);
            }
        }

        WorldManager.environmentSettings.backgroundContainer.position = blockSettings.blocksContainer.position;
    }

    private void GenerateBattlePath(int stagesCount, int cryticalStageIndex)
    {
        BattlePathStageInfo battleBathStageInfo = null;

        battlePathSettings.pathContainer.gameObject.SetActive(true);

        if (battlePathSettings.stagesContainer.childCount > 0)
        {
            battlePathSettings.stagesContainer.RemoveChildrenImmediate();
        }

        battlePath = new BattlePath(battlePathSettings.pathContainer.gameObject);

        battlePath.transform.position = blockPairs.GetLast().FloorBlockPosition + new Vector3(blockSettings.blockLength / 2f, 0, 0);

        currentTheme.battlePathBaseStage.SetActive(true);

        for (int i = 0; i < stagesCount; i++)
        {
            battleBathStageInfo = WorldManager.battlePathProgressionSettings.GetStageInfo(i);

            newBattlePathStage = new BattlePathStage(Instantiate(currentTheme.battlePathStagePrefab, battlePathSettings.stagesContainer), i % 2 == 0);

            newBattlePathStage.Initialize(battlePath.Position + new Vector3(currentTheme.battlePathBaseStage.transform.localScale.x + i * newBattlePathStage.Size.x, 0, 0), battleBathStageInfo.reward, i);
            newBattlePathStage.GenerateGuard(battleBathStageInfo.guardiansCount, levelPower * ((i + 1) / (float)(cryticalStageIndex + 1)));

            battlePath.stages.Add(newBattlePathStage);

            //print($" BPStage generated - guard: {newBattlePathStage.GuardCrew.MembersCount} / reward: {newBattlePathStage.Reward}");
        }

        battlePath.SetActive(false);
    }

    private void RebuildBlockPairsList()
    {
        blockPairs = new List<BlockPair>();

        GameObject[] blockPairElements = new GameObject[0];

        GameObject[] blockPairContainers = blockSettings.blocksContainer.GetGameObjectsInChildren();

        for (int i = 0; i < blockPairContainers.Length; i++)
        {
            blockPairElements = blockPairContainers[i].transform.GetGameObjectsInChildren();

            blockPairs.Add(new BlockPair(blockPairElements[0], blockPairElements[1], blockPairContainers[i], i, blockSettings.thresholdValue));
        }

        blockPairs.Sort((a, b) => a.Position.x.CompareTo(b.Position.x));

        for (int i = 0; i < blockPairs.Count; i++)
        {
            blockPairs[i].orderIndex = i;

            blockPairs[i].container.transform.SetSiblingIndex(i);
        }
    }

    private void RemoveCollectibles()
    {
        for (int i = 0; i < collectibles.Count; i++)
        {
            collectibles[i].SetVisible(false);

            Destroy(collectibles[i].gameObject);
        }

        totalHumansCount = 0;
        totalWeaponsCount = 0;

        levelPower = 0;
    }

    private void RemoveBattlePath()
    {
        battlePath.Clear();
    }

    private void PlaceHumanCollectibles()
    {
        int populationValue = GameManager.PopulationValue;

        int[] humanCollectibleCountMap = new int[humanCollectiblesCount];

        if (populationValue > humanCollectiblesCount)
        {
            List<float> humanCollectiblePortionFactors = new List<float>();

            float humanCollectiblePortionSum = 0;

            for (int i = 0; i < elementMap.Length; i++)
            {
                if (elementMap[i] == LevelElementType.CollectibleHuman)
                {
                    humanCollectiblePortionFactors.Add(collectibleSettings.populationCurve.Evaluate(i / (float)elementMap.Length));

                    humanCollectiblePortionSum += humanCollectiblePortionFactors.GetLast();
                }
            }

            int actualHumansSum = 0;

            for (int i = 0; i < humanCollectibleCountMap.Length; i++)
            {
                humanCollectibleCountMap[i] = Mathf.CeilToInt(populationValue * humanCollectiblePortionFactors[i] / humanCollectiblePortionSum);

                actualHumansSum += humanCollectibleCountMap[i];
            }

            if (actualHumansSum > populationValue)
            {
                humanCollectibleCountMap[humanCollectibleCountMap.GetIndexOfMax()] -= actualHumansSum - populationValue;
            }
        }
        else
        {
            for (int i = 0; i < humanCollectibleCountMap.Length; i++)
            {
                humanCollectibleCountMap[i] = i == 0 ? populationValue : 0;
            }
        }

        Queue<int> humanPortionQueue = new Queue<int>(humanCollectibleCountMap);

        for (int i = 0; i < blockPairs.Count; i++)
        {
            if (elementMap[i] == LevelElementType.CollectibleHuman && humanPortionQueue.Peek() > 0)
            {
                PlaceHumanCollectible(blockPairs[i], humanPortionQueue.Dequeue());

                AlignBlocksForCollectible(multicollectibleInstance, i);
            }
        }
    }

    private void PlaceWeaponCollectibles()
    {
        int topWeaponID = WorldManager.GetWeaponID(GameManager.TopWeaponPower);

        if (topWeaponID > 0)
        {
            int targetWeaponCount = Random.Range(Mathf.CeilToInt(totalHumansCount / 2f), totalHumansCount + 1);

            List<WeaponMulticollectibleInfo> weaponPlacementTable = new List<WeaponMulticollectibleInfo>();

            if (targetWeaponCount > 1)
            {
                Dictionary<int, float> weaponPortionFactorTable = new Dictionary<int, float>();

                float weaponPortionFactorSum = 0;

                int weaponPortionFactorKey = 0;

                for (int i = 0; i < weaponCollectiblesCount; i++)
                {
                    weaponPortionFactorKey = i - weaponCollectiblesCount + topWeaponID + 1;

                    weaponPortionFactorTable.Add(weaponPortionFactorKey, ((float)(weaponCollectiblesCount / (i + 1))) * Random.Range(0.9f, 1.1f));

                    weaponPortionFactorSum += weaponPortionFactorTable.GetValueOrDefault(weaponPortionFactorKey);
                }

                int actualWeaponSum = 0;

                foreach (int key in weaponPortionFactorTable.Keys)
                {
                    weaponPlacementTable.Add(new WeaponMulticollectibleInfo(Mathf.Clamp(key, 1, topWeaponID), Mathf.CeilToInt(targetWeaponCount * weaponPortionFactorTable.GetValueOrDefault(key) / weaponPortionFactorSum)));

                    actualWeaponSum += weaponPlacementTable.GetLast().count;
                }

                if (actualWeaponSum > targetWeaponCount)
                {
                    weaponPlacementTable[0].count -= actualWeaponSum - targetWeaponCount;
                }
            }
            else
            {
                weaponPlacementTable.Add(new WeaponMulticollectibleInfo(topWeaponID, 1));
            }

            for (int i = 0; i < weaponPlacementTable.Count; i++)
            {
                if (weaponPlacementTable[i].id != topWeaponID)
                {
                    weaponPlacementTable[i].id = Random.Range(1, topWeaponID);
                }
            }

            WeaponMulticollectibleInfo placementInfo;

            for (int i = 0; i < blockPairs.Count; i++)
            {
                if (elementMap[i] == LevelElementType.CollectibleWeapon)
                {
                    placementInfo = weaponPlacementTable.CutAt(weaponPlacementTable.Count - 1); //weaponPlacementTable.Count > 1 ? weaponPlacementTable.CutRandom(0, weaponPlacementTable.Count - 2) : weaponPlacementTable.CutAt(0);

                    PlaceWeaponCollectible(blockPairs[i], placementInfo.id, Mathf.Clamp(placementInfo.count, 1, totalHumansCount));

                    if (multicollectibleInstance.RangeNumber > 0)
                    {
                        AlignBlocksForCollectible(multicollectibleInstance, i);
                    }
                }
            }
        }
    }

    private void PlaceWind(BlockPair blockPair)
    {
        Transform windAreaTransform = Instantiate(currentTheme.forceAreaPrefabs.windAreaPrefab, blockPair.container.transform).transform;

        windAreaTransform.position = blockPair.groundBlock.transform.position;

        windAreaTransform.forward = new Vector3(0, 1, 0);
    }

    private void PlaceStaticBumper(BlockPair blockPair)
    {

    }

    private void PlaceDynamicBumper(BlockPair blockPair)
    {

    }

    private void PlaceSpikes(BlockPair blockPair)
    {
        Instantiate(dangerSettings.spikesPrefab, blockPair.CeilBlockPosition, Quaternion.identity, blockPair.ceilingBlock.transform);
    }

    private void PlacePendulumBlade(BlockPair blockPair)
    {
        Instantiate(dangerSettings.bladePendulumPrefab, blockPair.CeilBlockPosition, Quaternion.identity, blockPair.ceilingBlock.transform);
    }

    public void PlaceEnemyOnBlock(BlockPair blockPair, int weaponID)
    {
        HumanController enemyInstance = Instantiate(WorldManager.humanPrefab, blockPair.FloorBlockPosition - WorldManager.humanPrefab.components.animator.transform.localPosition, Quaternion.identity, blockPair.groundBlock.transform);

        enemyInstance.Initialize(HumanTeam.Red, weaponID);

        enemyInstance.defaultContainer = blockPair.groundBlock.transform;

        enemyInstance.DropOnBlock(blockPair, -Vector3.right);

        enemyInstance.SetPose(enemyInstance.poseSettings.additionalPoses[0]);

        enemyInstance.AI.SetEnemy(WorldManager.GetClosestHuman(HumanTeam.Yellow, enemyInstance.transform.position));

        enemyInstance.AI.Defend();
    }

    private void PlaceHumanMultiplier(BlockPair blockPair, float multiplier)
    {

    }

    private void PlaceMagnet(BlockPair blockPair)
    {
        magnetCollectibleInstance = Instantiate(collectibleSettings.magnetCollectible);

        magnetCollectibleInstance.Initialize();

        blockPair.AddCollectible(magnetCollectibleInstance, magnetCollectibleInstance.collectibleSettings.placementRange.Value);

        collectibles.Add(magnetCollectibleInstance);
    }

    private void PlacePropeller(BlockPair blockPair)
    {
        propellerCollectibleInstance = Instantiate(collectibleSettings.propellerCollectible);

        propellerCollectibleInstance.Initialize();

        blockPair.AddCollectible(propellerCollectibleInstance, propellerCollectibleInstance.collectibleSettings.placementRange.Value);

        collectibles.Add(propellerCollectibleInstance);
    }

    private void PlaceHumanCollectible(BlockPair blockPair, int humansCount)
    {
        humanCollectiblePrefab = collectibleSettings.GetAvailableHumanCollectiblePrefabs(GameManager.PopulationValue).GetRandom();

        humanCollectibleInstance = Instantiate(humanCollectiblePrefab, blockPair.container.transform);
        humanCollectibleInstance.Initialize(humansCount);

        blockPair.AddCollectible(humanCollectibleInstance, humanCollectibleInstance.collectibleSettings.placementRange.Value);

        totalHumansCount += humansCount;
        levelPower += humanPower * humansCount;

        multicollectibleInstance = humanCollectibleInstance;

        collectibles.Add(multicollectibleInstance);
    }

    private void PlaceWeaponCollectible(BlockPair blockPair, int weaponID, int weaponsCount)
    {
        weaponCollectiblePrefab = collectibleSettings.weaponCollectibles.GetRandom();

        weaponCollectibleInstance = Instantiate(weaponCollectiblePrefab, blockPair.container.transform);
        weaponCollectibleInstance.Initialize(weaponID, weaponsCount);

        blockPair.AddCollectible(weaponCollectibleInstance, weaponCollectibleInstance.collectibleSettings.placementRange.Value);

        totalWeaponsCount += weaponsCount;
        levelPower += (WorldManager.GetWeaponPower(weaponID) - humanPower) * weaponsCount;

        multicollectibleInstance = weaponCollectibleInstance;

        collectibles.Add(multicollectibleInstance);
    }

    private BlockPair InstantiateBlockPair(ThemeInfo theme, Vector3 origin, Vector2 heightRange, int orderIndex)
    {
        return InstantiateBlockPair(theme.ceilingBlockPrefab, theme.groundBlockPrefab, origin, heightRange, orderIndex);
    }

    public BlockPair InstantiateBlockPair(GameObject ceilingBlockPrefab, GameObject floorBlockPrefab, Vector3 origin, Vector2 heightRange, int orderIndex)
    {
        newBlockPairContainer = new GameObject("BlockPair");

        newBlockPairContainer.transform.position = origin;
        newBlockPairContainer.transform.SetParent(blockSettings.blocksContainer);

        newBlockPair = new BlockPair(Instantiate(ceilingBlockPrefab), Instantiate(floorBlockPrefab), newBlockPairContainer, orderIndex, blockSettings.thresholdValue);

        newBlockPair.SetHeight(Random.Range(heightRange.x, heightRange.y));

        return newBlockPair;
    }

    private void CheckForBattlePath()
    {
        if (humanballTransform.position.x > battlePath.Position.x)
        {
            isCavePassed = true;

            SwitchToBattleMode();
        }
    }

    private void SwitchToBattleMode()
    {
        PlayerController.Instance.SwitchToBattleMode();

        CameraController.Instance.SetView(LevelGenerator.Instance.battlePathSettings.battleView);

        UIManager.Instance.ChangeState(UIState.BattlePath);

        print($" Battle started ({Time.timeSinceLevelLoad - LevelManager.LevelStartTime} sec)");
    }

    private void SetBlocksHeightIncrement(int startOrderIndex, HeightIncrementData incrementData)
    {
        if (startOrderIndex < blockPairs.Count - incrementData.transitionLength - 1)
        {
            float transitionLength = incrementData.transitionLength;

            for (int i = startOrderIndex; i < blockPairs.Count; i++)
            {
                blockPairs[i].SetHeight(blockPairs[i].Height + incrementData.heightIncrement * Mathf.Clamp01((i - startOrderIndex) / transitionLength));
            }
        }
    }

    private void AlignBlocks(BlockPair referenceBlockPair, int lineIndex, int fromIndex, int toIndex)
    {
        for (int i = Mathf.Clamp(fromIndex, 0, blockPairs.Count - 1); i <= Mathf.Clamp(toIndex, 0, blockPairs.Count - 1); i++)
        {
            if (lineIndex == 0 || lineIndex == 1)
            {
                blockPairs[i].ceilingBlock.transform.SetCoordinate(TransformComponent.Position, Axis.Y, Space.World, referenceBlockPair.CeilBlockPosition.y);
            }

            if (lineIndex == 0 || lineIndex == -1)
            {
                blockPairs[i].groundBlock.transform.SetCoordinate(TransformComponent.Position, Axis.Y, Space.World, referenceBlockPair.FloorBlockPosition.y);
            }
        }
    }

    private void AlignBlocksForCollectible(Multicollectible collectible, int pivotBlockPairIndex)
    {
        if (collectible.RangeNumber > 0)
        {
            AlignBlocks(blockPairs[pivotBlockPairIndex], collectible.Placement == CollectiblePlacementType.Any ? 0 : (collectible.Placement == CollectiblePlacementType.Ceiling ? 1 : -1), pivotBlockPairIndex - collectible.RangeNumber, pivotBlockPairIndex + collectible.RangeNumber);
        }

        if (collectible.collectibleSettings.verticalShift != 0)
        {
            blockPairs[pivotBlockPairIndex].groundBlock.transform.position += new Vector3(0, collectible.collectibleSettings.verticalShift, 0);
        }

        blockPairs[pivotBlockPairIndex].FitCollectible();
    }

    public void UpdateLevelConfiguration(int humanballLayerIndex)
    {
        SetBlocksHeightIncrement(GetBlockPair(humanballTransform.position).orderIndex + blockSettings.heightIncrementSettings.transitionShift, blockSettings.heightIncrementSettings);

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

    public void FinishBattle()
    {
        StartCoroutine(BattleFinishingCoroutine());
    }

    public void UpdateLevelVisibility(int pivotBlockIndex)
    {
        for (int i = 0; i < blockPairs.Count; i++)
        {
            blockPairs[i].SetVisible(i >= pivotBlockIndex + levelSettings.visibilityRange.x && i <= pivotBlockIndex + levelSettings.visibilityRange.y);
        }

        if (!battlePath.gameObject.activeSelf && blockPairs[pivotBlockIndex].Position.x > battlePath.Position.x - battlePathSettings.activationDistance)
        {
            battlePath.SetActive(true);
        }
    }

    public void UpdateBattlePathVisibility(int pivotStageIndex)
    {
        for (int i = 0; i < battlePath.stages.Count; i++)
        {
            battlePath.stages[i].SetVisible(i >= Mathf.Clamp(pivotStageIndex + battlePathSettings.visibilityRange.x, 0, battlePath.stages.Count - 1) && i <= Mathf.Clamp(pivotStageIndex + battlePathSettings.visibilityRange.y, 0, battlePath.stages.Count - 1));
        }
    }

    public void MultiplyCollectibleRadiuses(float multiplier)
    {
        for (int i = 0; i < collectibles.Count; i++)
        {
            if (collectibles[i].collectibleSettings.collider.GetType() == typeof(BoxCollider))
            {
                ((BoxCollider)collectibles[i].collectibleSettings.collider).size *= multiplier;
            }

            if (collectibles[i].collectibleSettings.collider.GetType() == typeof(SphereCollider))
            {
                ((SphereCollider)collectibles[i].collectibleSettings.collider).radius *= multiplier;
            }
        }
    }

    public void FractureBlock(GameObject block, Vector3 hitPosition)
    {
        fracturedBlockPair = GetBlockPair(block.transform.position);

        if (block.transform.position.y > fracturedBlockPair.Position.y)
        {
            fracturedBlockVFX = WorldManager.environmentSettings.particles.cliffFracturePool.Eject();

            block.transform.position = new Vector3(block.transform.position.x, hitPosition.y + blockSettings.thresholdValue, block.transform.position.z);

            for (int i = 0; i < collectibles.Count; i++)
            {
                if (collectibles[i].Placement != CollectiblePlacementType.Ground && Mathf.RoundToInt(collectibles[i].transform.position.x) == Mathf.RoundToInt(block.transform.position.x))
                {
                    if (!collectibles[i].IsCollected)
                    {
                        collectibles[i].Collect();
                    }
                }
            }
        }
        else
        {
            fracturedBlockVFX = WorldManager.environmentSettings.particles.foilFracturePool.Eject();

            block.transform.position = new Vector3(block.transform.position.x, hitPosition.y - blockSettings.thresholdValue, block.transform.position.z);

            for (int i = 0; i < collectibles.Count; i++)
            {
                if (collectibles[i].Placement == CollectiblePlacementType.Ground && Mathf.RoundToInt(collectibles[i].transform.position.x) == Mathf.RoundToInt(block.transform.position.x))
                {
                    if (!collectibles[i].IsCollected)
                    {
                        collectibles[i].Collect();
                    }
                }
            }
        }

        if (block.transform.childCount > 1)
        {
            foreach (GameObject blockElement in block.transform.GetGameObjectsInChildren())
            {
                if (blockElement.layer != 7)
                {
                    blockElement.SetActive(false);
                }
            }
        }

        fracturedBlockVFX.transform.position = block.transform.position;

        fracturedBlockVFX.Play();
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

    private IEnumerator BattleFinishingCoroutine()
    {
        //BattlePath.Instance.PlayerCrew.Members[0].isImmortal = true;

        //CameraController.Instance.FocusOn(BattlePath.Instance.PlayerCrew.Members[0].transform, LevelGenerator.Instance.battlePathSettings.finishView);

        BattlePath.Instance.PlayerCrew.Stop();
        BattlePath.Instance.GuardCrew.Stop();

        Vector3 activeStageCenter = BattlePath.Instance.ActiveStage.Position + new Vector3(BattlePath.Instance.StageSize.x / 2f, 0, 0);

        //BattlePath.Instance.PlayerCrew.Members[0].FocusOn(activeStageCenter);
        //BattlePath.Instance.PlayerCrew.Members[0].PlayAnimation(HumanAnimationType.Win);

        yield return new WaitForSeconds(0.1f);

        CameraController.Instance.FocusOn(activeStageCenter, battlePathSettings.finishView);

        yield return new WaitForSeconds(battlePathSettings.finishView.translationDuration + 0.1f);

        BattlePath.Instance.ActiveStage.PullOutRewardText();

        LevelManager.Instance.OnLevelFinished(true);

        GameManager.Instance.ChangeCurrency(BattlePath.Instance.ActiveStage.Reward, true);
    }
}