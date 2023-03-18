using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanballCell
{
    private GameObject gameObject;

    private HumanballLayer relatedLayer;

    private HumanController placedHuman;

    private HumanPose placedHumanPose;

    private PulseEvaluator pulseEvaluator;

    private TransformData placedHumanLocalTransformData;

    public Transform transform => gameObject.transform;
    public HumanballLayer Layer => relatedLayer;
    public HumanController Human => placedHuman;
    public HumanPose Pose => placedHumanPose;

    public bool IsAvailable => placedHuman == null;

    public HumanballCell(GameObject gameObject)
    {
        this.gameObject = gameObject;

        pulseEvaluator = new PulseEvaluator(transform, 0.2f, 5f);
        pulseEvaluator.pulseRatio = new Vector3(0, 0, 1f);

        if (transform.childCount > 0)
        {
            placedHuman = transform.GetComponentInChildren<HumanController>();
        }
    }

    public void Update()
    {
        if (placedHuman != null)
        {
            pulseEvaluator.Update();
        }
    }

    public void Reserve(HumanController human)
    {
        placedHuman = human;
    }

    public void PutHuman(HumanController human)
    {
        placedHuman = human;

        placedHuman.PlaceInCell(this);

        if (placedHumanPose != null)
        {
            human.SetPose(placedHumanPose);

            placedHuman.transform.SetData(placedHumanLocalTransformData);
        }
        else
        {
            human.SetPose(human.poseSettings.GetConfusedPose(relatedLayer.Radius + 0.6f, ConfusedPoseType.BackConfuse));
        }

        pulseEvaluator.Click(2.5f);
    }

    public void EjectHuman()
    {
        placedHuman.Drop(Vector3.zero, Vector3.zero, true);

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
