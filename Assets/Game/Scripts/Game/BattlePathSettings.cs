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
    public List<GameObject> guardPrefabs;
    [Space]
    public CameraViewData battleView;
    public CameraViewData finishView;
}