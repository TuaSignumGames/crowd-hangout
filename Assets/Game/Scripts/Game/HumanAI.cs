using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private bool isActive = true;

    public HumanBehaviourType BehaviourMode => behaviourMode;

    public HumanController TargetHuman => targetHuman;

    public int ActiveRivalsCount => activeRivals.Count;

    public HumanAI(HumanController humanController)
    {
        hostHuman = humanController;

        rivals = new List<HumanController>();
    }

    public void Update()
    {
        if (isActive)
        {
            if (targetHuman && targetHuman.IsAlive)
            {
                if (hostHuman.Weapon.IsTargetReachable(targetHuman.transform.position))
                {
                    hostHuman.Attack(targetHuman);
                }
                else
                {
                    if (behaviourMode == HumanBehaviourType.Assault && hostHuman.components.animator.enabled)
                    {
                        hostHuman.MoveTo(targetHuman.transform.position);
                    }
                    else if (hostHuman.IsFree)
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
        else
        {
            hostHuman.Stop();
        }
    }

    public void Assault()
    {
        behaviourMode = HumanBehaviourType.Assault;

        isActive = true;
    }

    public void Defend(Vector3 position)
    {
        behaviourMode = HumanBehaviourType.Defence;

        targetPosition = position;

        isTargetPositionAvailable = true;

        isActive = true;
    }

    public void Defend()
    {
        behaviourMode = HumanBehaviourType.Defence;

        targetPosition = hostHuman.transform.position;

        isTargetPositionAvailable = true;

        isActive = true;
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

    public void Stop()
    {
        isActive = false;

        //hostHuman.Stop();
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