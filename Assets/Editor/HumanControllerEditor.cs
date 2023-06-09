using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(HumanController))]
public class HumanControllerEditor : Editor
{
    private GUICustomDrawer drawer = new GUICustomDrawer();

    private HumanController humanController;

    [MenuItem("Human/Pose/Peek")]
    static void PeekPose()
    {
        HumanController selectedHuman = null;

        foreach (GameObject gameObject in Selection.gameObjects)
        {
            selectedHuman = gameObject.GetComponent<HumanController>();

            if (selectedHuman)
            {
                selectedHuman.poseSettings.additionalPoses.Add(selectedHuman.PeekPose());
            }
        }
    }

    [MenuItem("Human/Pose/Peek From Animator")]
    static void PeekPoseFromAnimator()
    {
        HumanController selectedHuman = null;

        foreach (GameObject gameObject in Selection.gameObjects)
        {
            selectedHuman = gameObject.transform.parent.GetComponent<HumanController>();

            if (selectedHuman)
            {
                selectedHuman.poseSettings.additionalPoses.Add(selectedHuman.PeekPose());
            }
        }
    }

    [MenuItem("Human/Pose/Reset")]
    static void ResetPose()
    {
        HumanController selectedHuman = null;

        foreach (GameObject gameObject in Selection.gameObjects)
        {
            selectedHuman = gameObject.GetComponent<HumanController>();

            if (selectedHuman)
            {
                selectedHuman.SetPose(selectedHuman.poseSettings.defaultPose);
            }
        }
    }
}