using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIUpgradeMenu : UIElement
{
    public static UIUpgradeMenu Instance;

    [Space]
    [SerializeField] private UpgradeCard weaponCard;
    [SerializeField] private UpgradeCard populationCard;

    private Weapon weaponInfo;

    private UpgradeInfo actualUpgradeInfo;

    private int upgradeIndex;

    public override void Awake()
    {
        base.Awake();

        Instance = this;
    }

    public void UpgradeWeapon()
    {
        actualUpgradeInfo = WorldManager.weaponUpgradeSettings.upgradeTable[GameManager.WeaponUpgradeIndex++];

        GameManager.TopWeaponPower = actualUpgradeInfo.value;

        GameManager.Instance.ChangeCurrency(-actualUpgradeInfo.price, true);

        UpdateWeaponCard(WorldManager.weaponUpgradeSettings.upgradeTable[GameManager.WeaponUpgradeIndex]);
    }

    public void UpgradePopulation()
    {
        actualUpgradeInfo = WorldManager.populationUpgradeSettings.upgradeTable[GameManager.PopulationUpgradeIndex++];

        GameManager.PopulationValue = (int)actualUpgradeInfo.value;

        GameManager.Instance.ChangeCurrency(-actualUpgradeInfo.price, true);

        UpdatePopulationCard(WorldManager.populationUpgradeSettings.upgradeTable[GameManager.PopulationUpgradeIndex]);
    }

    public void UpdateCards()
    {
        UpdateWeaponCard(WorldManager.weaponUpgradeSettings.upgradeTable[GameManager.WeaponUpgradeIndex]);
        UpdatePopulationCard(WorldManager.populationUpgradeSettings.upgradeTable[GameManager.PopulationUpgradeIndex]);
    }

    private void UpdateWeaponCard(UpgradeInfo upgradeInfo)
    {
        weaponInfo = WorldManager.weaponAssortment[WorldManager.GetWeaponID(GameManager.TopWeaponPower)];

        weaponCard.SetSliderValue(Mathf.InverseLerp(weaponInfo.Power, WorldManager.weaponAssortment[weaponInfo.WeaponID + 1].Power, GameManager.TopWeaponPower));

        weaponCard.valueText.text = weaponInfo.weaponContainer.name.ToUpper();
        weaponCard.priceText.text = "$" + upgradeInfo.price.ToString("N0");

        weaponCard.buttonComponent.Interactable = GameManager.Currency >= upgradeInfo.price;
    }

    private void UpdatePopulationCard(UpgradeInfo upgradeInfo)
    {
        populationCard.valueText.text = upgradeInfo.value.ToString();
        populationCard.priceText.text = "$" + upgradeInfo.price.ToString("N0");

        populationCard.buttonComponent.Interactable = GameManager.Currency >= upgradeInfo.price;
    }
}

[System.Serializable]
public class UpgradeCard
{
    public QuickButton buttonComponent;
    [Space]
    public Transform sliderTransform;
    [Space]
    public Text valueText;
    public Text priceText;

    public void SetSliderValue(float value)
    {
        if (sliderTransform)
        {
            sliderTransform.localScale = new Vector3(Mathf.Clamp01(value), 1f, 1f);
        }
    }
}