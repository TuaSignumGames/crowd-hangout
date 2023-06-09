using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ThemeInfo
{
    public string title;
    [Space]
    public GameObject ceilingBlockPrefab;
    public GameObject floorBlockPrefab;
    [Space]
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
}
