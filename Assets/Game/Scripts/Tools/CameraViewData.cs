using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct CameraViewData
{
    public Vector3 position;
    public Space translationSpace;
    [Space]
    public Vector3 eulerAngles;
    public Space rotationSpace;
    [Space]
    public float translationDuration;
    public float rotationDuration;
}
