using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static PlayerController;

public class HumanballProcessor : MonoBehaviour
{
    private BallSettings ballData;

    private RopeProcessor assignedRope;

    private Humanball structure;

    private SpringEvaluator springEvaluator;
    private PulseEvaluator pulseEvaluator;

    private HumanballCell nextCell;
    private HumanballCell ropeConnectionCell;

    private Vector3 previousPosition;
    private Vector3 velocityDelta;

    private Vector3 areaForce;

    private Vector2 tensionDeformation;

    private float ropeThrowingAngle;

    private float linearSpeed;
    private float linearAccelerationDelta;

    private float speedLimit;

    private float swingAngularSpeed;
    private float swingAngularSpeedDelta;

    private float forceAreaDampingFactor;

    private float springValue;
    private float tensionValue;

    private bool isActive;
    private bool isLaunched;

    private bool inForceArea;

    private bool isPropellerActive;

    [HideInInspector]
    public bool isAccidented;

    public BallSettings Data => ballData;

    public Transform Transform => ballData.rigidbody.transform;

    public Rigidbody Rigidbody => ballData.rigidbody;

    public Humanball Structure => structure;

    public Vector3 Velocity => velocityDelta / Time.fixedDeltaTime;

    //public bool IsGrounded => isGrounded;

    public HumanballProcessor(BallSettings settings, int cellsCount)
    {
        Initialize(settings, cellsCount);
    }

    public void Initialize(BallSettings settings, int cellsCount)
    {
        ballData = settings;

        springEvaluator = new SpringEvaluator(ballData.elasticitySettings);
        pulseEvaluator = new PulseEvaluator(Transform, ballData.pulsingSettings.retrievalFactor, ballData.pulsingSettings.clickValue * 2f);

        linearAccelerationDelta = ballData.acceleration * Time.fixedDeltaTime;

        tensionDeformation = ballData.tensionRatio * ballData.tensionMultiplier;

        InitializeBallStructure(cellsCount);

        isActive = true;

        //PlayerController.Instance.humanCountMarker.SetValue(structure.humansCount.ToString());

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

        HumanController.selectedHuman = baseLayerCells[0].Human;
        HumanController.selectedHuman.isFree = false;

        structure = new Humanball(structureLayers);

        structure.humansCount = 1;

        PlayerController.Instance.humanCountMarker.SetValue(structure.humansCount.ToString());
    }

    public void AssignRope(RopeProcessor ropeProcessor)
    {
        assignedRope = ropeProcessor;

        ropeThrowingAngle = assignedRope.Data.throwingAngle - 90f;
    }

