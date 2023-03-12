using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MulticollectibleElement
{
    protected MotionSimulator motionSimulator;

    protected Vector3 targetVector;

    protected Vector3 velocity;
    protected Vector3 velocityIncrement;

    protected Vector3 pullingStartPosition;

    protected float t;
    protected float dt;

    protected float actualSpeed;
    protected float speedLimit;
    protected float speedIncrement;

    protected float sqrDistanceToTarget;
    protected float sqrTargetPointRadius;

    protected float pullDelay;
    protected float pullAvailabilityTime;

    protected bool isCollected;

    public bool IsCollected => isCollected;

    public MulticollectibleElement(MotionSimulator motionSimulator, float speed, float acceleration, float delay, float targetPointRadius)
    {
        this.motionSimulator = motionSimulator;

        dt = 1f / delay * Time.fixedDeltaTime;

        speedLimit = speed;
        speedIncrement = acceleration * Time.fixedDeltaTime;

        sqrTargetPointRadius = targetPointRadius * targetPointRadius;

        pullDelay = delay;
    }

    public virtual bool Pull(Transform targetTransform)
    {
        if (pullAvailabilityTime == 0)
        {
            pullAvailabilityTime = Time.timeSinceLevelLoad + pullDelay;
        }

        if (Time.timeSinceLevelLoad > pullAvailabilityTime)
        {
            if (t == 0)
            {
                pullingStartPosition = motionSimulator.Transform.position;
            }    

            t += dt;

            motionSimulator.Transform.position = Vector3.Lerp(pullingStartPosition, targetTransform.position, t);

            if (t >= 1f)
            {
                return isCollected = true;
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
