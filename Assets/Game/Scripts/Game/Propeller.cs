using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Propeller
{
    public GameObject container;
    public Transform screwTransform;
    [Space]
    public float flightSpeed;
    public float controlSensitivity;
    [Space]
    public float screwAngularSpeed;
    [Space]
    public float duration;

    private float screwAngle;
    private float angularDelta;

    private float activationTime;

    public bool IsActive => container.activeSelf;

    public void Initialize()
    {
        angularDelta = screwAngularSpeed * Time.fixedDeltaTime;
    }

    public void Update()
    {
        screwAngle += angularDelta;

        screwTransform.localEulerAngles = new Vector3(screwAngle, 0, 0);
    }

    public void SetActive(bool isActive)
    {
        if (isActive)
        {
            activationTime = Time.timeSinceLevelLoad;
        }

        container.SetActive(isActive);
    }

    public float GetNormalizedTime()
    {
        return (Time.timeSinceLevelLoad - activationTime) / duration;
    }
}
