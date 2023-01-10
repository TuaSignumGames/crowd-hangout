using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouncyPoint : MonoBehaviour
{
    [SerializeField] private float _springFactor;
    [SerializeField] private float _dampingFactor;
    [Space]
    [SerializeField] private float _stabilizationFactor;

    private Vector3 _localVelocity;

    private Vector3 _localPivotPosition;
    private Vector3 _localPivotOffset;

    private float _springFactorDelta;

    private void Awake()
    {
        _localPivotPosition = transform.localPosition;

        _springFactorDelta = _springFactor * Time.fixedDeltaTime;
    }

    private void FixedUpdate()
    {
        _localVelocity += (_localPivotPosition + _localPivotOffset - transform.localPosition) * _springFactorDelta;

        _localVelocity *= _dampingFactor;

        transform.localPosition += _localVelocity * Time.fixedDeltaTime;

        _localPivotOffset = Vector3.Lerp(_localPivotOffset, new Vector3(0, 0, 0), _stabilizationFactor);
    }

    public void Twitch(Vector3 impulse)
    {
        _localPivotOffset = impulse;
    }
}
