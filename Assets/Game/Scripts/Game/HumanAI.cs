using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO Improve AI 

public enum HumanBehaviourType { Assault, Defence }

public class HumanAI
{
    private HumanController hostHuman;
    private HumanController targetHuman;

    private List<HumanController> rivals;
    private List<HumanController> activeRivals;

    private HumanController[] humansToCalculate;

    private HumanController closestHuman;

    private HumanBehaviourType behaviourMode;

    private Vector3 targetPosition;

    private float minSqrDistance;
    private float actualSqrDistance;

    private bool isTargetPositionAvailable;

    public HumanAI(HumanController humanController)
    {
        hostHuman = humanController;

        rivals = new List<HumanController>();
    }

    public void Update()
    {
        if (targetHuman && targetHuman.IsAlive)
        {
            if (hostHuman.Weapon.IsTargetReachable(targetHuman.transform.position))
            {
                hostHuman.Attack(targetHuman);
            }
            else
            {
                if (behaviourMode == HumanBehaviourType.Assault)
                {
                    hostHuman.MoveTo(targetHuman.transform.position);
                }
                else
                {
                    hostHuman.FocusOn(targetHuman.transform.position);
                }
            }
        }
        else
        {
            if (rivals?.Count > 0)
            {
                activeRivals = GetAliveHumans(rivals);

                if (activeRivals.Count > 0)
                {
                    targetHuman = activeRivals.GetRandom();
                }
                else
                {
                    hostHuman.Stop();
                }
            }

            if (isTargetPositionAvailable)
            {
                if (!hostHuman.MoveTo(targetPosition))
                {
                    isTargetPositionAvailable = false;
                }
            }
        }
    }

    public void Assault()
    {
        behaviourMode = HumanBehaviourType.Assault;
    }

    public void Defend(Vector3 position)
    {
        behaviourMode = HumanBehaviourType.Defence;

        targetPosition = position;

        isTargetPositionAvailable = true;
    }

    public void SetEnemy(HumanController human)
    {
        rivals.Add(human);

        targetHuman = human;
    }

    public void AddRivals(IList<HumanController> humans)
    {
        rivals.AddRange(humans);
    }

    private HumanController GetClosestHuman(List<HumanController> humans, bool isAlive = true)
    {
        humansToCalculate = humans.ToArray();

        minSqrDistance = float.MaxValue;

        for (int i = 0; i < humansToCalculate.Length; i++)
        {
            actualSqrDistance = (humansToCalculate[i].transform.position - hostHuman.transform.position).GetPlanarSqrMagnitude(Axis.Y);

            if (actualSqrDistance < minSqrDistance)
            {
                closestHuman = humansToCalculate[i];

                minSqrDistance = actualSqrDistance;
            }
        }

        return closestHuman;
    }

    private List<HumanController> GetAliveHumans(List<HumanController> humans)
    {
        humansToCalculate = humans.ToArray();

        activeRivals = new List<HumanController>();

        for (int i = 0; i < humansToCalculate.Length; i++)
        {
            if (humansToCalculate[i].IsAlive)
            {
                activeRivals.Add(humansToCalculate[i]);
            }
        }

        return activeRivals;
    }
}