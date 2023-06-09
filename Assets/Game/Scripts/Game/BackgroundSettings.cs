using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BackgroundSettings
{
    public GameObject blockPrefab;
    public WavePatternData landscapeSettings;
    [Space]
    public GameObject treePrefab;
    [Range(0, 100f)]
    public float treePlacementProbability;
    public int treeDistancingThreshold;
    [Space]
    public int backgroundWidth;
    public int foregroundWidth;
    [Space]
    public Vector2 centerOffset;
    [Space]
    public AnimationCurve profileCurve;
    public float heightMultiplier;
}
