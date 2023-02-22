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

    private float swingAngularSpeed;
    private float swingAngularSpeedDelta;

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

        List<HumanballLayer> structureLayers = new List<HumanballLayer>
        {
            ballData.proceduralCells.GenerateLayer(baseLayerCells, "B")
        };

        structureLayers.AddRange(ballData.proceduralCells.GenerateProceduralLayers());

        baseLayerCells[0].Human.isFree = false;

        baseLayerCells[0].Human.SetWeapon(4);       // Weapons debugging 

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
            if (structure.FilledLayersCount < 2)
            {
                ballData.rigidbody.angularVelocity = new Vector3();

                Transform.eulerAngles = new Vector3(0, 0, Mathf.LerpAngle(Transform.eulerAngles.z, ropeThrowingAngle, 0.1f));
            }
        }
    }

    public void StickHuman(HumanController humanController)
    {
        structure.AddHuman(humanController);

        humanController.enabled = true;

        UpdateCenterOfMass();
    }

    public void UnstickHuman(HumanController humanController)
    {
        structure.RemoveHuman(humanController);

        if (structure.FilledCellsCount > 0)
        {
            UpdateCenterOfMass();
        }
    }

    public void Swing(float linearSpeed)
    {
        if (Transform.parent == null)
        {
            ballData.rigidbody.isKinematic = true;

            Transform.SetParent(assignedRope.Data.swingContainer);

            previousSwingPosition = Transform.position;
        }

        if (swingAngularSpeedDelta == 0)
        {
            swingAngularSpeed = linearSpeed / assignedRope.Length * 57.325f;
            swingAngularSpeedDelta = swingAngularSpeed * Time.fixedDeltaTime;
        }

        assignedRope.Data.swingContainer.localEulerAngles += new Vector3(0, 0, swingAngularSpeedDelta);

        swingVelocityDelta = Transform.position - previousSwingPosition;

        previousSwingPosition = Transform.position;
    }

    public void Release()
    {
        Debug.Log(structure.FilledLayersCount);

        ballData.rigidbody.isKinematic = false;

        ballData.rigidbody.velocity = Velocity;
        ballData.rigidbody.angularVelocity = new Vector3(0, 0, swingAngularSpeed / 30f);

        swingAngularSpeedDelta = 0;
    }

    public void UpdateContainerOrientation(Vector3 connectionPoint)
    {
        ballData.suspensionContainer.SetParent(null);

        ballData.suspensionContainer.position = structure.GetPlanarClosestCell(connectionPoint, Axis.Z).transform.position;
        ballData.suspensionContainer.up = (connectionPoint - ballData.suspensionContainer.position).GetPlanarDirection(Axis.Z);

        ballData.rigidbody.transform.up = ballData.suspensionContainer.up;

        ballData.suspensionContainer.SetParent(ballData.rigidbody.transform);
    }

    private void UpdateCenterOfMass()
    {
        ballData.suspensionContainer.SetParent(null);

        ballData.rigidbody.transform.position = structure.GetActiveCellsMidpoint();

        ballData.suspensionContainer.SetParent(ballData.rigidbody.transform);
    }
}
