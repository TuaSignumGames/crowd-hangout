using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MulticollectibleElement
{
    protected MotionSimulator motionSimulator;

    protected Vector3 targetVector;

    protected Vector3 velocity;
    protected Vector3 velocityIncrement;

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

        speedLimit = speed;
        speedIncrement = acceleration * Time.fixedDeltaTime;

        sqrTargetPointRadius = targetPointRadius * targetPointRadius;

        pullDelay = delay;
    }

    public virtual bool Pull(Transform targetTransform)
    {
        // TODO Define pull mechanics 

        if (pullAvailabilityTime == 0)
        {
            pullAvailabilityTime = Time.timeSinceLevelLoad + pullDelay;
        }

        if (Time.timeSinceLevelLoad > pullAvailabilityTime)
        {
            targetVector = targetTransform.position - motionSimulator.Transform.position;

            sqrDistanceToTarget = targetVector.sqrMagnitude;

            if (sqrDistanceToTarget > 10f)
            {
                velocity = Vector3.ClampMagnitude(velocity + targetVector.normalized * speedIncrement, speedLimit);
            }
            else
            {
                velocity = targetVector.normalized * actualSpeed;

                actualSpeed = actualSpeed < speedLimit ? actualSpeed + speedIncrement : speedLimit;
            }

            motionSimulator.velocity = velocity;

            if (sqrDistanceToTarget < sqrTargetPointRadius)
            {
                return isCollected = true;
            }
        }

        return false;
    }
}
