using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    public BallSettings ballSettings;
    public RopeSettings ropeSettings;

    private float ropeThrowingSpeed;

    private bool isBallConnected;

    private void Awake()
    {
        Instance = this;
    }

    private void FixedUpdate()
    {
        if (isBallConnected)
        {

        }
    }

    private void Update()
    {
        if (Input.GetMouseButton(0))
        {

        }
        else
        {
            isBallConnected = false;
        }
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
        public float throwingTime;
    }
}
