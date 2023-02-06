using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HumanRig
{
    public List<HumanBone> bones;
    public Transform leftHandContainer;
    public Transform rightHandContainer;

    private HumanPose pose;

    public void RandomizePose()
    {
        for (int i = 0; i < bones.Count; i++)
        {
            bones[i].BendRandomly();
        }
    }
}
