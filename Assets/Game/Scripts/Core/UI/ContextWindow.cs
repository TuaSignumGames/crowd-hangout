using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContextWindow : MonoBehaviour, IInitializable
{
    [SerializeField] protected string _title = "New Context Window";
    [SerializeField] protected ContextWindowStateData _minimizedStateSettings;
    [SerializeField] protected ContextWindowReferences _references;

    public Action OnMaximized;
    public Action OnMinimized;

    protected ContextWindowStateData _maximizedStateSettings;

    protected ContextWindowStateData _currentStateSettings;

    protected float _currentWidth;
    protected float _currentHeight;

    protected bool _isMaximized;

    public float Width => _references.widthScalingTransform.rect.width;
    public float Height => _references.heightScalingTransform.rect.height;

    public virtual void Initialize()
    {
        InitializeMaximizedStateSettings();

        UpdateView();
    }

    public virtual void SwitchState()
    {
        _isMaximized = !_isMaximized;

        UpdateView();

        DebuggerManager.Instance.RefreshView();

        AppManager.Instance.PlayHaptic(MoreMountains.NiceVibrations.HapticTypes.LightImpact);
    }

    public virtual void UpdateView()
    {
        _currentStateSettings = _isMaximized ? _maximizedStateSettings : _minimizedStateSettings;

        switch (_currentStateSettings.widthScalingMode)
        {
            case ClampedValueType.Custom:
                _currentWidth = _currentStateSettings.customWidth;
                break;

            case ClampedValueType.Maximal:
                _currentWidth = _references.heightScalingTransform.rect.width;
                break;

            case ClampedValueType.Minimal:
                _currentWidth = 0;
                break;
        }

        switch (_currentStateSettings.heightScalingMode)
        {
            case ClampedValueType.Custom:
                _currentHeight = _currentStateSettings.customHeight;
                break;

            case ClampedValueType.Maximal:
                _currentHeight = DebuggerManager.Instance.GetViewFreeHeight();
                break;

            case ClampedValueType.Minimal:
                _currentHeight = _references.headerTransform.rect.height;
                break;
        }

        _references.widthScalingTransform.anchorMin = new Vector2(0, _isMaximized ? 0 : 1f);
        _references.widthScalingTransform.anchorMax = new Vector2(0, 1f);

        _references.widthScalingTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _currentWidth);
        _references.heightScalingTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _currentHeight);

        _references.titleText.alignment = _isMaximized ? TextAnchor.MiddleCenter : TextAnchor.MiddleLeft;

        _references.bodyContent.SetActive(_isMaximized);

        UpdateIndicators();

        if (_isMaximized)
        {
            if (OnMaximized != null)
            {
                OnMaximized();
            }
        }
        else
        {
            if (OnMinimized != null)
            {
                OnMinimized();
            }
        }
    }

    public void UpdateIndicators()
    {
        for (int i = 0; i < _references.indicators.Length; i++)
        {
            _references.indicators[i].SetActive(false);
        }

        for (int i = 0; i < _currentStateSettings.activeIndicators.Count; i++)
        {
            _currentStateSettings.activeIndicators[i].SetActive(true);
        }
    }

    protected void InitializeMaximizedStateSettings()
    {
        _maximizedStateSettings = new ContextWindowStateData();

        _maximizedStateSettings.activeIndicators = new List<UIIndicator>();

        for (int i = 0; i < _references.indicators.Length; i++)
        {
            if (_references.indicators[i].gameObject.activeSelf)
            {
                _maximizedStateSettings.activeIndicators.Add(_references.indicators[i]);
            }
        }

        _maximizedStateSettings.widthScalingMode = _references.widthScalingTransform.rect.width.IsEqualToFloat(_references.heightScalingTransform.rect.width) ? ClampedValueType.Maximal : ClampedValueType.Custom;
        _maximizedStateSettings.heightScalingMode = ClampedValueType.Custom;

        if (_maximizedStateSettings.widthScalingMode == ClampedValueType.Custom)
        {
            _maximizedStateSettings.customWidth = _references.widthScalingTransform.rect.width;
        }

        if (_maximizedStateSettings.heightScalingMode == ClampedValueType.Custom)
        {
            _maximizedStateSettings.customHeight = _references.heightScalingTransform.rect.height;
        }
    }

    protected void OnValidate()
    {
        if (_references.titleText)
        {
            _references.titleText.text = _title;
        }
    }

    public void Deinitialize()
    {
        throw new NotImplementedException();
    }

    [Serializable]
    protected struct ContextWindowStateData
    {
        public List<UIIndicator> activeIndicators;
        public ClampedValueType widthScalingMode;
        public ClampedValueType heightScalingMode;
        public float customWidth;
        public float customHeight;
    }

    [Serializable]
    protected struct ContextWindowReferences
    {
        public RectTransform widthScalingTransform;
        public RectTransform heightScalingTransform;
        public RectTransform headerTransform;
        [Space]
        public UIIndicator[] indicators;
        [Space]
        public Text titleText;
        public GameObject bodyContent;
    }
}
