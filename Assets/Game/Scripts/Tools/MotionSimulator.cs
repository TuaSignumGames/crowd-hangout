using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionSimulator
{
    public Vector3 velocity;
    public Vector3 angularVelocity;

    private Transform transform;

    public float velocityMultiplier = 1f;

    private Vector3 gravity;
    private Vector3 gravityFixedDelta;

    private Vector3 position;
    private Vector3 eulerAngles;

    private float velocityMultiplierDelta;

    private float groundCoordY;

    private bool useGround;
    private bool useFixedUpdate;

    private bool isGrounded;

    public float groundFriction = 0;

    public bool enabled = true;
    public bool translationEnabled = true;
    public bool rotationEnabled = true;

    public Vector3 Gravity { get { return gravity; } set { gravity = value; if (useFixedUpdate) gravityFixedDelta = gravity * Time.fixedDeltaTime; } }

    public Transform Transform => transform;

    public float GroundHeight => groundCoordY;

    public bool IsGrounded => isGrounded;

    public MotionSimulator(Transform transform, MonoUpdateType updateType, float gravityModifier = 1f)
    {
        this.transform = transform;

        position = transform.position;
        eulerAngles = transform.eulerAngles;

        Gravity = Physics.gravity * gravityModifier;

        useFixedUpdate = updateType == MonoUpdateType.FixedUpdate;

        if (useFixedUpdate)
        {
            gravityFixedDelta = gravity * Time.fixedDeltaTime;
        }
    }

    public MotionSimulator(Transform transform, float groundHeight, MonoUpdateType updateType, float gravityModifier = 1f)
    {
        this.transform = transform;

        groundCoordY = groundHeight;

        position = transform.position;
        eulerAngles = transform.eulerAngles;

        Gravity = Physics.gravity * gravityModifier;

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
            if (translationEnabled)
            {
                velocity += useFixedUpdate ? gravityFixedDelta : gravity * Time.deltaTime;

                position = transform.position + velocity * velocityMultiplier * Time.fixedDeltaTime;

                transform.position = position;
            }

            if (rotationEnabled)
            {
                eulerAngles += angularVelocity * Time.fixedDeltaTime;

                transform.eulerAngles = eulerAngles;
            }

            if (useGround)
            {
                isGrounded = transform.position.y <= groundCoordY;

                transform.position = new Vector3(transform.position.x, isGrounded ? groundCoordY : transform.position.y, transform.position.z);

                if (isGrounded && groundFriction > 0 && velocity.sqrMagnitude > 0)
                {
                    if (velocity.sqrMagnitude > 0.01f)
                    {
                        velocity -= velocity.normalized * groundFriction * velocity.magnitude * Time.fixedDeltaTime;
                        angularVelocity -= angularVelocity.normalized * groundFriction * angularVelocity.magnitude * Time.fixedDeltaTime;
                    }
                    else
                    {
                        velocity = Vector3.zero;
                        angularVelocity = Vector3.zero;
                    }
                }
            }
        }
    }

    public bool DampVelocity(float dampingDuration)
    {
        if (enabled)
        {
            if (velocityMultiplierDelta == 0)
            {
                velocityMultiplierDelta = velocityMultiplier / dampingDuration * Time.fixedDeltaTime;
            }

            velocityMultiplier -= velocityMultiplierDelta;

            if (velocityMultiplier <= 0)
            {
                velocityMultiplier = 0;
            }

            return velocityMultiplier == 0;
        }
        else
        {
            return true;
        }
    }

    public void SetGround(float y)
    {
        groundCoordY = y;

        useGround = true;
    }
}
