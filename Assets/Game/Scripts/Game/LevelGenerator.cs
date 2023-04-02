using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO
//
// -> [ Process upgrades immediately or on new level? ] <- 
//
//  Scope
//  -
//  - Building colors (+2) 
//
//  Polishing

public class LevelGenerator : MonoBehaviour
{
    public static LevelGenerator Instance;

    public BlockSettings blockSettings;
    public CollectibleSettings collectibleSettings;
    public BattlePathSettings battlePathSettings;
    [Space]
    public LevelSettings levelSettings;

    private LevelData levelData;

    private ProgressionStageInfo stageInfo;

    private List<BlockPair> blockPairs;
    private List<Collectible> collectibles;

    private BattlePath battlePath;

    private Transform humanballTransform;

    private BlockPair newBlockPair;
    private BattlePathStage newBattlePathStage;

    private CollectibleType[] collectibleMap;

    private Multicollectible multicollectibleInstance;

    private HumanMulticollectible humanCollectiblePrefab;
    private HumanMulticollectible humanCollectibleInstance;

    private WeaponMulticollectible weaponCollectiblePrefab;
    private WeaponMulticollectible weaponCollectibleInstance;

    private GameObject newBlockPairContainer;

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

    private bool isCavePassed;

    private bool isLevelGenerated;

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
        stageInfo = WorldManager.gameProgressionSettings.GetStageOf(LevelManager.LevelNumber);

        int structureIndex = stageInfo.availableStructureIndices.GetRandom();
        int landscapeIndex = stageInfo.availableLandscapeIndices.GetRandom();

        levelData = levelSettings.GetConfiguration(structureIndex, landscapeIndex);

        humanPower = WorldManager.GetWeaponPower(0);
        levelPower = humanPower;

        GenerateBlocks(levelData.landscapeData, levelData.blocksCount);

        GenerateComposition();

        levelLength = blockPairs.GetLast().Position.x - blockPairs[3].Position.x;

        print($" -- Level {LevelManager.LevelNumber} Generated: \n\n - Stage: {stageInfo.title} (structure: '{levelSettings.structures[structureIndex].title}', landscape: '{levelSettings.landscapes[landscapeIndex].title}')");

