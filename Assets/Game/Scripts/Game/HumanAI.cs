using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanAI
{
    private HumanController humanController;

    private HumanController targetHuman;

    private Vector3 targetPosition;

    private bool isTargetPositionAvailable;

    public HumanAI(HumanController humanController)
    {
        this.humanController = humanController;
    }

    public void Update()
    {
        if (targetHuman)
        {
            if (!humanController.Attack(targetHuman))
            {
                humanController.MoveTo(humanController.transform.position);
            }
        }
        else if (isTargetPositionAvailable)
        {
            humanController.MoveTo(targetPosition);
        }
    }

    public void SetTarget(Vector3 position)
    {
        targetPosition = position;

        isTargetPositionAvailable = true;
    }

    public void SetTarget(HumanController human)
    {
        targetHuman = human;
    }
}