using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerController;

public class HumanballProcessor
{
    private BallSettings ballData;

    private RopeProcessor assignedRope;

    private Humanball structure;

    private Vector3 previousSwingPosition;
    private Vector3 swingVelocityDelta;

    private float ropeThrowingAngle;
    private float angularSpeedDelta;

    public BallSettings Data => ballData;

    public Transform Transform => ballData.rigidbody.transform;

    public HumanballProcessor(BallSettings settings)
    {
        ballData = settings;

        InitializeBallStructure();
    }

    private void InitializeBallStructure()
    {
        // TODO Add 'Base Cells' as Layer[0], add prcedural Layers 

        //structure = new Humanball(ballData.proceduralCells.GenerateLayers());
    }

    public void AssignRope(RopeProcessor ropeProcessor)
    {
        assignedRope = ropeProcessor;

        ropeThrowingAngle = assignedRope.Data.throwingAngle - 90f;
    }

    public void Update()
    {
        if (assignedRope.IsConnected)
        {
            Transform.up = Vector3.Lerp(Transform.up, assignedRope.Direction, 0.1f);
        }
        else
        {
            Transform.eulerAngles = new Vector3(0, 0, Mathf.LerpAngle(Transform.eulerAngles.z, ropeThrowingAngle, 0.1f));
        }
    }

    public void StickHuman(HumanController humanController)
    {
        Debug.Log($" - Human stick attempt");

        
    }

    public void UnstickHuman(HumanController humanController)
    {

    }

    public void Swing(float linearSpeed)
    {
        if (Transform.parent == null)
        {
            ballData.rigidbody.isKinematic = true;

            Transform.SetParent(assignedRope.Data.swingContainer);

            previousSwingPosition = Transform.position;
        }

        if (angularSpeedDelta == 0)
        {
            angularSpeedDelta = linearSpeed / assignedRope.Length * 57.325f * Time.fixedDeltaTime;
        }

        assignedRope.Data.swingContainer.localEulerAngles += new Vector3(0, 0, angularSpeedDelta);

        swingVelocityDelta = Transform.position - previousSwingPosition;

        previousSwingPosition = Transform.position;
    }

    public void Release()
    {
        ballData.rigidbody.isKinematic = false;

        ballData.rigidbody.velocity = swingVelocityDelta / Time.fixedDeltaTime;

        angularSpeedDelta = 0;
    }
}
