using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MulticollectibleElement
{
    protected Transform transform;

    protected Transform targetPointTransform;

    protected float pullAvailabilityTime;

    protected float t;
    protected float dt;

    protected bool isPulling;

    protected System.Action callback;

    public bool IsPulling => isPulling;

    public MulticollectibleElement(Transform elementTransform)
    {
        transform = elementTransform;
    }

    public virtual void Update()
    {
        if (isPulling)
        {
            if (Time.timeSinceLevelLoad > pullAvailabilityTime)
            {
                t += dt;

                transform.position = Vector3.Lerp(transform.position, targetPointTransform.position, t);

                if (t >= 1f)
                {
                    isPulling = false;

                    callback?.Invoke();
                }
            }
        }
    }

    public virtual void Pull(Transform targetPointTransform, float duration, System.Action onPointReached, float delay = 0)
    {
        this.targetPointTransform = targetPointTransform;

        dt = 1f / duration * Time.fixedDeltaTime;

        callback = onPointReached;

        if (delay > 0)
        {
            pullAvailabilityTime = Time.timeSinceLevelLoad + delay;
        }

        isPulling = true;
    }
}
