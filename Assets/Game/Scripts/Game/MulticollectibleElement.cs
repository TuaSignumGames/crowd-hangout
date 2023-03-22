using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MulticollectibleElement
{
    protected Transform transform;

    protected MotionSimulator motionSimulator;

    protected Vector3 targetVector;

    protected Vector3 pullingStartPosition;

    protected float t;
    protected float dt;

    protected float pullSpeed;
    protected float pullDelay;

    protected float pullAvailabilityTime;

    protected bool isActive;
    protected bool isCollected;

    protected bool internalMotionSimulation;

    public Transform Transform => transform;

    public bool IsActive => isActive;
    public bool IsCollected => isCollected;
    public bool IsCollecting => isActive && !isCollected;

    public MulticollectibleElement(Transform elementTransform, float speed, float delay)
    {
        transform = elementTransform;

        motionSimulator = new MotionSimulator(transform, MonoUpdateType.FixedUpdate);

        pullSpeed = speed;
        pullDelay = delay;

        internalMotionSimulation = true;
    }

    public MulticollectibleElement(MotionSimulator elementMotionSimulator, float speed, float delay)
    {
        transform = elementMotionSimulator.Transform;

        motionSimulator = elementMotionSimulator;

        pullSpeed = speed;
        pullDelay = delay;
    }

    public virtual void Collect()
    {
        isActive = true;

        transform.gameObject.SetActive(true);
    }

    public virtual void Drop(Vector3 impulse, Vector3 angularMomentum)
    {
        motionSimulator.velocity = impulse;
        motionSimulator.angularVelocity = angularMomentum;

        Collect();
    }

    public virtual bool Pull(Transform targetTransform)
    {
        if (isActive)
        {
            if (pullAvailabilityTime == 0)
            {
                pullAvailabilityTime = Time.timeSinceLevelLoad + pullDelay;
            }

            motionSimulator.DampVelocity(pullDelay);

            if (Time.timeSinceLevelLoad > pullAvailabilityTime)
            {
                if (t == 0)
                {
                    pullingStartPosition = transform.position;

                    dt = pullSpeed / (targetTransform.position - pullingStartPosition).magnitude * Time.fixedDeltaTime;

                    if (motionSimulator != null)
                    {
                        motionSimulator.enabled = false;
                    }
                }

                t += dt;

                transform.position = Vector3.Lerp(pullingStartPosition, targetTransform.position, t);

                if (t >= 1f)
                {
                    isActive = false;

                    return isCollected = true;
                }
            }

            if (internalMotionSimulation)
            {
                motionSimulator.Update();
            }
        }

        return false;
    }

    /*
    public virtual bool Pull(Transform targetTransform)
    {
        if (pullAvailabilityTime == 0)
        {
            pullAvailabilityTime = Time.timeSinceLevelLoad + pullDelay;
        }

        if (Time.timeSinceLevelLoad > pullAvailabilityTime)
        {
            targetVector = targetTransform.position - motionSimulator.Transform.position;

            sqrDistanceToTarget = targetVector.sqrMagnitude;

            velocity = Vector3.ClampMagnitude(velocity + targetVector.normalized * speedIncrement, speedLimit);

            motionSimulator.velocity = velocity;

            if (sqrDistanceToTarget < sqrTargetPointRadius)
            {
                return isCollected = true;
            }
        }

        return false;
    }
    */
}
