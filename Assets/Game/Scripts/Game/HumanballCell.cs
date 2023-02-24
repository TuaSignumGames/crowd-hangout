using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanballCell
{
    private GameObject gameObject;

    private HumanballLayer relatedLayer;

    private HumanController placedHuman;
    private HumanPose placedHumanPose;

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

    public void PutHuman(HumanController human)
    {
        placedHuman = human;

        placedHuman.PlaceInCell(this);
    }

    public void EjectHuman()
    {
        placedHuman.DropFromCell(Vector3.zero);

        placedHuman = null;
    }

    public void TrySavePose()
    {
        if (placedHuman)
        {
            placedHumanPose = placedHuman.PeekPose();
        }
    }

    public void TryCropPose()
    {
        if (placedHuman)
        {
            placedHumanPose = placedHuman.PeekPose();

            GameObject.Destroy(placedHuman.gameObject);

            placedHuman = null;
        }
    }

    public void SetLayer(HumanballLayer layer)
    {
        relatedLayer = layer;
    }
}
