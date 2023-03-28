using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelSettings
{
    public List<LevelCycleData> structures;
    public List<LandscapeData> landscapes;
    [Space]
    public Vector2Int visibilityRange;

    public LevelData GetConfiguration()
    {
        return new LevelData(structures.GetRandom(), landscapes.GetRandom());
    }

    public LevelData GetConfiguration(int structureIndex = 0, int landscapeIndex = 0)
    {
        return new LevelData(structures[structureIndex], landscapes[landscapeIndex]);
    }
}

public struct LevelData
{
    public LevelStepData startStep;
    public LevelStepData[] cycleSteps;

    public LandscapeData landscapeData;

    public int cyclesCount;
    public int blocksCount;

    public LevelData(LevelCycleData structureData, LandscapeData landscapeData)
    {
        startStep = structureData.startStep;
        cycleSteps = structureData.cycle.ToArray();

        this.landscapeData = landscapeData;

        cyclesCount = structureData.cyclesCount;
        blocksCount = 0;

        for (int i = 0; i < cycleSteps.Length; i++)
        {
            blocksCount += cycleSteps[i].blocksCount + 1;
        }

        blocksCount *= cyclesCount;
        blocksCount += structureData.startStep.blocksCount + 1;
    }
}

[System.Serializable]
public struct LevelCycleData
{
    public string title;
    public LevelStepData startStep;
    public List<LevelStepData> cycle;
    public int cyclesCount;

    private int blocksCounter;

    public int GetLength()
    {
        blocksCounter = 0;

        for (int i = 0; i < cycle.Count; i++)
        {
            blocksCounter += cycle[i].blocksCount + 1;
        }

        blocksCounter *= cyclesCount;

        blocksCounter = startStep.blocksCount + 1;

        title = "A";

        return blocksCounter;
    }
}

[System.Serializable]
public struct LevelStepData
{
    public int blocksCount;
    public CollectibleType collectiblePointType;
}

[System.Serializable]
public struct LandscapeData
{
    public string title;
    public List<WavePatternData> patterns;
    [Space]
    public Vector2 caveHeightRange;
}

[System.Serializable]
public struct WavePatternData
{
    [HideInInspector]
    public string title;

    public float waveHeight;
    public float waveFrequency;
}