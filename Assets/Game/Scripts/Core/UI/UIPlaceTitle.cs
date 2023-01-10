using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPlaceTitle : UIElement
{
    public static UIPlaceTitle Instance;

    [SerializeField] private Text _placeTitleText;

    private string _suffix;

    public override void Awake()
    {
        base.Awake();

        Instance = this;
    }

    public void SetPlace(int number)
    {
        _suffix = "th";

        switch (number)
        {
            case 1: _suffix = "st"; break;
            case 2: _suffix = "nd"; break;
            case 3: _suffix = "rd"; break;
        }

        _placeTitleText.text = $"{number}<size=2> </size><size=10>{_suffix}</size>";
    }
}
