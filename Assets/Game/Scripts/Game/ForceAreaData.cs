using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// - ApplyForce() to the Humanball from HumanController, find 'forceMagnitude' via EnvironmentSettings by tag 
// - Refactor Block[Water] as 'ForceArea', replace it in Environment folder and reasseble it to face Forward in direction of force should be applied 

[System.Serializable]
public struct ForceAreaData
{
    public string tag;
    public float forceMagnitude;
}
