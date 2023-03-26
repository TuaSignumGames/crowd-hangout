using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerController;

public class RopeProcessor
{
    private RopeSettings ropeData;

    private HumanballProcessor assignedBall;

    private float targetRopeLenght;
    private float actualRopeLenght;

    private float ropeLenghtDelta;

    private bool isLaunched;
    private bool isConnected;

    public RopeSettings Data => ropeData;

    public Vector3 ConnectionPoint => ropeData.swingContainer.position;

    public Vector3 Vector => ConnectionPoint - ropeData.originTransform.position;
    public Vector3 Direction => Vector.normalized;

    public float Length => actualRopeLenght;

    public bool IsConnected => isConnected;

    public RopeProcessor(RopeSettings settings)
    {
        ropeData = settings;

        ropeData.lineTransform.localScale = Vector3.zero;
        ropeData.endTransform.localScale = Vector3.zero;

        ropeLenghtDelta = ropeData.throwingSpeed * Time.fixedDeltaTime;
    }

    public void AssignBall(HumanballProcessor ballProcessor)
    {
        assignedBall = ballProcessor;
    }

    public void Update()
    {
        if (isConnected)
        {
            ropeData.endTransform.position = ConnectionPoint;
            ropeData.endTransform.localScale = new Vector3(1f, 1f, 1f);
        }

        ropeData.lineTransform.position = ropeData.originTransform.position;
        ropeData.lineTransform.up = Direction;

        ropeData.lineTransform.localScale = new Vector3(1f, actualRopeLenght, 1f);
    }

    public bool Connect(Vector3 point)
    {
        ropeData.swingContainer.position = point;

        if (!isLaunched)
        {
            isLaunched = true;

            if (assignedBall.Structure.FilledLayersCount > 1)
            {
                assignedBall.UpdateContainerOrientation(point);
            }
        }

        if (!isConnected)
        {
            targetRopeLenght = Vector.GetPlanarMagnitude(Axis.Z);

            actualRopeLenght += ropeLenghtDelta;
        }
        else
        {
            if (ropeData.swingContainer.childCount == 0)
            {
                assignedBall.Transform.SetParent(ropeData.swingContainer);

                AppManager.Instance.PlayHaptic(MoreMountains.NiceVibrations.HapticTypes.LightImpact);

                Debug.Log(" - Rope is connected");
            }
        }

        isConnected = actualRopeLenght >= targetRopeLenght;

        return isConnected;
    }

    public void ConnectImmediate(Vector3 point)
    {
        ropeData.swingContainer.position = point;

        if (!isConnected)
        {
            targetRopeLenght = (ConnectionPoint - ropeData.originTransform.position).GetPlanarMagnitude(Axis.Z);

            actualRopeLenght = targetRopeLenght;

            assignedBall.Transform.SetParent(ropeData.swingContainer);
        }

        isConnected = true;
    }

    public void Disconnect()
    {
        isLaunched = false;
        isConnected = false;

        assignedBall.Transform.SetParent(null);

        targetRopeLenght = 0;
        actualRopeLenght = 0;
    }
}
