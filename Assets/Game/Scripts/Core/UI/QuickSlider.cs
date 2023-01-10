using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class QuickSlider : MonoBehaviour
{
    public string title;
    [Space]
    public Text titleText;
    public Text valueText;
    [Space]
    public GameObject resetIcon;
    [Space]
    public Slider slider;
    [Space]
    public UnityEvent<float> OnValueChanged;

    private float defaultValue;

    private bool isInitialized;

    public void Initialize(float initialValue, float defaultValue = 0)
    {
        this.defaultValue = defaultValue;

        SetValue(initialValue);

        isInitialized = true;
    }

    public void SetValue(float value)
    {
        slider.value = value;

        UpdateValue(value);
    }

    public void ResetValue()
    {
        SetValue(defaultValue);
    }

    public void UpdateValue(float value)
    {
        valueText.text = value.ToString("N1");

        resetIcon.SetActive(value != defaultValue);

        if (isInitialized)
        {
            OnValueChanged.Invoke(value);
        }
    }

    public void OnValidate()
    {
        if (titleText)
        {
            titleText.text = title;
        }
    }
}
