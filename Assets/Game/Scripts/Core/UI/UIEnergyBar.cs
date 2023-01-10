using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIEnergyBar : WorldUIElement
{
    public static UIEnergyBar Instance;

    [Space]
    [SerializeField] private Animation _animationComponent;
    [Space]
    [SerializeField] private Image _backFilling;
    [SerializeField] private Image _frontFilling;
    [SerializeField] private Image _chargeOverlay;
    [Space]
    [SerializeField] private float _frontFillingDelay;
    [SerializeField] private float _fillingLerpFactor;

    private AnimationPlayer _animationPlayer;

    private float _energyValue;
    private float _targetFillingValue;

    private float _valueApplyingTime;

    private bool _isFull;

    protected override void Awake()
    {
        base.Awake();

        Instance = this;

        _animationPlayer = new AnimationPlayer(_animationComponent);

        GameManager.OnGameSceneLoaded += Reset;

        Reset();
    }

    protected override void LateUpdate()
    {
        base.LateUpdate();

        if (_isInitialized && Enabled)
        {
            if (_targetFillingValue - _backFilling.fillAmount > 0.00001f)
            {
                _backFilling.fillAmount = Mathf.Lerp(_backFilling.fillAmount, _targetFillingValue, _fillingLerpFactor);
            }
            else
            {
                _additionalScaleMultiplier = 1f;
            }

            if (Time.timeSinceLevelLoad > _valueApplyingTime)
            {
                if (_targetFillingValue - _frontFilling.fillAmount > 0.00001f)
                {
                    _frontFilling.fillAmount = Mathf.Lerp(_frontFilling.fillAmount, _targetFillingValue, _fillingLerpFactor);
                }
            }
        }
    }

    public void SetEnergyValue(float value)
    {
        _energyValue = value;
        _targetFillingValue = value * 0.5f;

        if (value < 1f)
        {
            _additionalScaleMultiplier = 1.2f;

            _valueApplyingTime = Time.timeSinceLevelLoad + _frontFillingDelay;
        }
        else
        {
            if (!_isFull)
            {
                _isFull = true;

                _animationPlayer.Play(0, () => _animationPlayer.Play(1));
            }
        }
    }

    public void Consume()
    {
        if (_isFull)
        {
            _animationPlayer.Play(2, Reset);
        }
    }

    private void Reset()
    {
        _isFull = false;

        _energyValue = 0;
        _targetFillingValue = 0;

        _backFilling.fillAmount = 0;
        _frontFilling.fillAmount = 0;

        _backFilling.gameObject.SetActive(true);
        _frontFilling.gameObject.SetActive(true);
        _chargeOverlay.gameObject.SetActive(false);
    }
}
