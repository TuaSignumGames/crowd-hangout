using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIIndicator : MonoBehaviour
{
    [SerializeField] private UIIndicatorStateData[] _states;
    [SerializeField] private bool _isNumeric;
    [SerializeField] private bool _hideIfZero;
    [SerializeField] private Text _numberText;

    private int _numberValue;

    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive && (_isNumeric && _hideIfZero ? _numberValue > 0 : isActive));
    }

    public void SetState(IndicatorStateType stateType)
    {
        for (int i = 0; i < _states.Length; i++)
        {
            _states[i].stateIcon.SetActive(_states[i].stateType == stateType);
        }
    }

    public void SetState(int stateTypeIndex)
    {
        for (int i = 0; i < _states.Length; i++)
        {
            _states[i].stateIcon.SetActive(i == stateTypeIndex);
        }
    }

    public void SetValue(int value)
    {
        _numberValue = value;

        _numberText.text = value.ToString();
    }

    private void OnValidate()
    {
        for (int i = 0; i < _states.Length; i++)
        {
            _states[i].title = _states[i].stateType.ToString();
        }

        _numberText.gameObject.SetActive(_isNumeric);
    }

    [Serializable]
    private struct UIIndicatorStateData
    {
        [HideInInspector] public string title;

        public IndicatorStateType stateType;
        public GameObject stateIcon;
    }
}
