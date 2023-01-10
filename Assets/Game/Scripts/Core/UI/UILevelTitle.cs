using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UILevelTitle : UIElement
{
    public static UILevelTitle Instance;

    [Space]
    [SerializeField] private Text titleText;
    [SerializeField] private string prefix;

    public override void Awake()
    {
        base.Awake();

        Instance = this;
    }

    public void SetLevelNumber(int value)
    {
        titleText.text = string.Format($"{prefix} {value}");
    }
}
