using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO Improve AI 

public enum HumanBehaviourType { Assault, Defence }

public class HumanAI
{
    private HumanController hostHuman;
    private HumanController targetHuman;

    private List<HumanController> enemies;

    private HumanBehaviourType behaviourMode;

    private Vector3 targetPosition;

    private bool isTargetPositionAvailable;

    public HumanAI(HumanController humanController)
    {
        hostHuman = humanController;
    }

    public void Update()
    {


        /*
        if (targetHuman)
        {
            if (!hostHuman.Attack(targetHuman))
            {
                hostHuman.MoveTo(targetHuman.transform.position);
            }
        }
        else if (isTargetPositionAvailable)
        {
            hostHuman.MoveTo(targetPosition);
        }
        */
    }

    public void Assault(HumanController human)
    {
        behaviourMode = HumanBehaviourType.Assault;

        targetHuman = human;
    }

    public void Defend(Vector3 position)
    {
        behaviourMode = HumanBehaviourType.Defence;

        targetPosition = position;

        isTargetPositionAvailable = true;
    }
}