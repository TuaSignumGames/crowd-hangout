using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BlockType { None, Ceiling, Ground, Sand, Lava }

[System.Serializable]
public class BlockSettings
{
    public Transform blocksContainer;
    public float blockLength;
    [Space]
    public float thresholdValue;
    [Space]
    public HeightIncrementData heightIncrementSettings;
    [Space]
    public GameObject waterBlockPrefab;
    public GameObject lavaBlockPrefab;
}

[System.Serializable]
public struct HeightIncrementData
{
    public float heightIncrement;
    [Space]
    public int transitionShift;
    public int transitionLength;
}