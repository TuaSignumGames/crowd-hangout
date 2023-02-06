using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanPose
{
    public TransformData bodyTransformData;
    public TransformData[] boneTransformDatas;

    public HumanPose(Transform bodyTransform, IList<HumanBone> bones)
    {
        bodyTransformData = new TransformData(bodyTransform, Space.Self);

        boneTransformDatas = new TransformData[bones.Count];

        for (int i = 0; i < boneTransformDatas.Length; i++)
        {
            boneTransformDatas[i] = new TransformData(bones[i].transform, Space.Self);
        }
    }
}
