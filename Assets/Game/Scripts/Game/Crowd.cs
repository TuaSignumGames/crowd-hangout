using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crowd
{
    private List<HumanController> members;

    private HumanController[] membersToCalculate;

    private Vector2 middlePoint;

    private HumanController requestedMember;

    private float currentMemberSqrDistance;
    private float closestMemberSqrDistance;

    private float totalHealthPoints;
    private float totalHealthCapacity;

    private float power;

    public HumanController[] Members => members.ToArray();

    public float Power => DefinePower();

    public int MembersCount => members.Count;

    public bool IsGrounded => IsEverybodyGrounded();

    public bool IsCombatCapable => members.Count > 0;

    public Crowd()
    {
        members = new List<HumanController>();
    }

    public Crowd(IList<HumanController> humans)
    {
        members = new List<HumanController>();

        for (int i = 0; i < humans.Count; i++)
        {
            AddMember(humans[i]);
        }

        DefinePower();
    }

    public Crowd Assault(Crowd enemyCrowd)
    {
        for (int i = 0; i < members.Count; i++)
        {
            members[i].AI.AddRivals(enemyCrowd.Members);
            members[i].AI.Assault();
        }

        return this;
    }

    public Crowd Defend(Vector3 position, Crowd enemyCrowd = null)
    {
        for (int i = 0; i < members.Count; i++)
        {
            if (enemyCrowd != null)
            {
                members[i].AI.AddRivals(enemyCrowd.Members);
            }

            members[i].AI.Defend(position);
        }

        return this;
    }

    public Crowd Defend(Crowd enemyCrowd)
    {
        for (int i = 0; i < members.Count; i++)
        {
            members[i].AI.AddRivals(enemyCrowd.Members);
            members[i].AI.Defend();
        }

        return this;
    }

    public Crowd Defend()
    {
        for (int i = 0; i < members.Count; i++)
        {
            members[i].AI.Defend();
        }

        return this;
    }

    public Crowd Stop()
    {
        for (int i = 0; i < members.Count; i++)
        {
            members[i].AI.Stop();
        }

        return this;
    }

    public void AddMember(HumanController human)
    {
        human.actualCrowd = this;

        members.Add(human);

        totalHealthPoints += human.HealthPoints;
        totalHealthCapacity += human.HealthCapacity;
    }

    public void RemoveMember(HumanController human)
    {
        human.actualCrowd = null;

        members.Remove(human);
    }

    public float DefineTotalHealthFactor()
    {
        totalHealthPoints = 0;

        for (int i = 0; i < members.Count; i++)
        {
            totalHealthPoints += members[i].HealthPoints;
        }

        return totalHealthPoints / totalHealthCapacity;
    }

    public HumanController GetClosestMember(Vector3 position)
    {
        closestMemberSqrDistance = float.MaxValue;

        membersToCalculate = members.ToArray();

        for (int i = 0; i < membersToCalculate.Length; i++)
        {
            currentMemberSqrDistance = (membersToCalculate[i].transform.position - position).GetPlanarSqrMagnitude(Axis.Y);

            if (currentMemberSqrDistance < closestMemberSqrDistance)
            {
                closestMemberSqrDistance = currentMemberSqrDistance;

                requestedMember = membersToCalculate[i];
            }
        }

        return requestedMember;
    }

    public Vector2 DefineMidpointXY()
    {
        middlePoint = new Vector2();

        membersToCalculate = members.ToArray();

        for (int i = 0; i < membersToCalculate.Length; i++)
        {
            middlePoint += new Vector2(membersToCalculate[i].transform.position.x, membersToCalculate[i].transform.position.y);
        }

        middlePoint /= membersToCalculate.Length;

        return middlePoint;
    }

    private float DefinePower()
    {
        power = 0;

        for (int i = 0; i < members.Count; i++)
        {
            //Debug.Log($" Member[{i}] Power: {members[i].Weapon.Power}");

            power += members[i].Weapon.Power;
        }

        return power;
    }

    private bool IsAnybodyGrounded()
    {
        for (int i = 0; i < members.Count; i++)
        {
            if (members[i].MotionSimulator.IsGrounded)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsEverybodyGrounded()
    {
        for (int i = 0; i < members.Count; i++)
        {
            if (!members[i].MotionSimulator.IsGrounded)
            {
                return false;
            }
        }

        return true;
    }
}
