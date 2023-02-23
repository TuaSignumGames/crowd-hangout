using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringEvaluator
{
    private float _pivotValue;
    private float _springFactor;
    private float _dampingFactor;

    private float _value;
    private float _baseValue;
    private float _basePivotValue;

    private float _valueIncrement;
    private float _springFactorDelta;

    private float _valueFloor;
    private float _valueCeil;

    private float _pivotLerpingFactor;

    public float TensionFactor => Mathf.Abs(_pivotValue - _value) / Mathf.Abs(_basePivotValue - _baseValue);

    public bool IsTensioned => TensionFactor > 0.1f;

    public SpringEvaluator(float pivotValue, float baseValue, float springFactor, float dampingFactor, float valueFloor, float valueCeil, float restTimeout = 0)
    {
        _pivotValue = pivotValue;
        _springFactor = springFactor;
        _dampingFactor = dampingFactor;

        _value = baseValue;
        _baseValue = baseValue;
        _basePivotValue = _pivotValue;

        _valueFloor = valueFloor;
        _valueCeil = valueCeil;

        _springFactorDelta = _springFactor * Time.fixedDeltaTime;

        //_pivotLerpingFactor = pivotLerpingFactor;
    }

    public SpringEvaluator(SpringData springData, float pivotValue = 0, float baseValue = 0, float valueFloor = -1f, float valueCeil = 1f, float restTimeout = 0)
    {
        _springFactor = springData.springFactor;
        _dampingFactor = springData.dampingFactor;

        _value = baseValue;
        _pivotValue = pivotValue;
        _baseValue = baseValue;
        _basePivotValue = _pivotValue;

        _valueFloor = valueFloor;
        _valueCeil = valueCeil;

        _springFactorDelta = _springFactor * Time.fixedDeltaTime;
    }

    /// <summary>
    /// Call in FixedUpdate loop
    /// </summary>
    /// <param name="value">Evaluated output value</param>
    public void Update(ref float value)
    {
        _valueIncrement += (_pivotValue - value) * _springFactorDelta;
        _valueIncrement *= _dampingFactor;

        _value += _valueIncrement * Time.fixedDeltaTime;

        if (_value <= _valueFloor || _value >= _valueCeil)
        {
            Damp();
        }

        value = _value;

        //value = Mathf.Clamp(_value, _valueFloor, _valueCeil);
    }

    public void SetValue(float value)
    {
        _valueIncrement = 0;

        _value = value;
    }

    public void ApplyPivotValue(float value)
    {
        _pivotValue = value;
    }

    public void Damp()
    {
        _valueIncrement = 0;
    }

    public void Reset()
    {
        _value = _baseValue;
        _valueIncrement = 0;
    }
}
