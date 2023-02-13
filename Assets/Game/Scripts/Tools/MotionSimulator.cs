using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionSimulator
{
    private Transform transform;

    private Vector3 gravity;
    private Vector3 gravityFixedDelta;

    private float groundPositionY;

    private bool useGround;
    private bool useFixedUpdate;

    private bool isGrounded;

    public Vector3 velocity;
    public Vector3 angularVelocity;

    public bool IsGrounded => isGrounded;

    public MotionSimulator(Transform transform, MonoUpdateType updateType)
    {
        this.transform = transform;

        gravity = Physics.gravity;

        useFixedUpdate = updateType == MonoUpdateType.FixedUpdate;

        if (useFixedUpdate)
        {
            gravityFixedDelta = gravity * Time.fixedDeltaTime;
        }
    }

    public MotionSimulator(Transform transform, float groundPositionY, MonoUpdateType updateType)
    {
        this.transform = transform;
        this.groundPositionY = groundPositionY;

        gravity = Physics.gravity;

        useGround = true;

        useFixedUpdate = updateType == MonoUpdateType.FixedUpdate;

        if (useFixedUpdate)
        {
            gravityFixedDelta = gravity * Time.fixedDeltaTime;
        }
    }

    public void Update()
    {
        velocity += useFixedUpdate ? gravityFixedDelta : gravity * Time.deltaTime;

        transform.position += velocity * Time.fixedDeltaTime;
        transform.eulerAngles += angularVelocity * Time.fixedDeltaTime;

        if (useGround)
        {
            isGrounded = transform.position.y <= groundPositionY;

            transform.position = new Vector3(transform.position.x, isGrounded ? groundPositionY : transform.position.y, transform.position.z);
        }
    }
}
