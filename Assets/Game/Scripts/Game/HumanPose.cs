using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HumanPose
{
    public TransformData[] boneTransformDatas;

    public HumanPose(IList<HumanBone> bones)
    {
        boneTransformDatas = new TransformData[bones.Count];

        for (int i = 0; i < boneTransformDatas.Length; i++)
        {
            boneTransformDatas[i] = new TransformData(bones[i].transform, Space.Self);
        }
    }

    public static HumanPose Lerp(HumanPose a, HumanPose b, float t)
    {
        HumanPose resultPose = new HumanPose(new HumanBone[a.boneTransformDatas.Length]);

        for (int i = 0; i < resultPose.boneTransformDatas.Length; i++)
        {
            resultPose.boneTransformDatas[i] = TransformData.Lerp(a.boneTransformDatas[i], b.boneTransformDatas[i], t);
        }

        return resultPose;
    }
}