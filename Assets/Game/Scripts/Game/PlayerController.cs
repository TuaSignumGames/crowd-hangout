using System.Collections;
using System.Collections.Generic;
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

    private void Awake()
    {
        Instance = this;
    }

    public void Initialize()
    {
        StartCoroutine(InitializationCoroutine());
    }

    private void FixedUpdate()
    {
        if (InputManager.touchPresent)
        {
            if (targetBlockTransform)
            {
                if (rope.Connect(targetBlockTransform.position))
                {
                    ball.ApplyVelocity(Vector3.Cross(rope.Direction, new Vector3(0, 0, 1f)));
                }
            }
        }
        else
        {
            if (rope.IsConnected)
            {
                targetBlockTransform = null;

                rope.Disconnect();
            }
        }
    }

    private void LateUpdate()
    {
        if (InputManager.touch)
        {
            if (ballSettings.rigidbody.isKinematic)
            {
                ballSettings.rigidbody.isKinematic = false;
            }

            if (!rope.IsConnected)
            {
                targetBlockTransform = RaycastBlock(new Vector2(0.5f, 1f));
            }
        }
    }

    private Transform RaycastBlock(Vector2 direction)
    {
        if (Physics.Raycast(ball.Transform.position, direction, out hitInfo, 100f, 1 << 7))
        {
            print($" - Raycasted block coord: {hitInfo.transform.parent.position.x}");

            return hitInfo.transform.parent;
        }

        return null;
    }

    private IEnumerator InitializationCoroutine()
    {
        ball = new BallProcessor(ballSettings);
        rope = new RopeProcessor(ropeSettings, ball.Transform);

        while (!(targetBlockTransform = RaycastBlock(new Vector2(0, 1f)))) { yield return null; }

        rope.ConnectImmediate(targetBlockTransform.position);
    }

    [System.Serializable]
    public class BallSettings
    {
        public GameObject gameObject;
        public Rigidbody rigidbody;
        [Space]
        public float motionSpeed;
    }

    [System.Serializable]
    public class RopeSettings
    {
        public Transform lineTransform;
        public Transform endTransform;
        [Space]
        public float throwingSpeed;
    }

    public class BallProcessor
    {
        private BallSettings ballData;

        public Transform Transform => ballData.gameObject.transform;

        public BallProcessor(BallSettings settings)
        {
            ballData = settings;
        }

        public void ApplyVelocity(Vector3 direction)
        {
            ballData.rigidbody.velocity = direction * ballData.motionSpeed;
        }
    }

    public class RopeProcessor
    {
        private RopeSettings ropeData;

        private Transform sourceTransform;

        private float targetRopeLenght;
        private float actualRopeLenght;

        private float ropeLenghtDelta;

        private bool isLaunched;
        private bool isConnected;

        public Vector3 Direction => ropeData.lineTransform.up;

        public bool IsConnected => isConnected;

        public RopeProcessor(RopeSettings settings, Transform ballTransform)
        {
            ropeData = settings;

            sourceTransform = ballTransform;

            ropeData.lineTransform.localScale = Vector3.zero;
            ropeData.endTransform.localScale = Vector3.zero;

            ropeLenghtDelta = ropeData.throwingSpeed * Time.fixedDeltaTime;
        }

        public bool Connect(Vector3 point)
        {
            if (!isConnected)
            {
                targetRopeLenght = (point - sourceTransform.position).GetPlanarMagnitude(Axis.Z);

                actualRopeLenght += ropeLenghtDelta;

                ropeData.lineTransform.localScale = new Vector3(1f, actualRopeLenght, 1f);
            }
            else
            {
                ropeData.endTransform.position = point;
                ropeData.endTransform.localScale = new Vector3(1f, 1f, 1f);
            }

            ropeData.lineTransform.position = sourceTransform.position;
            ropeData.lineTransform.up = (point - sourceTransform.position).normalized;

            isConnected = actualRopeLenght >= targetRopeLenght;

            return isConnected;
        }

        public void ConnectImmediate(Vector3 point)
        {
            if (!isConnected)
            {
                targetRopeLenght = (point - sourceTransform.position).GetPlanarMagnitude(Axis.Z);

                actualRopeLenght = targetRopeLenght;

                ropeData.lineTransform.localScale = new Vector3(1f, actualRopeLenght, 1f);

                ropeData.endTransform.position = point;
                ropeData.endTransform.localScale = new Vector3(1f, 1f, 1f);
            }

            ropeData.lineTransform.position = sourceTransform.position;
            ropeData.lineTransform.up = (point - sourceTransform.position).normalized;

            isConnected = true;
        }

        public void Disconnect()
        {
            isConnected = false;

            targetRopeLenght = 0;
            actualRopeLenght = 0;
        }
    }
}
