using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Propeller
{
    public GameObject propeller;
    public Transform screwContainer;
    [Space]
    public float angularSpeed;

    private float screwAngle;
    private float angularDelta;

    public void Initialize()
    {
        angularDelta = angularSpeed * Time.fixedDeltaTime;
    }

    public void Update()
    {
        screwAngle += angularDelta;

        screwContainer.localEulerAngles = new Vector3(screwAngle, 0, 0);
    }
}