    public void OnUpdate()
    {
        if (isActive)
        {
            if (isPropellerActive)
            {
                Rigidbody.velocity = Transform.up * linearSpeed;
                Rigidbody.angularVelocity = new Vector3();
            }
            else
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
            }

            speedLimit = Mathf.Lerp(speedLimit, ballData.speed, ballData.bumpDampingFactor);

            Rigidbody.velocity = Vector3.ClampMagnitude(Rigidbody.velocity, speedLimit);

            springEvaluator.Update(ref springValue);

            ballData.suspensionContainer.localScale = new Vector3(1f + tensionDeformation.x * springValue, 1f + tensionDeformation.y * springValue, 1f);
        }
    }

    public void OnLateUpdate()
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

        PlayerController.Instance.humanCountMarker.SetValue(structure.humansCount.ToString());

        AppManager.Instance.PlayHaptic(MoreMountains.NiceVibrations.HapticTypes.LightImpact);

        return nextCell;
    }

    public void UnstickHuman(HumanController humanController)
    {
        structure.RemoveHuman(humanController);

        if (structure.FilledCellsCount > 0)
        {
            UpdateCenterOfMass();
        }

        PlayerController.Instance.humanCountMarker.SetValue(structure.humansCount.ToString());
    }

    public void DropHumans(int count)
    {
        HumanController human = null;

        Vector3 humanballMidpoint = structure.GetActiveCellsMidpoint();

        for (int i = 0; i < count; i++)
        {
            human = structure.UsedCells[Random.Range(0, structure.UsedCells.Length)].Human;

            if (human && human != HumanController.selectedHuman)
            {
                UnstickHuman(human);

                human.MotionSimulator.groundFriction = 10f;

                human.Drop((human.transform.position - humanballMidpoint).normalized * Random.Range(5f, 10f), Random.insideUnitSphere.normalized * Random.Range(90f, 720f));
            }
        }
    }

    public void DropHumans(float percentage)
    {
        DropHumans(Mathf.FloorToInt(structure.humansCount * Mathf.Clamp01(percentage)));
    }

    public void StickWeapon(HumanController humanController, int weaponID)
    {
        humanController.SetWeapon(weaponID);

        pulseEvaluator.Click(ballData.pulsingSettings.clickValue);

        AppManager.Instance.PlayHaptic(MoreMountains.NiceVibrations.HapticTypes.LightImpact);
    }

    public void Swing()
    {
        if (!isLaunched)
        {
            isLaunched = true;
        }

        if (Transform.parent == null)
        {
            ballData.rigidbody.isKinematic = true;

            Transform.SetParent(assignedRope.Data.swingContainer);

            previousPosition = Transform.position;
        }
        
        if (swingAngularSpeedDelta == 0)
        {
            swingAngularSpeed = ballData.speed / assignedRope.Length * 57.325f;
            swingAngularSpeedDelta = swingAngularSpeed * Time.fixedDeltaTime;
        }
        
        /*
        linearSpeed = Mathf.Clamp(linearSpeed + linearAccelerationDelta, -ballData.speed, ballData.speed);

        swingAngularSpeed = linearSpeed / assignedRope.Length * 57.325f;
        swingAngularSpeedDelta = swingAngularSpeed * Time.fixedDeltaTime;
        */

        assignedRope.Data.swingContainer.localEulerAngles += new Vector3(0, 0, swingAngularSpeedDelta);

        velocityDelta = Transform.position - previousPosition;

        previousPosition = Transform.position;

        springValue = 0.5f;
    }

    public void Release()
    {
        ballData.rigidbody.isKinematic = false;

        ballData.rigidbody.velocity = Velocity;
        ballData.rigidbody.angularVelocity = new Vector3(0, 0, swingAngularSpeed / 30f);

        swingAngularSpeedDelta = 0;
    }

    public void Fly(Vector3 direction, float speed)
    {
        linearSpeed = speed;

        Transform.up = Vector3.Lerp(Transform.up, direction, 0.1f);
    }

    public void Jump(float height)
    {
        if (height > 0)
        {
            Rigidbody.velocity = new Vector3(0, Mathf.Sqrt(2 * -Physics.gravity.y * height) * 1.5f, 0);
        }
    }

    public void Bump(Vector3 contactPoint)
    {
        isAccidented = true;

        Release();

        speedLimit = ballData.speed * 3f;

        Rigidbody.velocity += (Transform.position - contactPoint).normalized * ballData.bumpImpulse;

        AppManager.Instance.PlayHaptic(MoreMountains.NiceVibrations.HapticTypes.LightImpact);
    }

    public void ApplyForce(Vector3 force)
    {
        Rigidbody.AddForce(force);
    }

    public void SetPropellerMode(bool enabled)
    {
        isPropellerActive = enabled;

        Transform.up = Vector3.right;
    }

    public void UpdateContainerOrientation(Vector3 connectionPoint)
    {
        ballData.structureContainer.SetParent(null);
        ballData.suspensionContainer.SetParent(null);
        ballData.attributesContainer.SetParent(null);

        ropeConnectionCell = structure.GetPlanarClosestFilledCell(connectionPoint, Axis.Z);

        ballData.suspensionContainer.position = ropeConnectionCell.transform.position.ToVector2();
        ballData.suspensionContainer.up = ropeConnectionCell.transform.forward.ToVector2(); //ropeConnectionCell.transform.forward.ToVector2();

        ballData.rigidbody.transform.up = ballData.suspensionContainer.up;

        ballData.structureContainer.SetParent(ballData.suspensionContainer);
        ballData.suspensionContainer.SetParent(ballData.rigidbody.transform);
        ballData.attributesContainer.SetParent(ballData.rigidbody.transform);

        previousPosition = Transform.position;
    }

    private void UpdateCenterOfMass()
    {
        ballData.suspensionContainer.SetParent(null);

        ballData.rigidbody.transform.position = structure.GetActiveCellsMidpoint();

        ballData.suspensionContainer.SetParent(ballData.rigidbody.transform);

        previousPosition = Transform.position;
    }
    /*
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 10)
        {
            areaForce = other.transform.forward * WorldManager.environmentSettings.GetForceMagnitude(other.tag);

            forceAreaDampingFactor = 0.9f;

            inForceArea = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 10)
        {
            inForceArea = false;
        }
    }
    */
    
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == 10)
        {
            PlayerController.Humanball.ApplyForce(other.transform.forward * WorldManager.environmentSettings.GetForceMagnitude(other.tag) / structure.humansCount);

            if (other.tag == "LAV")
            {
                structure.RegisteredHumans.GetRandom().Damage(1f);
            }
        }

        if (other.gameObject.layer == 11)
        {
            Bump(transform.position);

            UnstickHuman(structure.RegisteredHumans.GetRandom());
        }
    }
    
}
