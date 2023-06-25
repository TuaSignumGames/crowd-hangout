using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIStatBoosterPoint : MonoBehaviour
{
    public static UIStatBoosterPoint Instance;

    public Animation waveAnimation;
    [Space]
    public RectTransform motionSpeedFieldTransform;
    public RectTransform damageRateFieldTransform;
    [Space]
    public Text motionSpeedFieldText;
    public Text damageRateFieldText;
    [Space]
    public float pulseMagnitude;
    public float retrievalFactor;
    [Space]
    public float fieldMotionLerpingFactor;

    private RectTransform rectTransform;

    private Vector2 motionSpeedFieldOffset;
    private Vector2 damageRateFieldOffset;

    private float fieldSize;
    private float fieldSizeLimit;

    private float motionSpeedFieldValue;
    private float damageRateFieldValue;

    private float referenceValue;

    private bool isVisible;

    public RectTransform Transform => rectTransform;

    public void Awake()
    {
        Instance = this;

        rectTransform = GetComponent<RectTransform>();

        motionSpeedFieldTransform.parent = transform.parent;
        damageRateFieldTransform.parent = transform.parent;

        motionSpeedFieldOffset = motionSpeedFieldTransform.anchoredPosition - rectTransform.anchoredPosition;
        damageRateFieldOffset = damageRateFieldTransform.anchoredPosition - rectTransform.anchoredPosition;

        fieldSizeLimit = 1f + pulseMagnitude;

        SetVisibleImmediate(false);
    }

    private void FixedUpdate()
    {
        if (referenceValue == 0)
        {
            motionSpeedFieldTransform.anchoredPosition = rectTransform.anchoredPosition + motionSpeedFieldOffset;
            damageRateFieldTransform.anchoredPosition = rectTransform.anchoredPosition + damageRateFieldOffset;
        }
        else
        {
            motionSpeedFieldTransform.anchoredPosition = Vector3.Lerp(motionSpeedFieldTransform.anchoredPosition, rectTransform.anchoredPosition + motionSpeedFieldOffset, fieldMotionLerpingFactor);
            damageRateFieldTransform.anchoredPosition = Vector3.Lerp(damageRateFieldTransform.anchoredPosition, rectTransform.anchoredPosition + damageRateFieldOffset, fieldMotionLerpingFactor);
        }

        referenceValue = Mathf.Max(motionSpeedFieldValue, damageRateFieldValue);

        fieldSize = Mathf.Lerp(fieldSize, isVisible && referenceValue > 0 ? 1f : 0, retrievalFactor);

        motionSpeedFieldTransform.localScale = new Vector3(fieldSize, fieldSize, fieldSize);
        damageRateFieldTransform.localScale = new Vector3(fieldSize, fieldSize, fieldSize);
    }

    public void Pulse()
    {
        waveAnimation.Play();

        fieldSize = Mathf.Clamp(fieldSize + pulseMagnitude, 0, fieldSizeLimit);
    }

    public void SetMotionSpeedValue(float value)
    {
        motionSpeedFieldValue = value;

        motionSpeedFieldText.text = $"+{value}%";
    }

    public void SetDamageRateValue(float value)
    {
        damageRateFieldValue = value;

        damageRateFieldText.text = $"+{value}%";
    }

    public void SetVisible(bool isVisible)
    {
        this.isVisible = isVisible;

        waveAnimation.gameObject.SetActive(isVisible);

        if (isVisible)
        {
            Reset();

            gameObject.SetActive(true);
        }
    }

    public void SetVisibleImmediate(bool isVisible)
    {
        this.isVisible = isVisible;

        Reset();

        waveAnimation.gameObject.SetActive(isVisible);

        if (isVisible)
        {
            motionSpeedFieldTransform.anchoredPosition = rectTransform.anchoredPosition + motionSpeedFieldOffset;
            damageRateFieldTransform.anchoredPosition = rectTransform.anchoredPosition + damageRateFieldOffset;

            motionSpeedFieldTransform.localScale = Vector3.one;
            damageRateFieldTransform.localScale = Vector3.one;
        }

        gameObject.SetActive(isVisible);
    }

    public void Reset()
    {
        fieldSize = 0;
        referenceValue = 0;

        SetMotionSpeedValue(0);
        SetDamageRateValue(0);

        waveAnimation.Stop();

        motionSpeedFieldTransform.localScale = Vector3.zero;
        damageRateFieldTransform.localScale = Vector3.zero;
    }
}
