using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class LevelSettings
{
    public List<LevelCycleData> structures;
    public List<LandscapeData> landscapes;

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
    public WavePatternData[] landscapePatterns;

    public int cyclesCount;
    public int blocksCount;

    public LevelData(LevelCycleData structureData, LandscapeData landscapeData)
    {
        startStep = structureData.startStep;
        cycleSteps = structureData.cycle.ToArray();
        landscapePatterns = landscapeData.patterns.ToArray();

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
}

[System.Serializable]
public struct WavePatternData
{
    [HideInInspector]
    public string title;

    public float waveHeight;
    public float waveFrequency;
}