using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanballCell
{
    private GameObject gameObject;

    private HumanController placedHuman;

    public Transform transform => gameObject.transform;

    public HumanController Human => placedHuman;

    public bool IsAvailable => placedHuman == null;

    public HumanballCell(GameObject gameObject)
    {
        this.gameObject = gameObject;

        if (transform.childCount > 0)
        {
            placedHuman = transform.GetComponentInChildren<HumanController>();

            placedHuman.Initialize(false);
        }
    }

    public void PutHuman(HumanController human)
    {
        placedHuman = human;

        //placedHuman.rigSettings.RandomizePose();
    }

    public void EjectHuman()
    {
        placedHuman = null;
    }
}
