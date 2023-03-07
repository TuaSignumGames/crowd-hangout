using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HumanPose
{
    public TransformData[] boneTransformDatas;

    private TransformData[] baseBoneTransformDatas;

    private bool isTransformationComplete;

    public HumanPose(IList<HumanBone> bones)
    {
        boneTransformDatas = new TransformData[bones.Count];

        for (int i = 0; i < boneTransformDatas.Length; i++)
        {
            boneTransformDatas[i] = new TransformData(bones[i].transform, Space.Self);
        }

        baseBoneTransformDatas = boneTransformDatas;
    }

    public HumanPose(IList<TransformData> boneTransformDatas)
    {
        this.boneTransformDatas = new TransformData[boneTransformDatas.Count];

        for (int i = 0; i < boneTransformDatas.Count; i++)
        {
            this.boneTransformDatas[i] = boneTransformDatas[i];
        }

        baseBoneTransformDatas = this.boneTransformDatas;
    }

    public void Transformate(HumanPose targetPose, float t)
    {
        for (int i = 0; i < boneTransformDatas.Length; i++)
        {
            boneTransformDatas[i] = TransformData.Lerp(baseBoneTransformDatas[i], targetPose.boneTransformDatas[i], t);
        }

        isTransformationComplete = t >= 1f;

        if (isTransformationComplete)
        {
            baseBoneTransformDatas = boneTransformDatas;
        }
    }

    public static HumanPose Lerp(HumanPose a, HumanPose b, float t)
    {
        HumanPose resultPose = new HumanPose(new TransformData[a.boneTransformDatas.Length]);

        for (int i = 0; i < resultPose.boneTransformDatas.Length; i++)
        {
            resultPose.boneTransformDatas[i] = TransformData.Lerp(a.boneTransformDatas[i], b.boneTransformDatas[i], t);
        }

        return resultPose;
    }
}