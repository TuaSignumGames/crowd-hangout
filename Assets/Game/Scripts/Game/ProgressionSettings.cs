using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ProgressionSettings
{
    public List<ProgressionStageInfo> progressionStages;

    public ProgressionStageInfo GetStageOf(int levelNumber)
    {
        for (int i = 0; i < progressionStages.Count; i++)
        {
            if (progressionStages[i].initialLevelNumber > levelNumber)
            {
                return progressionStages[i - 1];
            }
        }

        return progressionStages.GetLast();
    }
}

[System.Serializable]
public class ProgressionStageInfo
{
    [HideInInspector]
    public string title;

    public int initialLevelNumber;
    [Space]
    public List<int> availableStructureIndices;
    public List<int> availableLandscapeIndices;
}