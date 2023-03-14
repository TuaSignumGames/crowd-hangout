using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionSimulator
{
    public Vector3 velocity;
    public Vector3 angularVelocity;

    private Transform transform;

    private Vector3 gravity;
    private Vector3 gravityFixedDelta;

    private Vector3 position;
    private Vector3 eulerAngles;

    private float groundCoordY;

    private bool useGround;
    private bool useFixedUpdate;

    private bool isGrounded;

    public bool enabled = true;

    public int instanceID;

    public Vector3 Gravity { get { return gravity; } set { gravity = value; if (useFixedUpdate) gravityFixedDelta = gravity * Time.fixedDeltaTime; } }

    public Transform Transform => transform;

    public bool IsGrounded => isGrounded;

    public MotionSimulator(Transform transform, MonoUpdateType updateType)
    {
        this.transform = transform;

        position = transform.position;
        eulerAngles = transform.eulerAngles;

        Gravity = Physics.gravity;

        useFixedUpdate = updateType == MonoUpdateType.FixedUpdate;

        if (useFixedUpdate)
        {
            gravityFixedDelta = gravity * Time.fixedDeltaTime;
        }
    }

    public MotionSimulator(Transform transform, float groundHeight, MonoUpdateType updateType)
    {
        this.transform = transform;

        groundCoordY = groundHeight;

        position = transform.position;
        eulerAngles = transform.eulerAngles;

        Gravity = Physics.gravity;

        useGround = true;

        useFixedUpdate = updateType == MonoUpdateType.FixedUpdate;

        if (useFixedUpdate)
        {
            gravityFixedDelta = gravity * Time.fixedDeltaTime;
        }
    }

    public void Update()
    {
        if (enabled)
        {
            velocity += useFixedUpdate ? gravityFixedDelta : gravity * Time.deltaTime;

            position = transform.position + velocity * Time.fixedDeltaTime;
            eulerAngles = transform.eulerAngles + angularVelocity * Time.fixedDeltaTime;

            transform.position = position;
            transform.eulerAngles = eulerAngles;

            if (useGround)
            {
                isGrounded = transform.position.y <= groundCoordY;

                transform.position = new Vector3(transform.position.x, isGrounded ? groundCoordY : transform.position.y, transform.position.z);
            }

            if (instanceID > 0)
            {
                Debug.Log($" - Motion simulator is updating [{instanceID}]");
            }
        }
    }

    public void SetGround(float y)
    {
        groundCoordY = y;

        useGround = true;
    }
}
