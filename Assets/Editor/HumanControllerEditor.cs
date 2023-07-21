using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(HumanController))]
public class HumanControllerEditor : Editor
{
    private GUICustomDrawer drawer = new GUICustomDrawer();

    private HumanController humanController;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        humanController = (HumanController)target;

        /*
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
        */

        if (drawer.DrawButton("Reset Pose"))
        {
            humanController.SetPose(humanController.poseSettings.defaultPose);
        }
    }
}