using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    public BallSettings ballSettings;
    public RopeSettings ropeSettings;

    private BallProcessor ball;
    private RopeProcessor rope;

    private RaycastHit hitInfo;

    private Transform targetBlockTransform;

    private Vector2 raycastDirection;

    private bool isLevelStarted;

    private void Awake()
    {
        Instance = this;
    }

    public void Initialize()
    {
        raycastDirection = new Vector2(Mathf.Cos(ropeSettings.throwingAngle * Mathf.Deg2Rad), Mathf.Sin(ropeSettings.throwingAngle * Mathf.Deg2Rad));

        StartCoroutine(InitializationCoroutine());
    }

    private void FixedUpdate()
    {
        if (isLevelStarted)
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
        }

        ball.Update();
    }

    private void LateUpdate()
    {
        if (InputManager.touch)
        {
            isLevelStarted = true;

            if (!rope.IsConnected)
            {
                targetBlockTransform = RaycastBlock(raycastDirection);
            }
        }

        rope.Update();
    }

    private Transform RaycastBlock(Vector2 direction)
    {
        if (Physics.Raycast(ball.Transform.position, direction, out hitInfo, 100f, 1 << 7))
        {
            return hitInfo.transform.parent;
        }

        return null;
    }

    private IEnumerator InitializationCoroutine()
    {
        ball = new BallProcessor(ballSettings);
        rope = new RopeProcessor(ropeSettings);

        ball.AssignRope(rope);
        rope.AssignBall(ball);

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
        public Transform exoCellsContainer;
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

    public class BallProcessor
    {
        private BallSettings ballData;

        private RopeProcessor assignedRope;

        private Vector3 previousSwingPosition;
        private Vector3 swingVelocityDelta;

        private float ropeThrowingAngle;
        private float angularSpeedDelta;

        public BallSettings Data => ballData;

        public Transform Transform => ballData.rigidbody.transform;

        public BallProcessor(BallSettings settings)
        {
            ballData = settings;
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

    public class RopeProcessor
    {
        private RopeSettings ropeData;

        private BallProcessor assignedBall;

        private float targetRopeLenght;
        private float actualRopeLenght;

        private float ropeLenghtDelta;

        private bool isLaunched;
        private bool isConnected;

        public RopeSettings Data => ropeData;

        public Vector3 ConnectionPoint => ropeData.swingContainer.position;

        public Vector3 Vector => ConnectionPoint - assignedBall.Transform.position;
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

        public void AssignBall(BallProcessor ballProcessor)
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

            ropeData.lineTransform.position = assignedBall.Transform.position;
            ropeData.lineTransform.up = Direction;

            ropeData.lineTransform.localScale = new Vector3(1f, actualRopeLenght, 1f);
        }

        public bool Connect(Vector3 point)
        {
            ropeData.swingContainer.position = point;

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
                targetRopeLenght = (ConnectionPoint - assignedBall.Transform.position).GetPlanarMagnitude(Axis.Z);

                actualRopeLenght = targetRopeLenght;

                assignedBall.Transform.SetParent(ropeData.swingContainer);
            }

            isConnected = true;
        }

        public void Disconnect()
        {
            isConnected = false;

            assignedBall.Transform.SetParent(null);

            targetRopeLenght = 0;
            actualRopeLenght = 0;
        }
    }
}
