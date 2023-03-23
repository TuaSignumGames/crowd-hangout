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

        ball = new HumanballProcessor(ballSettings, LevelGenerator.Instance.TotalHumansCount);
        rope = new RopeProcessor(ropeSettings);

        ball.AssignRope(rope);
        rope.AssignBall(ball);

        Humanball = ball;

        HumanController.InitializeAnimatorHashes();

        StartCoroutine(InitialRopeConnectionCoroutine());
    }

    private void FixedUpdate()
    {
        if (LevelManager.IsLevelStarted)
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
                    if (rope.IsConnected)
                    {
                        targetBlockTransform = null;

                        ball.Release();
                        rope.Disconnect();
                    }
                }

                //ball.Update();
            }
        }

        ball.Update();
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
        ball.Data.rigidbody.isKinematic = true;

        DropHumansToBattle();

        LevelGenerator.Instance.BattlePath.Enter(humanballCrowd);

        isBattleMode = true;
    }

    private void DropHumansToBattle()
    {
        humanballCrowd = new Crowd();

        foreach (HumanballCell cell in ball.Structure.GetFilledCells())
        {
            humanballCrowd.AddMember(cell.Human);

            cell.Human.DropToBattle(ball.Velocity + Random.insideUnitSphere, Vector3.right);
        }
    }

    private Transform RaycastBlock(Vector2 direction)
    {
        if (Physics.Raycast(ballSettings.suspensionContainer.position, direction, out hitInfo, 100f, 1 << 7))
        {
            return hitInfo.transform.parent;
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
}
