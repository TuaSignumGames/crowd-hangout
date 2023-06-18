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

    private float screwAngle;
    private float angularDelta;

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
        container.SetActive(isActive);
    }
}
