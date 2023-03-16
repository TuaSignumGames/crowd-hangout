using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulseEvaluator
{
    private Transform transform;

    private float scaleValue;
    private float retrievalFactor;

    private float sleepingThreshold;

    private bool isClicked;

    public PulseEvaluator(Transform transform, float retrievalFactor, float sleepingThreshold = 0.01f)
    {
        this.transform = transform;
        this.retrievalFactor = retrievalFactor;
        this.sleepingThreshold = sleepingThreshold;
    }

    public void Update()
    {
        if (isClicked && scaleValue != 1f)
        {
            scaleValue = Mathf.Abs(1f - scaleValue) > sleepingThreshold ? Mathf.Lerp(scaleValue, 1f, retrievalFactor) : 1f;

            transform.localScale = new Vector3(scaleValue, scaleValue, scaleValue);
        }
        else
        {
            isClicked = false;
        }
    }

    public void Click(float value)
    {
        scaleValue += value;

        isClicked = true;
    }
}
