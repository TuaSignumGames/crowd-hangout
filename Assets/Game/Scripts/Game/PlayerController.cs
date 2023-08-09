using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;
    public static HumanballProcessor Humanball;

    public BallSettings ballSettings;
    public RopeSettings ropeSettings;
    public TextMarker humanCountMarker;
    [Space]
    public BattleUnitSettings battleUnitSettings;

    private HumanballProcessor ball;
    private RopeProcessor rope;

    private Crowd humanballCrowd;

    private BattleUnit pickedBattleUnit;

    private BattlePathCell stageClosestCell;
    private BattlePathCell previousStageClosestCell;

    private RaycastHit hitInfo;

    private GameObject pickedGameObject;

    private Transform targetBlockTransform;

    private Vector2 raycastDirection;

    private bool isBattleMode;

    private void Awake()
    {
        Instance = this;
    }

    public void Initialize()
    {
        raycastDirection = new Vector2(Mathf.Cos(ropeSettings.throwingAngle * Mathf.Deg2Rad), Mathf.Sin(ropeSettings.throwingAngle * Mathf.Deg2Rad));

        ball = new HumanballProcessor(ballSettings, LevelGenerator.Instance.TotalHumansCount);
        rope = new RopeProcessor(ropeSettings);

        ball.AssignRope(rope);
        rope.AssignBall(ball);

        Humanball = ball;

        humanCountMarker.Initialize();

        ball.Structure.OnLayerIncremented += (a) => humanCountMarker.IncrementDistance(0.2f);

        HumanController.InitializeAnimatorHashes();

        StartCoroutine(InitialRopeConnectionCoroutine());
    }

    private void Start()
    {
        HumanController.selectedHuman.SetWeapon(WorldManager.GetWeaponID(GameManager.TopWeaponPower), false);
    }

    private void FixedUpdate()
    {
        if (LevelManager.IsLevelStarted && !LevelManager.IsLevelFinished)
        {
            if (isBattleMode)
            {
                /*
                if (humanballCrowd.MembersCount > 0)
                {
                    ball.Transform.position = humanballCrowd.DefineMidpointXY();
                }
                */
            }
            else
            {
                if (InputManager.touchPresent && !ball.isAccidented)
                {
                    if (targetBlockTransform)
                    {
                        if (rope.Connect(targetBlockTransform.position))
                        {
                            ball.Swing();
                        }
                    }
                }
                else
                {
                    if (rope.IsLaunched)
                    {
                        targetBlockTransform = null;

                        ball.Release();
                        rope.Disconnect();
                    }
                }
            }
        }

        ball.Update();

        humanCountMarker.Update();
    }

    private void LateUpdate()
    {
        if (isBattleMode)
        {
            if (InputManager.touch)
            {
                pickedGameObject = CameraController.Instance.Raycast().transform?.gameObject;

                if (pickedGameObject?.layer == 12)
                {
                    pickedBattleUnit = pickedGameObject.GetComponent<BattleUnit>();

                    if (pickedBattleUnit.Team == HumanTeam.Yellow)
                    {
                        pickedBattleUnit.SetPicked(true);

                        BattlePathGenerator.Instance.ActiveStage.SetBattleUnitRangesActive(true);
                    }
                    else
                    {
                        pickedBattleUnit = null;
                    }
                }
            }

            if (InputManager.touchPresent)
            {
                if (pickedBattleUnit)
                {
                    pickedBattleUnit.transform.position += new Vector3(InputManager.normalizedSlideDelta.y * battleUnitSettings.translationSensitivity.y, 0, -InputManager.normalizedSlideDelta.x * battleUnitSettings.translationSensitivity.x);

                    stageClosestCell = BattlePathGenerator.Instance.ActiveStage.GetClosestAvailableCell(pickedBattleUnit.transform.position);

                    if (stageClosestCell != previousStageClosestCell)
                    {
                        pickedBattleUnit.UpdateRange(stageClosestCell);

                        previousStageClosestCell = stageClosestCell;
                    }

                    BattlePathGenerator.Instance.ActiveStage.SetBattleUnitRangesActive(true);
                }
            }
            else
            {
                if (pickedBattleUnit)
                {
                    pickedBattleUnit.PlaceAt(stageClosestCell);
                    pickedBattleUnit.SetPicked(false);

                    pickedBattleUnit = null;

                    BattlePathGenerator.Instance.ActiveStage.UpdateBattleUnitObjectives();
                    BattlePathGenerator.Instance.ActiveStage.SetBattleUnitRangesActive(false);
                }
            }
        }
        else
        {
            if (InputManager.touch)
            {
                ball.isAccidented = false;

                if (!rope.IsConnected)
                {
                    targetBlockTransform = RaycastBlock(raycastDirection);
                }
            }

            if (Input.GetKeyDown(KeyCode.Space))
            {
                float targetCoordY = LevelGenerator.Instance.GetBlockPair(Humanball.Transform.position).Position.y;

                print($"Target height: {targetCoordY}");

                Humanball.Jump(targetCoordY - Humanball.Transform.position.y);
            }

            rope.Update();
        }

        ball.LateUpdate();
    }

    public void SwitchToBattleMode()
    {
        ball.Rigidbody.isKinematic = true;

        rope.Disconnect();
        rope.Update();

        humanCountMarker.SetActive(false);

        //ball.Transform.position = BattlePathGenerator.Instance.ActiveStage.MidPoint;

        BattlePathGenerator.Instance.EnterBattle(DropCrewsToBattle(WorldManager.GetHumansAhead(HumanTeam.Yellow, BattlePathGenerator.Instance.Position.x - LevelGenerator.Instance.blockSettings.blockLength * 2f)));

        isBattleMode = true;
    }

    public void Fail()
    {
        ball.Rigidbody.gameObject.SetActive(false);

        rope.Disconnect();

        LevelManager.Instance.OnLevelFinished(false);
    }

    private void DropHumansToBattle()
    {
        humanballCrowd = new Crowd();

        foreach (HumanController human in WorldManager.GetHumansAhead(HumanTeam.Yellow, BattlePath.Instance.Position.x - LevelGenerator.Instance.blockSettings.blockLength * 2f)) //ball.Structure.GetActuallyPresentHumans())
        {
            humanballCrowd.AddMember(human);

            human.DropToBattle(ball.Velocity + Random.insideUnitSphere, Vector3.right);
        }
    }

    private Crowd[] DropCrewsToBattle(HumanController[] humans)
    {
        List<Crowd> crews = new List<Crowd>();

        Crowd requiredCrew = null;

        for (int i = 0; i < humans.Length; i++)
        {
            requiredCrew = crews.Find((c) => c.Members.GetFirst().Weapon.WeaponID == humans[i].Weapon.WeaponID);

            if (requiredCrew == null)
            {
                requiredCrew = new Crowd();

                crews.Add(requiredCrew);
            }

            requiredCrew.AddMember(humans[i]);

            humans[i].DropToBattle(ball.Velocity + Random.insideUnitSphere, Vector3.right);
        }

        for (int i = 0; i < crews.Count; i++)
        {
            print($" Crew[{i}] - WeaponID: {crews[i].Members[0].Weapon.WeaponID} / Size: {crews[i].MembersCount}");
        }

        return crews.ToArray();
    }

    private Transform RaycastBlock(Vector2 direction)
    {
        if (Physics.Raycast(ballSettings.suspensionContainer.position, direction, out hitInfo, 100f, 1 << 7))
        {
            return hitInfo.transform;
        }

        return null;
    }

    private IEnumerator InitialRopeConnectionCoroutine()
    {
        while (!(targetBlockTransform = RaycastBlock(new Vector2(0, 1f)))) { yield return null; }

        rope.ConnectImmediate(targetBlockTransform.position);
    }

    [System.Serializable]
    public class BallSettings
    {
        public Rigidbody rigidbody;
        [Space]
        public float speed;
        public float acceleration;
        [Space]
        public float bumpImpulse;
        public float bumpDampingFactor;
        [Space]
        public Transform suspensionContainer;
        public Transform structureContainer;
        [Space]
        public SpringData elasticitySettings;
        public PulseData pulsingSettings;
        public Vector2 tensionRatio;
        public float tensionMultiplier;
        [Space]
        public List<Transform> baseCells;
        public HumanballGenerator proceduralCells;
    }

    [System.Serializable]
    public class RopeSettings
    {
        public Transform lineTransform;
        [Space]
        public Transform originTransform;
        public Transform endTransform;
        [Space]
        public Transform swingContainer;
        [Space]
        public float throwingSpeed;
        public float throwingAngle;
    }

    [System.Serializable]
    public class BattleUnitSettings
    {
        public Vector2 translationSensitivity;
    }
}
