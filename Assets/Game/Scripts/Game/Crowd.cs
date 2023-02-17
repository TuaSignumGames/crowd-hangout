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

    public HumanController[] Members => members.ToArray();

    public int MembersCount => members.Count;

    public Crowd()
    {
        members = new List<HumanController>();
    }

    public Crowd(IList<HumanController> humans)
    {
        members = new List<HumanController>(humans);
    }

    public void Assault(Crowd enemyCrowd)
    {
        for (int i = 0; i < members.Count; i++)
        {
            members[i].AI.AddRivals(enemyCrowd.Members);
            members[i].AI.Assault();
        }
    }

    public void Defend(Vector3 position, Crowd enemyCrowd = null)
    {
        for (int i = 0; i < members.Count; i++)
        {
            if (enemyCrowd != null)
            {
                members[i].AI.AddRivals(enemyCrowd.Members);
            }

            members[i].AI.Defend(position);
        }
    }

    public void Defend(Crowd enemyCrowd)
    {
        for (int i = 0; i < members.Count; i++)
        {
            members[i].AI.AddRivals(enemyCrowd.Members);
            members[i].AI.Defend(members[i].transform.position);
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
