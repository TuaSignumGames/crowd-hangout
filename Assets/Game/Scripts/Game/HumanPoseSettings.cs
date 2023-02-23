using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO Check pose lerping 

public enum HumanPoseType { Default, FrontConfuse, BackConfuse }

[System.Serializable]
public class HumanPoseSettings
{
    public HumanPose defaultPose;
    [Space]
    public HumanPose frontConfusePose;
    public HumanPose backConfusePose;
    [Range(-1f, 1f)]
    public float confuseFactor;
    public bool updateLerping;

    //[Space]
    //public HumanPoseType selectedPose;
}