using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanController : MonoBehaviour
{
    public new BoxCollider collider;
    public Animator animator;
    [Space]
    public HumanRig rigSettings;

    public HumanPose actualPose;

    private bool isFree = true;

    public void Initialize(bool isFree)
    {
        this.isFree = isFree;
    }

    public void ApplyPose(HumanPose pose)
    {
        actualPose = pose;

        transform.ApplyData(actualPose.bodyTransformData);

        for (int i = 0; i < actualPose.boneTransformDatas.Length; i++)
        {
            rigSettings.bones[i].transform.ApplyData(actualPose.boneTransformDatas[i]);
        }
    }

    public void PeekPose()
    {
        actualPose = new HumanPose(transform, rigSettings.bones);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 8)
        {
            if (isFree)
            {
                isFree = false;

                PlayerController.Instance.Ball.StickHuman(this);
            }
        }
        else
        {
            if (!isFree)
            {
                isFree = true;

                PlayerController.Instance.Ball.UnstickHuman(this);
            }
        }
    }
}