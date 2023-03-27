using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BattlePathSettings
{
    public Transform pathContainer;
    public Transform stagesContainer;
    [Space]
    public Transform baseStageTransform;
    [Space]
    public GameObject stagePrefab;
    [Space]
    public CameraViewData battleView;
    public CameraViewData finishView;
    [Space]
    public float visibilityDistance; 
}