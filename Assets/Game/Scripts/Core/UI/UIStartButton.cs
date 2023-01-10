using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIStartButton : UIElement
{
    public static UIStartButton Instance;

    public override void Awake()
    {
        base.Awake();

        Instance = this;
    }
}
