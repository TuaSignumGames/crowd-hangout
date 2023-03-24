using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIUpgradeMenu : UIElement
{
    public static UIUpgradeMenu Instance;

    [Space]
    [SerializeField] private UpgradeCard[] upgradeCards;

    public override void Awake()
    {
        base.Awake();

        Instance = this;
    }

    public void UpgradePopulation()
    {
        print($" - Upgrade: Population");
    }
}

[System.Serializable]
public class UpgradeCard
{
    public QuickButton buttonElement;
}