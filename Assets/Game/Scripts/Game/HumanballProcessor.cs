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

    public Humanball Structure => structure;

    public Vector3 Velocity => swingVelocityDelta / Time.fixedDeltaTime;

    public HumanballProcessor(BallSettings settings)
    {
        ballData = settings;

        InitializeBallStructure();
    }

    private void InitializeBallStructure()
    {
        List<HumanballCell> baseLayerCells = new List<HumanballCell>();

        for (int i = 0; i < ballData.baseCells.Count; i++)
        {
            baseLayerCells.Add(new HumanballCell(ballData.baseCells[i].gameObject));

            if (i == 0)
            {
                baseLayerCells[i].TrySavePose();
            }
            else
            {
                baseLayerCells[i].TryCropPose();
            }
        }

        List<HumanballLayer> structureLayers = new List<HumanballLayer>();

        structureLayers.Add(ballData.proceduralCells.GenerateLayer(baseLayerCells, "B"));
        structureLayers.AddRange(ballData.proceduralCells.GenerateProceduralLayers());

        baseLayerCells[0].Human.Initialize(false);

        structure = new Humanball(structureLayers);
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
        structure.AddHuman(humanController);

        humanController.enabled = true;
    }

    public void UnstickHuman(HumanController humanController)
    {
        structure.RemoveHuman(humanController);
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

        ballData.rigidbody.velocity = Velocity;

        angularSpeedDelta = 0;
    }
}
