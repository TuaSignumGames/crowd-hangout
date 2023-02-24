using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(HumanController))]
public class HumanControllerEditor : Editor
{
    private GUICustomDrawer drawer = new GUICustomDrawer();

    private HumanController humanController;

    /*
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        humanController = (HumanController)target;

        if (humanController.poseSettings.updateLerping)
        {
            humanController.SetConfusedPose(humanController.poseSettings.confuseFactor);
        }

        if (drawer.DrawButton("Peek Selected Pose"))
        {
            switch (humanController.poseSettings.selectedPose)
            {
                case HumanPoseType.Default: humanController.poseSettings.defaultPose = humanController.PeekPose(); break;
                case HumanPoseType.FrontConfuse: humanController.poseSettings.frontConfusePose = humanController.PeekPose(); break;
                case HumanPoseType.BackConfuse: humanController.poseSettings.backConfusePose = humanController.PeekPose(); break;
            }
        }

        if (drawer.DrawButton("Add Curve Keypoints"))
        {
            humanController.poseSettings.confusingCurve.AddKey(Mathf.InverseLerp(0.5f, 10f, 10f), 1f);
            humanController.poseSettings.confusingCurve.AddKey(Mathf.InverseLerp(0.5f, 10f, 7.5f), 1f);
            humanController.poseSettings.confusingCurve.AddKey(Mathf.InverseLerp(0.5f, 10f, 5f), 1f);
            humanController.poseSettings.confusingCurve.AddKey(Mathf.InverseLerp(0.5f, 10f, 2.5f), 1f);
            humanController.poseSettings.confusingCurve.AddKey(Mathf.InverseLerp(0.5f, 10f, 0.5f), 1f);
        }

        if (drawer.DrawButton("Add Curve Keypoints"))
        {
            humanController.poseSettings.confusingCurve.AddKey(Mathf.InverseLerp(0.5f, 10f, 1f), 1f);
            humanController.poseSettings.confusingCurve.AddKey(Mathf.InverseLerp(0.5f, 10f, 1.5f), 1f);
            humanController.poseSettings.confusingCurve.AddKey(Mathf.InverseLerp(0.5f, 10f, 2f), 1f);
        }

        if (drawer.DrawButton("Reset Pose"))
        {
            humanController.SetPose(humanController.poseSettings.defaultPose);
        }
    }
    */
}