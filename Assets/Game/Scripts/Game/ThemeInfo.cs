using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ThemeInfo
{
    public string title;
    [Space]
    public GameObject ceilingBlockPrefab;
    public GameObject groundBlockPrefab;
    [Space]
    public GameObject sandBlockPrefab;
    public GameObject lavaBlockPrefab;
    [Space]
    public ForceAreaPrefabs forceAreaPrefabs;
    [Space]
    public GameObject battlePathBaseStage;
    public GameObject battlePathStagePrefab;
    [Space]
    public bool useBackground;
    public BackgroundSettings backgroundSettings;
    [Space]
    public Material skyboxMaterial;

    public GameObject GetBlockPrefab(BlockType type)
    {
        switch (type)
        {
            case BlockType.Ceiling: return ceilingBlockPrefab;
            case BlockType.Ground: return groundBlockPrefab;
            case BlockType.Sand: return sandBlockPrefab;
            case BlockType.Lava: return lavaBlockPrefab;
        }

        return null;
    }
}
