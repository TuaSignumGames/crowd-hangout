using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum InclusionType { Inside, Outside }

public class MarkerController : MonoBehaviour
{
    public Transform pivotTransform;
    public bool autoInitialize;
    [Space]
    public Transform graphicsContainer;
    public Animation animationComponent;
    [Space]
    public Axis planeAxis;
    public InclusionType visibilityCondition;
    public float visibilityDistance = 50f;
    public bool switchByDistance;
    [Space]
    public float transitionTime = 0.1f;

    public Action OnShown;
    public Action OnHiden;

    private AnimationPlayer _animationPlayer;

    private Transform _viewerTransform;

    private Evaluator _evaluator;

    private Vector3 _pivotOffset;

    private float _transitionFactor;

    private float _baseScale;
    private float _currentScale;

    private float _baseDistance;

    private float _distanceFactor;

    private float _maxScaleValue;

    private bool _isInitialized;
    private bool _inVisibleRange;
    private bool _isVisible;

    public bool Visible => _isVisible;

    public const float scaleFactor = 3f;

    private void Start()
    {
        if (autoInitialize)
        {
            Initialize(visibilityDistance);
        }
    }

    public void Initialize(float visibilityDistance)
    {
        if (!_isInitialized)
        {
            _viewerTransform = Camera.main.transform;

            _evaluator = new Evaluator(MonoUpdateType.FixedUpdate);

            this.visibilityDistance = visibilityDistance;

            if (pivotTransform)
            {
                _pivotOffset = transform.position - pivotTransform.position;
            }

            if (_viewerTransform)
            {
                _baseDistance = (_viewerTransform.position - transform.position).GetPlanarMagnitude(planeAxis);
            }

            if (animationComponent)
            {
                _animationPlayer = new AnimationPlayer(animationComponent);
            }

            _baseScale = transform.localScale.x;

            graphicsContainer.gameObject.SetActive(false);

            transform.localScale = Vector3.zero;

            OnShown += () => graphicsContainer.gameObject.SetActive(true);
            OnHiden += () => graphicsContainer.gameObject.SetActive(false);

            _maxScaleValue = 0;

            _isInitialized = true;
        }
    }

    public void Initialize()
    {
        Initialize(visibilityDistance);
    }

    protected virtual void FixedUpdate()
    {
        if (_isInitialized)
        {
            _evaluator.Iterate(ref _transitionFactor);

            if (switchByDistance)
            {
                _distanceFactor = GetDistanceFactor();

                _inVisibleRange = visibilityCondition == InclusionType.Inside ? _distanceFactor <= 1f : _distanceFactor > 1f;

                if (_inVisibleRange)
                {
                    Show();
                }
                else
                {
                    Hide();
                }
            }

            if (_isVisible)
            {
                _distanceFactor = GetDistanceFactor();

                if (pivotTransform)
                {
                    transform.position = pivotTransform.position + _pivotOffset;
                }

                _currentScale = _baseScale * Mathf.Clamp(1f + _distanceFactor * scaleFactor, 1f, 10f) * _transitionFactor;

                transform.localScale = new Vector3(_currentScale, _currentScale, _currentScale);

                if (_viewerTransform)
                {
                    transform.forward = _viewerTransform.position - transform.position;
                }
            }

            /*
            if (pivotTransform == null)
            {
                print($"({(_viewerTransform.position - transform.position).GetPlanarMagnitude(planeAxis)}) / {visibilityDistance}");
            }
            */
        }
    }

    public void Show(Action callback = null)
    {
        if (_transitionFactor == 0)
        {
            ShowProcessing(callback);
        }
    }

    public void Hide(Action callback = null)
    {
        if (_transitionFactor == 1f)
        {
            HideProcessing(callback);
        }
    }

    public virtual void ShowProcessing(Action callback = null)
    {
        _isVisible = true;

        _evaluator.Setup(0, 1f, transitionTime, EvaluationType.Linear);

        if (callback != null)
        {
            callback();
        }

        if (OnShown != null)
        {
            OnShown();
        }
    }

    public virtual void HideProcessing(Action callback = null)
    {
        _evaluator.Setup(1f, 0, transitionTime, EvaluationType.Linear, () => { if (callback != null) callback(); if (OnHiden != null) OnHiden(); _isVisible = false; });
    }

    public void PlayAnimation(int clipIndex = 0)
    {
        if (animationComponent)
        {
            _animationPlayer.Play(clipIndex);
        }
    }

    private float GetDistanceFactor()
    {
        return (_viewerTransform.position - transform.position).GetPlanarMagnitude(planeAxis) / visibilityDistance;
    }
}
