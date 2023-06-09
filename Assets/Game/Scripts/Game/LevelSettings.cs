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

    public string[] GetStructureTitles()
    {
        string[] titles = new string[structures.Count];

        for (int i = 0; i < titles.Length; i++)
        {
            titles[i] = structures[i].title;
        }

        return titles;
    }

    public string[] GetLandscapeTitles()
    {
        string[] titles = new string[landscapes.Count];

        for (int i = 0; i < titles.Length; i++)
        {
            titles[i] = landscapes[i].title;
        }

        return titles;
    }
}

public struct LevelData
{
    public LevelCycleData structureData;
    public LandscapeData landscapeData;

    public int cyclesCount;
    public int blocksCount;

    public LevelData(LevelCycleData structureData, LandscapeData landscapeData)
    {
        this.structureData = structureData;
        this.landscapeData = landscapeData;

        cyclesCount = structureData.cyclesCount;
        blocksCount = 0;

        for (int i = 0; i < structureData.cycle.Count; i++)
        {
            blocksCount += structureData.cycle[i].blocksCount + 1;
        }

        blocksCount *= cyclesCount;

        blocksCount += structureData.startStep.blocksCount + structureData.endStep.blocksCount + 2;
    }
}

[System.Serializable]
public struct LevelCycleData
{
    public string title;
    public LevelStepData startStep;
    public List<LevelStepData> cycle;
    public LevelStepData endStep;
    [Space]
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
    public LevelElementType elementType;
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