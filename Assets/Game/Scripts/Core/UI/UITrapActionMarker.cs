using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UITrapActionMarker : WorldUIElement
{
    public static UITrapActionMarker Instance;

    public GameObject tutorialDescription;

    protected override void Awake()
    {
        base.Awake();

        Instance = this;

        pivotOffset = new Vector2(Screen.width / 4f * 1.3f, pivotOffset.y);

        //GameManager.OnGameSceneLoaded += Reset;
    }

    private void Reset()
    {
        tutorialDescription.SetActive(false);
    }
}
