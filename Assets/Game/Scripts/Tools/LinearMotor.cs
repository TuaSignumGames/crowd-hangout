using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinearMotor : MonoBehaviour
{
    public Transform startPoint;
    public Transform endPoint;
    [Space]
    public Transform targetTransform;
    [Space]
    public float period;

    private Vector3 _motionPivot;
    private Vector3 _motionVector;
    private Vector3 _motionDirection;

    private float _halfAmplitude;
    private float _actualDisplacementValue;

    private float _t;
    private float _dt;

    private void Awake()
    {
        _motionPivot = (startPoint.position + endPoint.position) / 2f;
        _motionVector = endPoint.position - startPoint.position;
        _motionDirection = _motionVector.normalized;

        _halfAmplitude = _motionVector.magnitude / 2f;

        _dt = 2f * Mathf.PI / period * Time.fixedDeltaTime;

        targetTransform.position = _motionPivot;
    }

    private void FixedUpdate()
    {
        _t += _dt;
        _actualDisplacementValue = Mathf.Sin(_t) * _halfAmplitude;

        targetTransform.position = _motionPivot + _motionDirection * _actualDisplacementValue;
    }
}
