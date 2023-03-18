using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PlayerController;

public class HumanballProcessor
{
    private BallSettings ballData;

    private RopeProcessor assignedRope;

    private Humanball structure;

    private SpringEvaluator springEvaluator;
    private PulseEvaluator pulseEvaluator;

    private HumanballCell nextCell;
    private HumanballCell ropeConnectionCell;

    private Vector3 previousSwingPosition;
    private Vector3 swingVelocityDelta;

    private Vector2 tensionDeformation;

    private float ropeThrowingAngle;

    private float swingAngularSpeed;
    private float swingAngularSpeedDelta;

    private float springValue;
    private float tensionValue;

    private bool isLaunched;

    public BallSettings Data => ballData;

    public Transform Transform => ballData.rigidbody.transform;

    public Rigidbody Rigidbody => ballData.rigidbody;

    public Humanball Structure => structure;

    public Vector3 Velocity => swingVelocityDelta / Time.fixedDeltaTime;

    public HumanballProcessor(BallSettings settings, int cellsCount)
    {
        ballData = settings;

        springEvaluator = new SpringEvaluator(ballData.elasticitySettings);
        pulseEvaluator = new PulseEvaluator(Transform, ballData.pulsingSettings.retrievalFactor, ballData.pulsingSettings.clickValue * 2f);

        tensionDeformation = ballData.tensionRatio * ballData.tensionMultiplier;

        InitializeBallStructure(cellsCount);

        //UpdateCenterOfMass();
    }

    private void InitializeBallStructure(int cellsCount)
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
            ballData.proceduralCells.GenerateLayer(baseLayerCells, 0.2f, "B")
        };

        structureLayers.AddRange(ballData.proceduralCells.GenerateProceduralCells(cellsCount));//GenerateProceduralLayers(5));

        baseLayerCells[0].Human.isFree = false;

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

            if (isLaunched)
            {
                tensionValue = Mathf.Lerp(tensionValue, 1f, 0.1f);

                springEvaluator.SetValue(tensionValue);
            }
            else
            {
                assignedRope.Data.swingContainer.localEulerAngles = new Vector3(0, 0, Mathf.Sin(6.28f * Time.timeSinceLevelLoad / 4f) * 6f);
            }
        }
        else
        {
            if (structure.FilledLayersCount < 2)
            {
                ballData.rigidbody.angularVelocity = new Vector3();

                Transform.eulerAngles = new Vector3(0, 0, Mathf.LerpAngle(Transform.eulerAngles.z, ropeThrowingAngle, 0.1f));
            }

            tensionValue = 0;
        }

        Rigidbody.velocity = Vector3.ClampMagnitude(Rigidbody.velocity, ballData.motionSpeed);

        springEvaluator.Update(ref springValue);

        ballData.suspensionContainer.localScale = new Vector3(1f + tensionDeformation.x * springValue, 1f + tensionDeformation.y * springValue, 1f);
    }

    public void LateUpdate()
    {
        structure.LateUpdate();

        pulseEvaluator.Update();
    }

    public HumanballCell ReserveCell(HumanController humanController)
    {
        nextCell = structure.ReserveCell(humanController);

        humanController.enabled = true;

        UpdateCenterOfMass();

        return nextCell;
    }

    public HumanballCell StickHuman(HumanController humanController, bool closestCell = true)
    {
        nextCell = structure.AddHuman(humanController, closestCell);

        humanController.enabled = true;

        UpdateCenterOfMass();

        pulseEvaluator.Click(ballData.pulsingSettings.clickValue);

        return nextCell;
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
        if (!isLaunched)
        {
            isLaunched = true;
        }

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

        springValue = 0.5f;
    }

    public void Release()
    {
        ballData.rigidbody.isKinematic = false;

        ballData.rigidbody.velocity = Velocity;
        ballData.rigidbody.angularVelocity = new Vector3(0, 0, swingAngularSpeed / 30f);

        swingAngularSpeedDelta = 0;
    }

    public void UpdateContainerOrientation(Vector3 connectionPoint)
    {
        ballData.structureContainer.SetParent(null);
        ballData.suspensionContainer.SetParent(null);

        ropeConnectionCell = structure.GetPlanarClosestFilledCell(connectionPoint, Axis.Z);

        ballData.suspensionContainer.position = ropeConnectionCell.transform.position.ToVector2();
        ballData.suspensionContainer.up = ropeConnectionCell.transform.forward.ToVector2(); //ropeConnectionCell.transform.forward.ToVector2();

        ballData.rigidbody.transform.up = ballData.suspensionContainer.up;

        ballData.structureContainer.SetParent(ballData.suspensionContainer);
        ballData.suspensionContainer.SetParent(ballData.rigidbody.transform);

        previousSwingPosition = Transform.position;
    }

    private void UpdateCenterOfMass()
    {
        ballData.suspensionContainer.SetParent(null);

        ballData.rigidbody.transform.position = structure.GetActiveCellsMidpoint();

        ballData.suspensionContainer.SetParent(ballData.rigidbody.transform);

        previousSwingPosition = Transform.position;
    }
}
