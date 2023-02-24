using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ConfusedPoseType { Default, FrontConfuse, BackConfuse }

[System.Serializable]
public class HumanPoseSettings
{
    public HumanPose defaultPose;
    [Space]
    public HumanPose frontConfusePose;
    public HumanPose backConfusePose;
    //[Range(-1f, 1f)]
    //public float confuseFactor;
    //public bool updateLerping;
    [Space]
    public AnimationCurve confusingCurve;
    public Vector2 sphereRadiusRange;

    //[Space]
    //public HumanPoseType selectedPose;

    public HumanPose GetConfusedPose(float sphereRadius, ConfusedPoseType poseType)
    {
        switch (poseType)
        {
            case ConfusedPoseType.FrontConfuse: return GetConfusedPose(GetEvaluatedConfusingValue(sphereRadius, poseType));
            case ConfusedPoseType.BackConfuse: return GetConfusedPose(GetEvaluatedConfusingValue(sphereRadius, poseType));

            default: return defaultPose;
        }
    }

    private HumanPose GetConfusedPose(float confuseFactor)
    {
        if (confuseFactor == -1f || confuseFactor == 0 || confuseFactor == 1f)
        {
            return confuseFactor == 0 ? defaultPose : (confuseFactor == 1f ? frontConfusePose : backConfusePose);
        }
        else
        {
            return HumanPose.Lerp(defaultPose, confuseFactor > 0 ? frontConfusePose : backConfusePose, Mathf.Abs(confuseFactor));
        }
    }

    private float GetEvaluatedConfusingValue(float sphereRadius, ConfusedPoseType poseType)
    {
        return confusingCurve.Evaluate(Mathf.InverseLerp(sphereRadiusRange.x, sphereRadiusRange.y, sphereRadius)) * (poseType == ConfusedPoseType.FrontConfuse ? 1f : -1f);
    }
}