using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanballCell
{
    private GameObject gameObject;

    private HumanballLayer relatedLayer;

    private HumanController placedHuman;

    private HumanPose placedHumanPose;

    private TransformData placedHumanLocalTransformData;

    public Transform transform => gameObject.transform;
    public HumanballLayer Layer => relatedLayer;
    public HumanController Human => placedHuman;
    public HumanPose Pose => placedHumanPose;

    public bool IsAvailable => placedHuman == null;

    public HumanballCell(GameObject gameObject)
    {
        this.gameObject = gameObject;

        if (transform.childCount > 0)
        {
            placedHuman = transform.GetComponentInChildren<HumanController>();
        }
    }

    public void Reserve(HumanController human)
    {
        placedHuman = human;
    }

    public void PutHuman(HumanController human)
    {
        Debug.Log($"PutHuman({human})");

        placedHuman = human;

        placedHuman.PlaceInCell(this);

        if (placedHumanPose != null)
        {
            human.SetPose(placedHumanPose);

            placedHuman.transform.ApplyData(placedHumanLocalTransformData);
        }
        else
        {
            human.SetPose(human.poseSettings.GetConfusedPose(relatedLayer.Radius + 0.6f, ConfusedPoseType.BackConfuse));
        }
    }

    public void EjectHuman()
    {
        placedHuman.DropFromCell(Vector3.zero, Vector3.zero);

        placedHuman = null;
    }

    public void TrySavePose()
    {
        if (placedHuman)
        {
            placedHumanPose = placedHuman.PeekPose();

            placedHumanLocalTransformData = new TransformData(placedHuman.transform, Space.Self);
        }
    }

    public void TryCropPose()
    {
        if (placedHuman)
        {
            TrySavePose();

            GameObject.Destroy(placedHuman.gameObject);

            placedHuman = null;
        }
    }

    public void SetLayer(HumanballLayer layer)
    {
        relatedLayer = layer;
    }
}
