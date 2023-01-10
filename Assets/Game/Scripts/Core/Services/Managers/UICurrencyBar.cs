using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICurrencyBar : UIElement
{
    public static UICurrencyBar Instance;

    [Space]
    [SerializeField] private Text _amountText;

    private Evaluator _recounterEvaluator;

    private float _amountValue;

    public override void Awake()
    {
        base.Awake();

        Instance = this;

        _recounterEvaluator = new Evaluator(MonoUpdateType.LateUpdate);

        SetAmount(GameManager.Currency);
    }

    private void LateUpdate()
    {
        if (_recounterEvaluator.Iterating)
        {
            _recounterEvaluator.Iterate(ref _amountValue);

            _amountText.text = Mathf.FloorToInt(_amountValue).ToString("N0");
        }
    }

    public void SetAmount(float value)
    {
        _amountValue = value;

        _amountText.text = Mathf.FloorToInt(value).ToString("N0");
    }

    public void Recount(float targetValue, float unitsPerSecond = 50f, float durationLimit = 1.5f, Action onRecountCompleted = null)
    {
        _recounterEvaluator.Setup(_amountValue, targetValue, Mathf.Clamp(Mathf.Abs(targetValue - _amountValue) / unitsPerSecond, 0, durationLimit), EvaluationType.Smooth, onRecountCompleted);
    }
}
