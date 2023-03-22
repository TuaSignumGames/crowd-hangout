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
    public LevelStepData[] cycleSteps;
    public WavePatternData[] landscapePatterns;

    public int cyclesCount;

    public LevelData(LevelCycleData structureData, LandscapeData landscapeData)
    {
        cycleSteps = structureData.cycle.ToArray();
        landscapePatterns = landscapeData.patterns.ToArray();

        cyclesCount = structureData.cyclesCount;
    }
}

[System.Serializable]
public struct LevelCycleData
{
    public string title;
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