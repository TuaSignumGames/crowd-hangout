using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldUIElement : MonoBehaviour
{
    public Transform targetTransform;
    public Transform viewerTransform;
    [Space]
    public Transform contentTransform;
    [Space]
    public Vector2 pivotOffset;
    public float motionLerpingFactor;
    public float scalingLerpingFactor;
    public bool affectOffsetByScale;
    [Space]
    public FollowerData[] followers;
    [Space]
    public Axis worldUpAxis;
    public float referenceDistance;
    [Space]
    public float appearingDistance;
    public float disappearingDistance;
    public bool switchByDistance;
    [Space]
    public float transitionDuration;
    [Space]
    public AnimationCurve scaleByDistanceCurve;

    public Action OnShown;
    public Action OnHiden;

    protected TransformEvaluator _transformEvaluator;

    protected float _distance;
    protected float _normalizedDistance;

    protected float _contentScale;
    protected float _additionalScaleMultiplier = 1f;

    protected float _targetDotProduct;

    protected bool _isShowing;
    protected bool _isHiding;

    protected bool _isInitialized;

    public bool Enabled => contentTransform.gameObject.activeSelf;

    protected virtual void Awake()
    {
        if (targetTransform && viewerTransform)
        {
            Initialize(targetTransform, viewerTransform);
        }
    }

    public virtual void Initialize(Transform targetTransform = null, Transform viewerTransform = null)
    {
        contentTransform.gameObject.SetActive(false);

        if (targetTransform)
        {
            this.targetTransform = targetTransform;
        }

        if (viewerTransform)
        {
            this.viewerTransform = viewerTransform;
        }

        _transformEvaluator = new TransformEvaluator(transform, MonoUpdateType.FixedUpdate);

        transform.localScale = Vector3.zero;

        OnShown = null;
        OnHiden = null;

        _isShowing = contentTransform.gameObject.activeInHierarchy;
        _isHiding = !_isShowing;

        _isInitialized = true;

        HideImmediate();
    }

    protected virtual void FixedUpdate()
    {
        if (_isInitialized && Enabled)
        {
            _transformEvaluator.Iterate();

            if (targetTransform)
            {
                if (switchByDistance)
                {
                    _normalizedDistance = Mathf.InverseLerp(appearingDistance, disappearingDistance, _distance);
                }
                else
                {
                    _normalizedDistance = _distance / referenceDistance;
                }

                _contentScale = Mathf.Lerp(_contentScale, scaleByDistanceCurve.Evaluate(_normalizedDistance) * _additionalScaleMultiplier, scalingLerpingFactor);

                if (affectOffsetByScale)
                {
                    transform.position = Vector3.Lerp(transform.position, Camera.main.WorldToScreenPoint(targetTransform.position) + new Vector3(pivotOffset.x, pivotOffset.y, 0) * _contentScale, motionLerpingFactor);
                }
                else
                {
                    transform.position = Vector3.Lerp(transform.position, Camera.main.WorldToScreenPoint(targetTransform.position) + new Vector3(pivotOffset.x, pivotOffset.y, 0), motionLerpingFactor);
                }

                contentTransform.localScale = new Vector3(_contentScale, _contentScale, _contentScale);

                if (followers.Length > 0)
                {
                    for (int i = 0; i < followers.Length; i++)
                    {
                        followers[i].transform.position = transform.position + followers[i].Offset * _contentScale;
                        followers[i].contentTransform.localScale = new Vector3(_contentScale, _contentScale, _contentScale);
                    }
                }
            }
        }
    }

    protected virtual void LateUpdate()
    {
        if (_isInitialized)
        {
            if (targetTransform && viewerTransform)
            {
                _distance = (targetTransform.position - viewerTransform.position).GetPlanarMagnitude(worldUpAxis);

                if (switchByDistance)
                {
                    _targetDotProduct = Vector3.Dot(targetTransform.position - viewerTransform.position, targetTransform.forward);

                    //print($"{_distance} / {disappearingDistance}");

                    if (_distance < disappearingDistance || _distance > appearingDistance || _targetDotProduct < 0)
                    {
                        TryHide();
                    }
                    else if (_distance < appearingDistance)
                    {
                        TryShow();
                    }
                }
            }
            else
            {
                TryHide();
            }
        }
    }

    public void TryShow(Action callback = null)
    {
        if (!_isShowing)
        {
            _isShowing = true;

            transform.position = Camera.main.WorldToScreenPoint(targetTransform.position) + new Vector3(pivotOffset.x, pivotOffset.y, 0) * (affectOffsetByScale ? _contentScale : 1f);

            contentTransform.gameObject.SetActive(true);

            _transformEvaluator.Scale(Vector3.one, transitionDuration, EvaluationType.Smooth, () => { if (callback != null) callback(); if (OnShown != null) OnShown(); _isHiding = false; });
        }
    }

    public void TryHide(Action callback = null)
    {
        if (!_isHiding)
        {
            _isHiding = true;

            _transformEvaluator.Scale(Vector3.zero, transitionDuration, EvaluationType.Smooth, () => { if (callback != null) callback(); contentTransform.gameObject.SetActive(false); _isShowing = false; });

            if (OnHiden != null)
            {
                OnHiden();
            }
        }
    }

    public void ShowImmediate()
    {
        _isShowing = true;
        _isHiding = false;

        contentTransform.gameObject.SetActive(true);

        if (OnShown != null)
        {
            OnShown();
        }
    }

    public void HideImmediate()
    {
        _isShowing = false;
        _isHiding = true;

        contentTransform.gameObject.SetActive(false);

        if (OnHiden != null)
        {
            OnHiden();
        }
    }

    [Serializable]
    public struct FollowerData
    {
        [HideInInspector]
        public string name;

        public Transform transform;
        public Transform contentTransform;
        public Vector2 offset;

        public Vector3 Offset => new Vector3(offset.x, offset.y, 0);

        public bool IsActive => transform.gameObject.activeSelf;
    }
}
