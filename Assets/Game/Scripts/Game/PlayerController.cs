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
    public PowerUpSettings powerUpSettings;
    [Space]
    public TextMarker humanCountMarker;

    private HumanballProcessor ball;
    private RopeProcessor rope;

    private Crowd humanballCrowd;

    private RaycastHit hitInfo;

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

        ball = ballSettings.processor;
        //ball = new HumanballProcessor(ballSettings, LevelGenerator.Instance.TotalHumansCount);
        rope = new RopeProcessor(ropeSettings);

        ball.Initialize(ballSettings, LevelGenerator.Instance.TotalHumansCount * 2);

        ball.AssignRope(rope);
        rope.AssignBall(ball);

        powerUpSettings.propeller.Initialize();

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
                if (humanballCrowd.MembersCount > 0)
                {
                    ball.Transform.position = humanballCrowd.DefineMidpointXY();
                }
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

        ball.OnUpdate();
        powerUpSettings.propeller.Update();

        humanCountMarker.Update();
    }

    private void LateUpdate()
    {
        if (isBattleMode)
        {

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
                ball.DropHumans(Mathf.FloorToInt(ball.Structure.humansCount / 2));
            }

            rope.Update();
        }

        ball.OnLateUpdate();
    }

    public void SwitchToBattleMode()
    {
        ball.Rigidbody.isKinematic = true;

        rope.Disconnect();
        //rope.Update();

        ropeSettings.lineTransform.gameObject.SetActive(false);

        humanCountMarker.SetActive(false);

        DropHumansToBattle();

        LevelGenerator.Instance.BattlePath.Enter(humanballCrowd);

        isBattleMode = true;
    }

    public void Fail()
    {
        ball.Rigidbody.gameObject.SetActive(false);

        rope.SetActive(false);

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
        public HumanballProcessor processor;
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
}
