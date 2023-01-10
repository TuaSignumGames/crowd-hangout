using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class UICurrencyIncrement : UIElement
{
    [Space]
    public RectTransform rectTransform;
    public Text incrementText;
    [Space]
    public Vector2 offsetRange;

    private Vector2 initialPosition;

    public override void Awake()
    {
        base.Awake();

        initialPosition = rectTransform.anchoredPosition;
    }

    public void Show(int increment, float duration = 1f)
    {
        incrementText.text = $"+{increment}";

        rectTransform.anchoredPosition = initialPosition + new Vector2(Random.Range(-offsetRange.x, offsetRange.x), Random.Range(-offsetRange.y, offsetRange.y));

        Show();

        DOVirtual.DelayedCall(duration, () => Hide());
    }
}
