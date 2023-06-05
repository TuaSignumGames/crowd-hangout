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

    public override void Awake()
    {
        base.Awake();

        Instance = this;
    }

    public void UpgradeWeapon()
    {
        WorldManager.Upgrade(LevelElementType.CollectibleWeapon);

        UpdateCards();
    }

    public void UpgradePopulation()
    {
        WorldManager.Upgrade(LevelElementType.CollectibleHuman);

        UpdateCards();
    }

    public void UpdateCards()
    {
        UpdateWeaponCard(WorldManager.weaponUpgradeSettings.GetUpgradeInfo(GameManager.WeaponUpgradeIndex));
        UpdatePopulationCard(WorldManager.populationUpgradeSettings.GetUpgradeInfo(GameManager.PopulationUpgradeIndex));
    }

    private void UpdateWeaponCard(UpgradeInfo upgradeInfo)
    {
        weaponInfo = WorldManager.weaponAssortment[WorldManager.GetWeaponID(upgradeInfo.value)];

        //weaponCard.SetSliderValue(Mathf.InverseLerp(weaponInfo.Power, WorldManager.weaponAssortment[weaponInfo.WeaponID + 1].Power, GameManager.TopWeaponPower));

        int weaponID = weaponInfo.WeaponID;

        for (int i = 0; i < weaponCard.icons.Length; i++)
        {
            weaponCard.icons[i].SetActive(i == weaponID);
        }

        weaponCard.valueText.text = $"Damage:  <b>{upgradeInfo.value}</b>";
        weaponCard.priceText.text = "$" + upgradeInfo.price.ToString("N0");

        weaponCard.buttonComponent.Interactable = GameManager.Currency >= upgradeInfo.price;
    }

    private void UpdatePopulationCard(UpgradeInfo upgradeInfo)
    {
        populationCard.valueText.text = $"Number:  <b>{upgradeInfo.value + 1}</b>";
        populationCard.priceText.text = "$" + upgradeInfo.price.ToString("N0");

        populationCard.buttonComponent.Interactable = GameManager.Currency >= upgradeInfo.price;
    }
}

[System.Serializable]
public class UpgradeCard
{
    public QuickButton buttonComponent;
    [Space]
    public GameObject[] icons;
    [Space]
    public Text valueText;
    public Text priceText;

    /*
    public void SetSliderValue(float value)
    {
        if (sliderTransform)
        {
            sliderTransform.localScale = new Vector3(Mathf.Clamp01(value), 1f, 1f);
        }
    }
    */
}