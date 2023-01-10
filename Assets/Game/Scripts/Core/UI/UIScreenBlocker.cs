using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIScreenBlocker : UIElement
{
    public static UIScreenBlocker Instance;

    public override void Awake()
    {
        base.Awake();

        Instance = this;

        GameManager.OnGameSceneLoaded += Reset;
    }

    private void Reset()
    {
        HideImmediate();
    }
}
