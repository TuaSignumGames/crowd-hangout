using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public class CircularProgressMarker
{
    public Transform pivotContainer;
    public Transform markerContainer;
    [Space]
    public SpriteRenderer fillingRenderer;
    [Space]
    public Transform targetTransform;
    [Space]
    public bool lerpMotion;
    public float lerpingFactor;

    private Material fillingMaterial;

    private Vector3 facingVector;

    private Vector3 offsetDirection;

    private float offsetDistance;

    public float Distance => offsetDistance;

    public void Initialize()
    {
        offsetDirection = markerContainer.localPosition;

        offsetDistance = offsetDirection.magnitude;

        offsetDirection = offsetDirection.normalized;

        fillingMaterial = fillingRenderer.material;
    }

    public void Update()
    {
        facingVector = CameraController.Instance.camera.transform.position - markerContainer.transform.position;

        if (targetTransform)
        {
            pivotContainer.position = lerpMotion ? Vector3.Lerp(pivotContainer.position, targetTransform.position, lerpingFactor) : targetTransform.position;
        }

        pivotContainer.forward = facingVector;
        markerContainer.forward = facingVector;
    }

    public void SetDistance(float value)
    {
        offsetDistance = value;

        markerContainer.localPosition = offsetDirection * offsetDistance;
    }

    public void IncrementDistance(float increment)
    {
        SetDistance(offsetDistance + increment);
    }

    public void SetActive(bool isActive)
    {
        pivotContainer.gameObject.SetActive(isActive);
    }

    public float SetValue(float value)
    {
        fillingMaterial.SetFloat("_Arc1", 360f * value);

        return value;
    }
}
