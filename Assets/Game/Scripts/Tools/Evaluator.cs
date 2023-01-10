using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EvaluationType { Linear, Smooth }

public class Evaluator
{
    private MonoUpdateType _iteratorType;
    private EvaluationType _evaluationType;

    private float _t;
    private float _dt;

    private float _baseFloatValue;
    private float _targetFloatValue;

    private float _floatValue;

    private Vector3 _baseVector3Value;
    private Vector3 _targetVector3Value;

    private Transform _targetTransform;

    private Vector3 _vector3Value;

    private Action _onEvaluationCompleted;

    private bool _iterating;

    public MonoUpdateType IteratorType => _iteratorType;

    public float EvaluationFactor => _t;

    public bool angular;

    public bool Iterating => _iterating;

    public Evaluator(MonoUpdateType iteratorType)
    {
        _iteratorType = iteratorType;
    }

    public Evaluator Setup(float from, float to, float duration, EvaluationType evaluationType, Action onEvaluationCompleted = null)
    {
        _evaluationType = evaluationType;

        _t = 0;
        _dt = 1f / duration;

        if (_iteratorType == MonoUpdateType.FixedUpdate)
        {
            _dt *= Time.fixedDeltaTime;
        }

        _baseFloatValue = from;
        _targetFloatValue = to;

        _onEvaluationCompleted = onEvaluationCompleted;

        _iterating = true;

        return this;
    }

    public Evaluator Setup(Vector3 from, Vector3 to, float duration, EvaluationType evaluationType, Action onEvaluationCompleted = null)
    {
        _evaluationType = evaluationType;

        _t = 0;
        _dt = 1f / duration;

        if (_iteratorType == MonoUpdateType.FixedUpdate)
        {
            _dt *= Time.fixedDeltaTime;
        }

        _baseVector3Value = from;
        _targetVector3Value = to;

        _onEvaluationCompleted = onEvaluationCompleted;

        _iterating = true;

        return this;
    }

    public Evaluator Setup(Vector3 from, Transform to, float duration, EvaluationType evaluationType, Action onEvaluationCompleted = null)
    {
        _evaluationType = evaluationType;

        _t = 0;
        _dt = 1f / duration;

        if (_iteratorType == MonoUpdateType.FixedUpdate)
        {
            _dt *= Time.fixedDeltaTime;
        }

        _baseVector3Value = from;
        _targetTransform = to;

        _onEvaluationCompleted = onEvaluationCompleted;

        _iterating = true;

        return this;
    }

    public void Iterate(ref float value)
    {
        if (_iterating)
        {
            if (_iteratorType == MonoUpdateType.FixedUpdate)
            {
                _t = Mathf.Clamp01(_t + _dt);
            }
            else
            {
                _t = Mathf.Clamp01(_t + _dt * Time.deltaTime);
            }

            switch (_evaluationType)
            {
                case EvaluationType.Linear:
                    _floatValue = angular ? Mathf.LerpAngle(_baseFloatValue, _targetFloatValue, _t) : Mathf.Lerp(_baseFloatValue, _targetFloatValue, _t);
                    break;

                case EvaluationType.Smooth:
                    _floatValue = angular ? Mathf.LerpAngle(_baseFloatValue, _targetFloatValue, Mathf.Sin(_t * 1.5708f)) : Mathf.Lerp(_baseFloatValue, _targetFloatValue, Mathf.Sin(_t * 1.5708f));
                    break;
            }

            value = _floatValue;

            if (_t == 1f)
            {
                if (_onEvaluationCompleted != null)
                {
                    _onEvaluationCompleted();

                    _onEvaluationCompleted = null;
                }

                _iterating = false;
            }
        }
    }

    public void Iterate(ref Vector3 value)
    {
        if (_iterating)
        {
            if (_iteratorType == MonoUpdateType.FixedUpdate)
            {
                _t = Mathf.Clamp01(_t + _dt);
            }
            else
            {
                _t = Mathf.Clamp01(_t + _dt * Time.deltaTime);
            }

            switch (_evaluationType)
            {
                case EvaluationType.Linear:

                    if (_targetTransform)
                    {
                        _vector3Value = Vector3.Lerp(_baseVector3Value, _targetTransform.position, _t);
                    }
                    else
                    {
                        _vector3Value = Vector3.Lerp(_baseVector3Value, _targetVector3Value, _t);
                    }

                    break;

                case EvaluationType.Smooth:

                    if (_targetTransform)
                    {
                        _vector3Value = Vector3.Lerp(_baseVector3Value, _targetTransform.position, Mathf.Sin(_t * 3.1416f));
                    }
                    else
                    {
                        _vector3Value = Vector3.Lerp(_baseVector3Value, _targetVector3Value, Mathf.Sin(_t * 3.1416f));
                    }

                    break;
            }

            value = _vector3Value;

            if (_t == 1f)
            {
                if (_onEvaluationCompleted != null)
                {
                    _onEvaluationCompleted();

                    _onEvaluationCompleted = null;
                }

                _targetTransform = null;

                _iterating = false;
            }
        }
    }
}
