using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pendulum : MonoBehaviour
{
    public Transform pivotTransform;
    public Vector3 amplitude;
    public Vector3 period;

    private Vector3 baseEulerAngles;
    private Vector3 actualEulerAngles;

    private Vector3 time;
    private Vector3 deltaTime;

    private void Awake()
    {
        baseEulerAngles = pivotTransform.localEulerAngles;

        deltaTime = new Vector3();

        deltaTime.x = period.x != 0 ? Time.fixedDeltaTime / period.x : 0;
        deltaTime.y = period.y != 0 ? Time.fixedDeltaTime / period.y : 0;
        deltaTime.z = period.z != 0 ? Time.fixedDeltaTime / period.z : 0;
    }

    private void FixedUpdate()
    {
        time += deltaTime;

        if (amplitude.x != 0)
        {
            actualEulerAngles.x = Mathf.Sin(3.1416f * time.x) * amplitude.x;
        }
        if (amplitude.y != 0)
        {
            actualEulerAngles.y = Mathf.Sin(3.1416f * time.y) * amplitude.y;
        }
        if (amplitude.z != 0)
        {
            actualEulerAngles.z = Mathf.Sin(3.1416f * time.z) * amplitude.z;
        }

        pivotTransform.localEulerAngles = baseEulerAngles + actualEulerAngles;
    }
}
