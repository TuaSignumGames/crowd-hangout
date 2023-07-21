using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct LandscapeSegmentData
{
    public LandscapeProfileData ceilingProfile;
    public LandscapeProfileData groundProfile;
    public int segmentSize;
}

[System.Serializable]
public struct LandscapeProfileData
{
    public GameObject blockPrefab;
    public AnimationCurve profileCurve;
    public float profileAmplitude;
}