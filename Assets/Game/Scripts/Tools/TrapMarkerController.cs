using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrapMarkerController : MarkerController
{
    [Space]
    public SpriteRenderer bodySprite;
    public Color dangerColor;
    public Color advantageColor;

    public void Show(bool isAdvantage, Action callback = null)
    {
        bodySprite.color = isAdvantage ? advantageColor : dangerColor;

        Show(callback);
    }
}