        isLevelGenerated = true;
    }

    public void GenerateComposition()
    {
        if (isLevelGenerated)
        {
            RemoveCollectibles();
            RemoveBattlePath();
        }

        PlaceCollectibles(levelData.startStep, levelData.endStep, levelData.cycleSteps, levelData.cyclesCount);
        GenerateBattlePath(Mathf.Clamp(GameManager.CryticalStageIndex + 6, 10, int.MaxValue), GameManager.CryticalStageIndex);

        print($" -- Compositions Generated: \n Population: {GameManager.PopulationValue} / Top weapon ID: {WorldManager.GetWeaponID(GameManager.TopWeaponPower)} \n\n - Multicollectibles: {humanCollectiblesCount + weaponCollectiblesCount} (Human: {humanCollectiblesCount}[{totalHumansCount}], Weapon: {weaponCollectiblesCount}[{totalWeaponsCount}]) \n - Level power: {levelPower}");
    }

    public void GenerateFromEditor(bool collectibles, bool battlePath)
    {
        levelData = levelSettings.GetConfiguration(0, 0);

        GenerateBlocks(levelData.landscapeData, levelData.blocksCount);

        if (collectibles)
        {
            PlaceCollectibles(levelData.startStep, levelData.endStep, levelData.cycleSteps, levelData.cyclesCount);
        }

        if (battlePath)
        {
            GenerateBattlePath(10, 0);
        }
    }

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

                //UpdateLevelVisibility(GetBlockPair(PlayerController.Humanball.Transform.position).OrderIndex);

                UIProgressBar.Instance.SetProgressValue(Mathf.Clamp01(PlayerController.Humanball.Transform.position.x / levelLength));
            }
        }
    }

    public void GenerateBlocks(LandscapeData landscapeData, int blocksCount)
    {
        if (blockSettings.blocksContainer.childCount > 0)
        {
            blockSettings.blocksContainer.RemoveChildrenImmediate();

            blockSettings.blocksContainer.position = Vector3.zero;
        }

        blockPairs = new List<BlockPair>();

        perlinNoiseOrigin = new Vector2(Random.Range(-500f, 500f), Random.Range(-500f, 500f));

        offsetMap = new List<float>(new float[blocksCount]);

        for (int patternIndex = 0; patternIndex < landscapeData.patterns.Count; patternIndex++)
        {
            for (int blockIndex = 0; blockIndex < blocksCount; blockIndex++)
            {
                perlinValue = Mathf.PerlinNoise(perlinNoiseOrigin.x + (float)blockIndex * landscapeData.patterns[patternIndex].waveFrequency, perlinNoiseOrigin.y);

                offsetMap[blockIndex] += (perlinValue - 0.5f) * 2f * landscapeData.patterns[patternIndex].waveHeight;
            }
        }

        for (int i = 0; i < offsetMap.Count; i++)
        {
            offsetMap[i] = Mathf.Round(offsetMap[i] / blockSettings.thresholdValue) * blockSettings.thresholdValue;

            InstantiateBlockPair(new Vector3(i * blockSettings.blockLength, offsetMap[i]), landscapeData.caveHeightRange, i);
        }

        blockSettings.blocksContainer.position = new Vector3(-blockSettings.blockLength * 3, -offsetMap[3], 0);
    }

    private void PlaceCollectibles(LevelStepData startStep, LevelStepData endStep, LevelStepData[] cycleSteps, int cyclesCount)
    {
        collectibles = new List<Collectible>();

        collectibleMap = new CollectibleType[blockPairs.Count];

        collectibleMap[previousCollectiblePlacementIndex = startStep.blocksCount] = startStep.collectiblePointType;

        for (int i = 0; i < cyclesCount; i++)
        {
            for (int j = 0; j < cycleSteps.Length; j++)
            {
                collectibleMap[previousCollectiblePlacementIndex + cycleSteps[j].blocksCount] = cycleSteps[j].collectiblePointType;

                previousCollectiblePlacementIndex += cycleSteps[j].blocksCount;
            }
        }

        collectibleMap[previousCollectiblePlacementIndex + endStep.blocksCount] = endStep.collectiblePointType;

        for (int i = 0; i < collectibleMap.Length; i++)
        {
            if (collectibleMap[i] != CollectibleType.None)
            {
                if (collectibleMap[i] == CollectibleType.Human)
                {
                    humanCollectiblesCount++;
                }
                if (collectibleMap[i] == CollectibleType.Weapon)
                {
                    weaponCollectiblesCount++;
                }
            }
        }

        PlaceHumanCollectibles();
        PlaceWeaponCollectibles();
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

        for (int i = 0; i < stagesCount; i++)
        {
            battleBathStageInfo = WorldManager.battlePathProgressionSettings.GetStageInfo(i);

            newBattlePathStage = new BattlePathStage(Instantiate(battlePathSettings.stagePrefab, battlePathSettings.stagesContainer), i % 2 == 0);

            newBattlePathStage.Initialize(battlePath.Position + new Vector3(battlePathSettings.baseStageTransform.localScale.x + i * newBattlePathStage.Size.x, 0, 0), battleBathStageInfo.reward, i);
            newBattlePathStage.GenerateGuard(battleBathStageInfo.guardiansCount, levelPower * ((i + 1) / (float)(cryticalStageIndex + 1)));

            battlePath.stages.Add(newBattlePathStage);

            //print($" BPStage generated - guard: {newBattlePathStage.GuardCrew.MembersCount} / reward: {newBattlePathStage.Reward}");
        }

        battlePath.SetActive(false);
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

            for (int i = 0; i < collectibleMap.Length; i++)
            {
                if (collectibleMap[i] == CollectibleType.Human)
                {
                    humanCollectiblePortionFactors.Add(collectibleSettings.populationCurve.Evaluate(i / (float)collectibleMap.Length));

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
            if (collectibleMap[i] == CollectibleType.Human && humanPortionQueue.Peek() > 0)
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
                if (collectibleMap[i] == CollectibleType.Weapon)
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

    private BlockPair InstantiateBlockPair(Vector3 origin, Vector2 heightRange, int orderIndex)
    {
        newBlockPairContainer = new GameObject("BlockPair");

        newBlockPairContainer.transform.position = origin;
        newBlockPairContainer.transform.SetParent(blockSettings.blocksContainer);

        newBlockPair = new BlockPair(Instantiate(blockSettings.ceilBlockPrefabs.GetRandom()), Instantiate(blockSettings.floorBlockPrefabs.GetRandom()), newBlockPairContainer, orderIndex, blockSettings.thresholdValue);

        newBlockPair.SetHeight(Random.Range(heightRange.x, heightRange.y));

        blockPairs.Add(newBlockPair);

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
                blockPairs[i].ceilBlock.transform.SetCoordinate(TransformComponent.Position, Axis.Y, Space.World, referenceBlockPair.CeilBlockPosition.y);
            }

            if (lineIndex == 0 || lineIndex == -1)
            {
                blockPairs[i].floorBlock.transform.SetCoordinate(TransformComponent.Position, Axis.Y, Space.World, referenceBlockPair.FloorBlockPosition.y);
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
            blockPairs[pivotBlockPairIndex].floorBlock.transform.position += new Vector3(0, collectible.collectibleSettings.verticalShift, 0);
        }

        blockPairs[pivotBlockPairIndex].FitCollectible();
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