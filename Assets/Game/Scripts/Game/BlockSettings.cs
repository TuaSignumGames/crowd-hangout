using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BlockSettings
{
    public Transform blocksContainer;
    public GameObject[] ceilBlockPrefabs;
    public GameObject[] floorBlockPrefabs;
    public float blockLength;
    [Space]
    public Vector2 caveHeightRange;
    public float thresholdValue;
    [Space]
    public HeightIncrementData heightIncrementSettings;
}

[System.Serializable]
public struct HeightIncrementData
{
    public float heightIncrement;
    [Space]
    public int transitionShift;
    public int transitionLength;
}