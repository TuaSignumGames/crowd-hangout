using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulseEvaluator
{
    private Transform transform;

    private Vector3 pulseVector;

    private float scaleValue;
    private float retrievalFactor;

    private float sleepingThreshold;

    private float minValue;
    private float maxValue;

    private bool isClicked;

    public Vector3 pulseRatio;

    public PulseEvaluator(Transform transform, float retrievalFactor, float scalingLimit = 1f, float sleepingThreshold = 0.01f)
    {
        this.transform = transform;
        this.retrievalFactor = retrievalFactor;
        this.sleepingThreshold = sleepingThreshold;

        minValue = 1f - scalingLimit;
        maxValue = 1f + scalingLimit;

        pulseVector = Vector3.one;
        pulseRatio = Vector3.one;
    }

    public void Update()
    {
        if (isClicked && scaleValue != 1f)
        {
            if (Mathf.Abs(1f - scaleValue) > sleepingThreshold)
            {
                scaleValue = Mathf.Lerp(scaleValue, 1f, retrievalFactor);

                pulseVector = Vector3.Lerp(pulseVector, new Vector3(1f, 1f, 1f), retrievalFactor);
            }
            else
            {
                scaleValue = 1f;

                pulseVector = new Vector3(1f, 1f, 1f);
            }

            transform.localScale = pulseVector;
        }
        else
        {
            isClicked = false;
        }
    }

    public void Click(float value)
    {
        scaleValue = Mathf.Clamp(scaleValue + value, minValue, maxValue);

        pulseVector = new Vector3(pulseVector.x + value * pulseRatio.x, pulseVector.y + value * pulseRatio.y, pulseVector.z + value * pulseRatio.z);
        pulseVector = new Vector3(Mathf.Clamp(pulseVector.x, minValue, maxValue), Mathf.Clamp(pulseVector.y, minValue, maxValue), Mathf.Clamp(pulseVector.z, minValue, maxValue));

        isClicked = true;
    }
}
