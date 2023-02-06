using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanballCell
{
    private GameObject gameObject;

    private HumanController placedHuman;

    private HumanPose placedHumanPose;

    public Transform transform => gameObject.transform;

    public HumanController Human => placedHuman;

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

        placedHuman.transform.SetParent(transform);

        placedHuman.transform.localPosition = Vector3.zero;
        placedHuman.transform.forward = transform.forward;

        Debug.Log(placedHumanPose);

        if (placedHumanPose != null)
        {
            placedHuman.ApplyPose(placedHumanPose);
        }
    }

    public void EjectHuman()
    {
        placedHuman.transform.SetParent(null);

        placedHuman = null;
    }

    public void CropPose()
    {
        if (placedHuman)
        {
            placedHumanPose = placedHuman.actualPose;

            GameObject.Destroy(placedHuman.gameObject);

            placedHuman = null;
        }
    }
}
