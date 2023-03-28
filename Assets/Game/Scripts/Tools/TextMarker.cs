using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
public class TextMarker
{
    public Transform pivotContainer;
    public Transform markerContainer;
    [Space]
    public TextMeshPro textMesh;
    public string prefix;
    [Space]
    public Transform targetTransform;
    [Space]
    public bool lerpMotion;
    public float lerpingFactor;

    private Vector3 facingVector;

    private Vector3 offsetDirection;

    private float offsetDistance;

    public float Distance => offsetDistance;

    public TextMarker(Transform pivotContainer)
    {
        this.pivotContainer = pivotContainer;

        markerContainer = pivotContainer.GetChild(0);

        textMesh = markerContainer.GetComponentInChildren<TextMeshPro>();
    }

    public void Initialize()
    {
        offsetDirection = markerContainer.localPosition;

        offsetDistance = offsetDirection.magnitude;

        offsetDirection = offsetDirection.normalized;
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

    public void SetValue(string value)
    {
        textMesh.text = prefix + value;
    }

    public void SetActive(bool isActive)
    {
        pivotContainer.gameObject.SetActive(isActive);
    }
}