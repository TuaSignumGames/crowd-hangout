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

    public int MemebersCount => members.Count;

    public Crowd()
    {
        members = new List<HumanController>();
    }

    public void MoveTo(Vector3 position)
    {
        for (int i = 0; i < members.Count; i++)
        {
            members[i].AI.SetTarget(position);
        }
    }

    public void Attack(Crowd otherCrowd)
    {
        for (int i = 0; i < members.Count; i++)
        {
            members[i].AI.SetTarget(otherCrowd.GetClosestMember(members[i].transform.position));
        }
    }

    public void AddMember(HumanController human)
    {
        human.actualCrowd = this;

        members.Add(human);
    }

    public void RemoveMember(HumanController human)
    {
        human.actualCrowd = null;

        members.Remove(human);
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
}