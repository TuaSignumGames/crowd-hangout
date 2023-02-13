using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformEvaluator
{
    private Transform _transform;

    private MonoUpdateType _iteratorType;

    private Vector3 _position;
    private Vector3 _eulerAngles;

    private Vector3 _localPosition;
    private Vector3 _localEulerAngles;
    private Vector3 _localScale;

    private Vector3 _lerpingBasePosition;
    private Vector3 _lerpingBaseEulerAngles;
    private Vector3 _lerpingBaseLocalScale;

    private Vector3 _velocity;
    private Vector3 _angularVelocity;

    private Vector3 _angularDeceleration;

    private Evaluator3 _translationEvaluator;
    private Evaluator3 _rotationEvaluator;

    private Evaluator3 _localTranslationEvaluator;
    private Evaluator3 _localRotationEvaluator;
    private Evaluator3 _localScaleEvaluator;

    private Evaluator _dynamicTransitionEvaluator;

    private Vector3Bool _translationMatrix;
    private Vector3Bool _rotationMatrix;

    private Vector3Bool _localTranslationMatrix;
    private Vector3Bool _localRotationMatrix;
    private Vector3Bool _localScaleMatrix;

    private Action _onTranslationCompleted;
    private Action _onRotationCompleted;
    private Action _onLocalTranslationCompleted;
    private Action _onLocalRotationCompleted;
    private Action _onLocalScalingCompleted;
    private Action _onDynamicTranslationCompleted;
    private Action _onThrowingCompleted;
    private Action _onSpinningCompleted;

    private float _processingDelay;
    private float _processingDelaySetupTime;

    private float _gravityMagnitude;

    private float _throwFinishTime;
    private float _spinFinishTime;

    //private float _evaluationFactor;

    private bool _isWorldSpaceTranslating;
    private bool _isWorldSpaceRotating;

    private bool _isLocalSpaceTranslating;
    private bool _isLocalSpaceRotating;
    private bool _isLocalSpaceScaling;

    private bool _isWorldSpaceDynamicTranslating;

    private bool _isTransformLerping;

    private bool _isThrowing;
    private bool _isSpinning;

    public Action OnEvaluationCompleted;

    //public float EvaluationFactor;

    public bool disableOnZeroScale;

    public bool Evaluating => _isWorldSpaceTranslating || _isWorldSpaceRotating || _isLocalSpaceTranslating || _isLocalSpaceRotating || _isLocalSpaceScaling || _isThrowing || _isSpinning;

    public TransformEvaluator(MonoUpdateType updateType)
    {
        _iteratorType = updateType;
    }

    public TransformEvaluator(Transform transform, MonoUpdateType updateType)
    {
        _transform = transform;

        _iteratorType = updateType;
    }

    public void Update()
    {
        if (Time.timeSinceLevelLoad > _processingDelaySetupTime + _processingDelay)
        {
            if (_isWorldSpaceTranslating)
            {
                ProcessTranslation();
            }

            if (_isWorldSpaceDynamicTranslating)
            {
                ProcessDynamicTranslation();
            }

            if (_isLocalSpaceTranslating)
            {
                ProcessLocalTranslation();
            }

            if (_isWorldSpaceRotating)
            {
                ProcessRotation();
            }

            if (_isLocalSpaceRotating)
            {
                ProcessLocalRotation();
            }

            if (_isLocalSpaceScaling)
            {
                ProcessLocalScale();
            }

            if (_isThrowing)
            {
                ProcessThrowing();
            }

            if (_isSpinning)
            {
                ProcessSpinning();
            }
        }

        if (!Evaluating)
        {
            if (OnEvaluationCompleted != null)
            {
                OnEvaluationCompleted();

                OnEvaluationCompleted = null;
            }

            if (disableOnZeroScale && _transform.localScale == Vector3.zero)
            {
                _transform.gameObject.SetActive(false);
            }
        }
    }

    public TransformEvaluator SetTransform(Transform transform)
    {
        _transform = transform;

        return this;
    }

    public void Translate(Vector3 targetPosition, float duration, EvaluationType mode, Action completionCallback = null)
    {
        _translationMatrix = !Vector3Bool.Compare(_transform.position, targetPosition);

        if (_translationMatrix != Vector3Bool.zero)
        {
            if (_translationEvaluator == null)
            {
                _translationEvaluator = new Evaluator3(_iteratorType);
            }

            if (_translationMatrix.x)
            {
                _translationEvaluator.x.Setup(_transform.position.x, targetPosition.x, duration, mode);
            }

            if (_translationMatrix.y)
            {
                _translationEvaluator.y.Setup(_transform.position.y, targetPosition.y, duration, mode);
            }

            if (_translationMatrix.z)
            {
                _translationEvaluator.z.Setup(_transform.position.z, targetPosition.z, duration, mode);
            }

            _position = _transform.position;

            if (completionCallback != null)
            {
                //_onTranslationCompleted = completionCallback;
                OnEvaluationCompleted += completionCallback;
            }

            _isWorldSpaceTranslating = true;
        }
    }

    public void TranslateDynamic(Transform targetTransform, float duration, EvaluationType mode, Action completionCallback = null)
    {
        //_translationMatrix = !Vector3Bool.Compare(_transform.position, targetTransform.position);

        //if (_translationMatrix != Vector3Bool.zero)
        {
            if (_translationEvaluator == null)
            {
                _dynamicTransitionEvaluator = new Evaluator(_iteratorType);
            }

            _dynamicTransitionEvaluator.Setup(_transform.position, targetTransform, duration, mode, completionCallback);

            _position = _transform.position;

            if (completionCallback != null)
            {
                OnEvaluationCompleted += completionCallback;
            }

            _isWorldSpaceDynamicTranslating = true;
        }
    }

    public void TranslateLocal(Vector3 targetLocalPosition, float duration, EvaluationType mode, Action completionCallback = null)
    {
        _localTranslationMatrix = !Vector3Bool.Compare(_transform.localPosition, targetLocalPosition);

        if (_localTranslationMatrix != Vector3Bool.zero)
        {
            if (_localTranslationEvaluator == null)
            {
                _localTranslationEvaluator = new Evaluator3(_iteratorType);
            }

            if (_localTranslationMatrix.x)
            {
                _localTranslationEvaluator.x.Setup(_transform.localPosition.x, targetLocalPosition.x, duration, mode);
            }

            if (_localTranslationMatrix.y)
            {
                _localTranslationEvaluator.y.Setup(_transform.localPosition.y, targetLocalPosition.y, duration, mode);
            }

            if (_localTranslationMatrix.z)
            {
                _localTranslationEvaluator.z.Setup(_transform.localPosition.z, targetLocalPosition.z, duration, mode);
            }

            _localPosition = _transform.localPosition;

            if (completionCallback != null)
            {
                //_onLocalTranslationCompleted = completionCallback;
                OnEvaluationCompleted += completionCallback;
            }

            _isLocalSpaceTranslating = true;
        }
    }

    public void TranslateLocal(Axis axis, float distance, float duration, EvaluationType mode, Action completionCallback = null)
    {
        TranslateLocal(_transform.localPosition + (axis == Axis.X ? new Vector3(distance, 0, 0) : (axis == Axis.Y ? new Vector3(0, distance, 0) : new Vector3(0, 0, distance))), duration, mode);
    }

    public void Rotate(Vector3 targetEulerAngles, float duration, EvaluationType mode, Action completionCallback = null)
    {
        _rotationMatrix = !Vector3Bool.Compare(_transform.eulerAngles, targetEulerAngles);

        if (_rotationMatrix != Vector3Bool.zero)
        {
            if (_rotationEvaluator == null)
            {
                _rotationEvaluator = new Evaluator3(_iteratorType);
            }

            if (_rotationMatrix.x)
            {
                _rotationEvaluator.x.Setup(_transform.eulerAngles.x, targetEulerAngles.x, duration, mode).angular = true;
            }

            if (_rotationMatrix.y)
            {
                _rotationEvaluator.y.Setup(_transform.eulerAngles.y, targetEulerAngles.y, duration, mode).angular = true;
            }

            if (_rotationMatrix.z)
            {
                _rotationEvaluator.z.Setup(_transform.eulerAngles.z, targetEulerAngles.z, duration, mode).angular = true;
            }

            _eulerAngles = _transform.eulerAngles;

            if (completionCallback != null)
            {
                //_onRotationCompleted = completionCallback;
                OnEvaluationCompleted += completionCallback;
            }

            _isWorldSpaceRotating = true;
        }
    }

    public void RotateLocal(Vector3 targetLocalEulerAngles, float duration, EvaluationType mode, Action completionCallback = null)
    {
        _localRotationMatrix = !Vector3Bool.Compare(_transform.localEulerAngles, targetLocalEulerAngles);

        if (_localRotationMatrix != Vector3Bool.zero)
        {
            if (_localRotationEvaluator == null)
            {
                _localRotationEvaluator = new Evaluator3(_iteratorType);
            }

            if (_localRotationMatrix.x)
            {
                _localRotationEvaluator.x.Setup(_transform.localEulerAngles.x, targetLocalEulerAngles.x, duration, mode).angular = true;
            }

            if (_localRotationMatrix.y)
            {
                _localRotationEvaluator.y.Setup(_transform.localEulerAngles.y, targetLocalEulerAngles.y, duration, mode).angular = true;
            }

            if (_localRotationMatrix.z)
            {
                _localRotationEvaluator.z.Setup(_transform.localEulerAngles.z, targetLocalEulerAngles.z, duration, mode).angular = true;
            }

            _localEulerAngles = _transform.localEulerAngles;

            if (completionCallback != null)
            {
                //_onLocalRotationCompleted = completionCallback;
                OnEvaluationCompleted += completionCallback;
            }

            _isLocalSpaceRotating = true;
        }
    }

    public void RotateAround(Vector3 axis, float angle, float duration, EvaluationType mode, Action completionCallback = null)
    {
        Rotate(_transform.eulerAngles + axis.normalized * angle, duration, mode, completionCallback);
    }

    public void RotateAroundLocal(Axis axis, float angle, float duration, EvaluationType mode, Action completionCallback = null)
    {
        RotateLocal(_transform.localEulerAngles + (axis == Axis.X ? new Vector3(angle, 0, 0) : (axis == Axis.Y ? new Vector3(0, angle, 0) : new Vector3(0, 0, angle))), duration, mode, completionCallback);
    }

    public void Scale(Vector3 targetLocalScale, float duration, EvaluationType mode, Action completionCallback = null)
    {
        _localScaleMatrix = !Vector3Bool.Compare(_transform.localScale, targetLocalScale);

        if (_localScaleMatrix != Vector3Bool.zero)
        {
            if (_localScaleEvaluator == null)
            {
                _localScaleEvaluator = new Evaluator3(_iteratorType);
            }

            if (_localScaleMatrix.x)
            {
                _localScaleEvaluator.x.Setup(_transform.localScale.x, targetLocalScale.x, duration, mode);
            }

            if (_localScaleMatrix.y)
            {
                _localScaleEvaluator.y.Setup(_transform.localScale.y, targetLocalScale.y, duration, mode);
            }

            if (_localScaleMatrix.z)
            {
                _localScaleEvaluator.z.Setup(_transform.localScale.z, targetLocalScale.z, duration, mode);
            }

            _localScale = _transform.localScale;

            if (completionCallback != null)
            {
                //_onLocalScalingCompleted = completionCallback;
                OnEvaluationCompleted += completionCallback;
            }

            _isLocalSpaceScaling = true;
        }
    }

    public void ScaleBy(Vector3 axis, float multiplier, float duration, EvaluationType mode, Action completionCallback = null)
    {
        Scale(_transform.localScale + axis.normalized * multiplier, duration, mode, completionCallback);
    }

    public void Lerp(Transform targetTransform, float t)
    {
        if (!_isTransformLerping)
        {
            _isTransformLerping = true;

            _lerpingBasePosition = _transform.position;
            _lerpingBaseEulerAngles = _transform.eulerAngles;
            _lerpingBaseLocalScale = _transform.localScale;
        }
        else
        {
            _transform.position = Vector3.Lerp(_lerpingBasePosition, targetTransform.position, t);
            _transform.eulerAngles = Vector3.Lerp(_lerpingBaseEulerAngles, targetTransform.eulerAngles, t);
            _transform.localScale = Vector3.Lerp(_lerpingBaseLocalScale, targetTransform.localScale, t);

            if (t >= 1f)
            {
                _isTransformLerping = false;
            }
        }
    }

    public void Throw(Vector3 impulse, float gravity, float duration, Action completionCallback = null)
    {
        _gravityMagnitude = gravity;

        _velocity = impulse;

        _throwFinishTime = Time.timeSinceLevelLoad + duration;

        if (completionCallback != null)
        {
            OnEvaluationCompleted += completionCallback;
        }

        _isThrowing = true;
    }

    public void Spin(Vector3 angularImpulse, float duration, Action completionCallback = null)
    {
        _angularVelocity = angularImpulse;

        _angularDeceleration = angularImpulse / duration;

        _spinFinishTime = Time.timeSinceLevelLoad + duration;

        if (completionCallback != null)
        {
            OnEvaluationCompleted += completionCallback;
        }

        _isSpinning = true;
    }

    public void SetDelay(float delay)
    {
        _processingDelay = delay;

        _processingDelaySetupTime = Time.timeSinceLevelLoad;
    }

    private void ProcessTranslation()
    {
        if (_translationMatrix.x)
        {
            _translationEvaluator.x.Iterate(ref _position.x);
        }
        if (_translationMatrix.y)
        {
            _translationEvaluator.y.Iterate(ref _position.y);
        }
        if (_translationMatrix.z)
        {
            _translationEvaluator.z.Iterate(ref _position.z);
        }

        _transform.position = _position;

        _isWorldSpaceTranslating = _translationEvaluator.Iterating;

        if (!_isWorldSpaceTranslating && _onTranslationCompleted != null)
        {
            _onTranslationCompleted();

            _onTranslationCompleted = null;
        }
    }

    private void ProcessDynamicTranslation()
    {
        _dynamicTransitionEvaluator.Iterate(ref _position);

        _transform.position = _position;

        _isWorldSpaceDynamicTranslating = _dynamicTransitionEvaluator.Iterating;

        if (!_isWorldSpaceDynamicTranslating && _onDynamicTranslationCompleted != null)
        {
            _onDynamicTranslationCompleted();

            _onDynamicTranslationCompleted = null;
        }
    }

    private void ProcessLocalTranslation()
    {
        if (_localTranslationMatrix.x)
        {
            _localTranslationEvaluator.x.Iterate(ref _localPosition.x);
        }
        if (_localTranslationMatrix.y)
        {
            _localTranslationEvaluator.y.Iterate(ref _localPosition.y);
        }
        if (_localTranslationMatrix.z)
        {
            _localTranslationEvaluator.z.Iterate(ref _localPosition.z);
        }

        _transform.localPosition = _localPosition;

        _isLocalSpaceTranslating = _localTranslationEvaluator.Iterating;

        if (!_isLocalSpaceTranslating && _onLocalTranslationCompleted != null)
        {
            _onLocalTranslationCompleted();

            _onLocalTranslationCompleted = null;
        }
    }

    private void ProcessRotation()
    {
        if (_rotationMatrix.x)
        {
            _rotationEvaluator.x.Iterate(ref _eulerAngles.x);
        }
        if (_rotationMatrix.y)
        {
            _rotationEvaluator.y.Iterate(ref _eulerAngles.y);
        }
        if (_rotationMatrix.z)
        {
            _rotationEvaluator.z.Iterate(ref _eulerAngles.z);
        }

        _transform.eulerAngles = _eulerAngles;

        _isWorldSpaceRotating = _rotationEvaluator.Iterating;

        if (!_isWorldSpaceRotating && _onRotationCompleted != null)
        {
            _onRotationCompleted();

            _onRotationCompleted = null;
        }
    }

    private void ProcessLocalRotation()
    {
        if (_localRotationMatrix.x)
        {
            _localRotationEvaluator.x.Iterate(ref _localEulerAngles.x);
        }
        if (_localRotationMatrix.y)
        {
            _localRotationEvaluator.y.Iterate(ref _localEulerAngles.y);
        }
        if (_localRotationMatrix.z)
        {
            _localRotationEvaluator.z.Iterate(ref _localEulerAngles.z);
        }

        _transform.localEulerAngles = _localEulerAngles;

        _isLocalSpaceRotating = _localRotationEvaluator.Iterating;

        if (!_isLocalSpaceRotating && _onLocalRotationCompleted != null)
        {
            _onLocalRotationCompleted();

            _onLocalRotationCompleted = null;
        }
    }

    private void ProcessLocalScale()
    {
        if (_localScaleMatrix.x)
        {
            _localScaleEvaluator.x.Iterate(ref _localScale.x);
        }
        if (_localScaleMatrix.y)
        {
            _localScaleEvaluator.y.Iterate(ref _localScale.y);
        }
        if (_localScaleMatrix.z)
        {
            _localScaleEvaluator.z.Iterate(ref _localScale.z);
        }

        _transform.localScale = _localScale;

        _isLocalSpaceScaling = _localScaleEvaluator.Iterating;

        if (!_isLocalSpaceScaling && _onLocalScalingCompleted != null)
        {
            _onLocalScalingCompleted();

            _onLocalScalingCompleted = null;
        }
    }

    private void ProcessThrowing()
    {
        if (Time.timeSinceLevelLoad < _throwFinishTime)
        {
            if (_iteratorType == MonoUpdateType.FixedUpdate)
            {
                _transform.position += _velocity * Time.fixedDeltaTime;

                _velocity += new Vector3(0, _gravityMagnitude * Time.fixedDeltaTime, 0);
            }
            else
            {
                _transform.position += _velocity * Time.deltaTime;

                _velocity += new Vector3(0, _gravityMagnitude * Time.deltaTime, 0);
            }
        }
        else
        {
            _isThrowing = false;

            if (_onThrowingCompleted != null)
            {
                _onThrowingCompleted();

                _onThrowingCompleted = null;
            }
        }
    }

    private void ProcessSpinning()
    {
        if (Time.timeSinceLevelLoad < _spinFinishTime)
        {
            if (_iteratorType == MonoUpdateType.FixedUpdate)
            {
                _transform.eulerAngles += _angularVelocity * Time.fixedDeltaTime;

                _angularVelocity -= _angularDeceleration * Time.fixedDeltaTime;
            }
            else
            {
                _transform.position += _velocity * Time.deltaTime;

                _angularVelocity -= _angularDeceleration * Time.deltaTime;
            }
        }
        else
        {
            _isSpinning = false;

            if (_onSpinningCompleted != null)
            {
                _onSpinningCompleted();

                _onSpinningCompleted = null;
            }
        }
    }

    private class Evaluator3
    {
        public Evaluator x;
        public Evaluator y;
        public Evaluator z;

        private float divider;

        public bool Iterating => x.Iterating || y.Iterating || z.Iterating;

        public Evaluator3(MonoUpdateType iteratorType)
        {
            x = new Evaluator(iteratorType);
            y = new Evaluator(iteratorType);
            z = new Evaluator(iteratorType);
        }

        public float GetAverageEvaluationFactor()
        {
            divider = 0;

            if (x.Iterating)
            {
                divider++;
            }
            if (y.Iterating)
            {
                divider++;
            }
            if (z.Iterating)
            {
                divider++;
            }

            return (x.EvaluationFactor + y.EvaluationFactor + z.EvaluationFactor) / divider;
        }
    }
}
