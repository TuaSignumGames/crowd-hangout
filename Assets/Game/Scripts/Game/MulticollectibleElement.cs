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

    public bool IsActive => isActive;
    public bool IsCollected => isCollected;

    public MulticollectibleElement(Transform elementTransform, float speed, float delay)
    {
        transform = elementTransform;

        pullSpeed = speed;
        pullDelay = delay;
    }

    public MulticollectibleElement(MotionSimulator elementMotionSimulator, float speed, float delay)
    {
        motionSimulator = elementMotionSimulator;

        transform = motionSimulator.Transform;

        pullSpeed = speed;
        pullDelay = delay;
    }

    public virtual void Collect()
    {
        isActive = true;
    }

    public virtual bool Pull(Transform targetTransform)
    {
        if (isActive)
        {
            if (pullAvailabilityTime == 0)
            {
                pullAvailabilityTime = Time.timeSinceLevelLoad + pullDelay;
            }

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
                    return isCollected = true;
                }
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
