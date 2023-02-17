using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    public BallSettings ballSettings;
    public RopeSettings ropeSettings;

    private HumanballProcessor ball;
    private RopeProcessor rope;

    private Crowd humanballCrowd;

    private RaycastHit hitInfo;

    private Transform targetBlockTransform;

    private Vector2 raycastDirection;

    private bool isBattleMode;

    public HumanballProcessor Ball => ball;

    private void Awake()
    {
        Instance = this;
    }

    public void Initialize()
    {
        raycastDirection = new Vector2(Mathf.Cos(ropeSettings.throwingAngle * Mathf.Deg2Rad), Mathf.Sin(ropeSettings.throwingAngle * Mathf.Deg2Rad));

        ball = new HumanballProcessor(ballSettings);
        rope = new RopeProcessor(ropeSettings);

        ball.AssignRope(rope);
        rope.AssignBall(ball);

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
                if (InputManager.touchPresent)
                {
                    if (targetBlockTransform)
                    {
                        if (rope.Connect(targetBlockTransform.position))
                        {
                            ball.Swing(ballSettings.motionSpeed);
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

                ball.Update();
            }
        }
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
                if (!rope.IsConnected)
                {
                    targetBlockTransform = RaycastBlock(raycastDirection);
                }
            }

            rope.Update();
        }
    }

    public void SwitchToBattleMode()
    {
        ball.Data.rigidbody.isKinematic = true;

        DropHumansToBattle();

        isBattleMode = true;
    }

    public void DropHumansToBattle()
    {
        humanballCrowd = new Crowd();

        foreach (HumanballCell cell in ball.Structure.GetFilledCells())
        {
            humanballCrowd.AddMember(cell.Human);

            cell.Human.DropToBattle(ball.Velocity + Random.insideUnitSphere);
        }

        humanballCrowd.Assault(LevelGenerator.Instance.BattlePath.stages[0].GuardCrew);

        LevelGenerator.Instance.BattlePath.stages[0].GuardCrew.Defend(humanballCrowd);
    }

    private Transform RaycastBlock(Vector2 direction)
    {
        if (Physics.Raycast(ball.Transform.position, direction, out hitInfo, 100f, 1 << 7))
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
        public float motionSpeed;
        [Space]
        public List<Transform> baseCells;
        public HumanballGenerator proceduralCells;
    }

    [System.Serializable]
    public class RopeSettings
    {
        public Transform lineTransform;
        public Transform endTransform;
        [Space]
        public Transform swingContainer;
        [Space]
        public float throwingSpeed;
        public float throwingAngle;
    }
}
