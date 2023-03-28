using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO
//
// -> [ MAR 26 - POLISHING DAY, but not only ] <- LOL 
//
//  Upgrades
//  -
//  - Implement upgrading (Upgrade tables, BattlePath) 
//
//  Scope
//  -
//  - Difficulty progression (10-20 levels, Cave height, Landscape complexity, Collectible radius) 
//
//  - DropToBattle() actually present humans (Check 'usedCells')
//  - Multicollectible blocks fitting (Building)
//  - Hide rope on touch not present
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

    private List<BlockPair> blockPairs;

    private BattlePath battlePath;

    private Transform humanballTransform;

    private BlockPair newBlockPair;
    private BattlePathStage newBattlePathStage;

    private Multicollectible multicollectibleInstance;

    private HumanMulticollectible humanCollectiblePrefab;
    private HumanMulticollectible humanCollectibleInstance;

    private WeaponMulticollectible weaponCollectiblePrefab;
    private WeaponMulticollectible weaponCollectibleInstance;

    private WeaponMulticollectible weaponCollectible;

    private GameObject newBlockPairContainer;

    private Vector2 perlinNoiseOrigin;

    private List<float> offsetMap;

    private float progressValue;
    private float perlinValue;

    private float humanPower;
    private float levelPower;

    private float levelLength;

    private int availableBlockPairIndex;

    private int localHumansCount;
    private int totalHumansCount;

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
        levelData = levelSettings.GetConfiguration();

        humanPower = WorldManager.GetWeaponPower(0);
        levelPower = humanPower;

        print($" Human Power: {humanPower}");

        GenerateBlocks(levelData.landscapePatterns, levelData.blocksCount);
        PlaceCollectibles(levelData.startStep, levelData.cycleSteps, levelData.cyclesCount);
        GenerateBattlePath(3, 1);

        levelLength = blockPairs.GetLast().Position.x - blockPairs[3].Position.x;

        print($" -- Level Power: {levelPower}");

        isLevelGenerated = true;
    }

    public void GenerateFromEditor(bool collectibles, bool battlePath)
    {
        levelData = levelSettings.GetConfiguration();

        GenerateBlocks(levelData.landscapePatterns, levelData.blocksCount);

        if (collectibles)
        {
            PlaceCollectibles(levelData.startStep, levelData.cycleSteps, levelData.cyclesCount);
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
            }
            else
            {
                CheckForBattlePath();

                //UpdateVisibility(GetBlockPair(PlayerController.Humanball.Transform.position).OrderIndex);

                UIProgressBar.Instance.SetProgressValue(Mathf.Clamp01(PlayerController.Humanball.Transform.position.x / levelLength));
            }
        }
    }

    public void GenerateBlocks(WavePatternData[] landscapePatterns, int blocksCount)
    {
        if (blockSettings.blocksContainer.childCount > 0)
        {
            blockSettings.blocksContainer.RemoveChildrenImmediate();

            blockSettings.blocksContainer.position = Vector3.zero;
        }

        blockPairs = new List<BlockPair>();

        perlinNoiseOrigin = new Vector2(Random.Range(-500f, 500f), Random.Range(-500f, 500f));

        offsetMap = new List<float>(new float[blocksCount]);

        for (int patternIndex = 0; patternIndex < landscapePatterns.Length; patternIndex++)
        {
            for (int blockIndex = 0; blockIndex < blocksCount; blockIndex++)
            {
                perlinValue = Mathf.PerlinNoise(perlinNoiseOrigin.x + (float)blockIndex * landscapePatterns[patternIndex].waveFrequency, perlinNoiseOrigin.y);

                print(perlinValue);

                offsetMap[blockIndex] += (perlinValue - 0.5f) * 2f * landscapePatterns[patternIndex].waveHeight;
            }
        }

        for (int i = 0; i < offsetMap.Count; i++)
        {
            offsetMap[i] = Mathf.Round(offsetMap[i] / blockSettings.thresholdValue) * blockSettings.thresholdValue;

            InstantiateBlockPair(new Vector3(i * blockSettings.blockLength, offsetMap[i]), i);
        }

        blockSettings.blocksContainer.position = new Vector3(-blockSettings.blockLength * 3f, -offsetMap[3], 0);
    }

    private void PlaceCollectibles(LevelStepData startStep, LevelStepData[] cycleSteps, int cyclesCount)
    {
        CollectibleType[] collectibleMap = new CollectibleType[blockPairs.Count];

        int previousCollectiblePlacementIndex = 0;

        collectibleMap[previousCollectiblePlacementIndex = startStep.blocksCount] = startStep.collectiblePointType;

        for (int i = 0; i < cyclesCount; i++)
        {
            for (int j = 0; j < cycleSteps.Length; j++)
            {
                collectibleMap[previousCollectiblePlacementIndex + cycleSteps[j].blocksCount] = cycleSteps[j].collectiblePointType;

                previousCollectiblePlacementIndex += cycleSteps[j].blocksCount;
            }
        }

        for (int i = 0; i < blockPairs.Count; i++)
        {
            progressValue = i / (float)blockPairs.Count;

            if (collectibleMap[i] != CollectibleType.None)
            {
                switch (collectibleMap[i])
                {
                    case CollectibleType.Human: PlaceHumanCollectible(blockPairs[i], Random.Range((int)Mathf.Lerp(1, 16, progressValue), (int)Mathf.Lerp(5, 26, progressValue))); break;
                    case CollectibleType.Weapon: PlaceWeaponCollectible(blockPairs[i], 10, Random.Range((int)Mathf.Lerp(3, 8, progressValue), (int)Mathf.Lerp(5, 15, progressValue))); break;
                }

                if (multicollectibleInstance.RangeNumber > 0)
                {
                    AlignBlocksForCollectible(multicollectibleInstance, i);
                }
            }
        }
    }

    private void GenerateBattlePath(int stagesCount, int cryticalStageIndex)
    {
        battlePathSettings.pathContainer.gameObject.SetActive(true);

        if (battlePathSettings.stagesContainer.childCount > 0)
        {
            battlePathSettings.stagesContainer.RemoveChildrenImmediate();
        }

        battlePath = new BattlePath(battlePathSettings.pathContainer.gameObject);

        battlePath.transform.position = blockPairs.GetLast().FloorBlockPosition + new Vector3(blockSettings.blockLength / 2f, 0, 0);

        for (int i = 0; i < stagesCount; i++)
        {
            newBattlePathStage = new BattlePathStage(Instantiate(battlePathSettings.stagePrefab, battlePathSettings.stagesContainer), i % 2 == 0);

            newBattlePathStage.Initialize(battlePath.Position + new Vector3(battlePathSettings.baseStageTransform.localScale.x + i * newBattlePathStage.Size.x, 0, 0), 100f + i * 100f);
            newBattlePathStage.GenerateGuard(6 * (i + 1), levelPower * ((i + 1) / (float)(cryticalStageIndex + 1)));

            battlePath.stages.Add(newBattlePathStage);
        }

        battlePath.SetActive(false);
    }

    private void PlaceHumanCollectible(BlockPair blockPair, int humansCount)
    {
        humanCollectiblePrefab = collectibleSettings.humanCollectibles.GetRandom().prefab;

        humanCollectibleInstance = Instantiate(humanCollectiblePrefab, blockPair.container.transform);
        humanCollectibleInstance.Initialize(humansCount);

        blockPair.AddCollectible(humanCollectibleInstance);

        totalHumansCount += humansCount;
        levelPower += humanPower * humansCount;

        multicollectibleInstance = humanCollectibleInstance;
    }

    private void PlaceWeaponCollectible(BlockPair blockPair, int weaponID, int weaponsCount)
    {
        weaponCollectiblePrefab = collectibleSettings.weaponCollectibles.GetRandom();

        weaponCollectibleInstance = Instantiate(weaponCollectiblePrefab, blockPair.container.transform);
        weaponCollectibleInstance.Initialize(weaponID, weaponsCount);

        blockPair.AddCollectible(weaponCollectibleInstance);

        levelPower += (WorldManager.GetWeaponPower(weaponID) - humanPower) * weaponsCount;

        multicollectibleInstance = weaponCollectibleInstance;
    }

    private BlockPair InstantiateBlockPair(Vector3 origin, int orderIndex)
    {
        newBlockPairContainer = new GameObject("BlockPair");

        newBlockPairContainer.transform.position = origin;
        newBlockPairContainer.transform.SetParent(blockSettings.blocksContainer);

        newBlockPair = new BlockPair(Instantiate(blockSettings.ceilBlockPrefabs.GetRandom()), Instantiate(blockSettings.floorBlockPrefabs.GetRandom()), newBlockPairContainer, orderIndex, blockSettings.thresholdValue);

        newBlockPair.SetHeight(Random.Range(blockSettings.caveHeightRange.x, blockSettings.caveHeightRange.y));

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

    public void FinishBattle()
    {
        StartCoroutine(BattleFinishingCoroutine());
    }

    public void UpdateVisibility(int pivotBlockIndex)
    {
        for (int i = 0; i < blockPairs.Count; i++)
        {
            blockPairs[i].SetVisible(i >= pivotBlockIndex + levelSettings.visibilityRange.x && i <= pivotBlockIndex + levelSettings.visibilityRange.y);
        }

        if (!battlePath.gameObject.activeSelf && blockPairs[pivotBlockIndex].Position.x > battlePath.Position.x - battlePathSettings.visibilityDistance)
        {
            battlePath.SetActive(true);
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
        BattlePath.Instance.PlayerCrew.Members[0].isImmortal = true;

        //CameraController.Instance.FocusOn(BattlePath.Instance.PlayerCrew.Members[0].transform, LevelGenerator.Instance.battlePathSettings.finishView);

        BattlePath.Instance.PlayerCrew.Stop();
        BattlePath.Instance.GuardCrew.Stop();

        Vector3 activeStageCenter = BattlePath.Instance.ActiveStage.Position + new Vector3(BattlePath.Instance.StageSize.x / 2f, 0, 0);

        BattlePath.Instance.PlayerCrew.Members[0].FocusOn(activeStageCenter);
        BattlePath.Instance.PlayerCrew.Members[0].PlayAnimation(HumanAnimationType.Win);

        yield return new WaitForSeconds(1f);

        CameraController.Instance.FocusOn(activeStageCenter, battlePathSettings.finishView);

        yield return new WaitForSeconds(battlePathSettings.finishView.translationDuration + 0.1f);

        BattlePath.Instance.ActiveStage.PullOutRewardText();

        LevelManager.Instance.OnLevelFinished(true);

        GameManager.Instance.ChangeCurrency(BattlePath.Instance.ActiveStage.Reward, true);
    }
}