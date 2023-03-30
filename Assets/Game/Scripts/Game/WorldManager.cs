using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public static HumanController humanPrefab;

    public static List<Weapon> weaponAssortment;

    public static ProgressionSettings gameProgressionSettings;
    public static BattlePathProgressionSettings battlePathProgressionSettings;

    public static UpgradeSettings weaponUpgradeSettings;
    public static UpgradeSettings populationUpgradeSettings;

    public HumanController _humanPrefab;
    [Space]
    public EnvironmentSettings _environmentSettings;
    [Space]
    public ProgressionSettings _gameProgressionSettings;
    public BattlePathProgressionSettings _battlePathProgressionSettings;
    [Space]
    public UpgradeSettings _weaponUpgradeSettings;
    public UpgradeSettings _populationUpgradeSettings;

    private static UpgradeInfo actualUpgradeInfo;

    private void Awake()
    {
        humanPrefab = _humanPrefab;

        weaponAssortment = new List<Weapon>(humanPrefab.weaponSettings);

        gameProgressionSettings = _gameProgressionSettings;
        battlePathProgressionSettings = _battlePathProgressionSettings;

        weaponUpgradeSettings = _weaponUpgradeSettings;
        populationUpgradeSettings = _populationUpgradeSettings;

        //int iterations = 50;

        //print($" - Weapon upgrade cycle for [{iterations}] iterations");
        //weaponUpgradeSettings.SimulateUpgradeCycle(iterations);

        //print($" - Population upgrade cycle for [{iterations}] iterations");
        //populationUpgradeSettings.SimulateUpgradeCycle(iterations);
    }

    public static float GetWeaponPower(int weaponID)
    {
        return humanPrefab.weaponSettings[weaponID].Power;
    }

    public static int GetWeaponID(float power)
    {
        //print($" Get Weapon ID -- Power: {power}");

        for (int i = 0; i < humanPrefab.weaponSettings.Count; i++)
        {
            if (humanPrefab.weaponSettings[i].Power > power)
            {
                return i - 1;
            }
        }

        return humanPrefab.weaponSettings.Count - 1;
    }

    public static void Upgrade(CollectibleType upgradeTarget)
    {
        if (upgradeTarget == CollectibleType.Weapon)
        {
            actualUpgradeInfo = weaponUpgradeSettings.GetUpgradeInfo(GameManager.WeaponUpgradeIndex++);

            GameManager.TopWeaponPower = actualUpgradeInfo.value;

            HumanController.selectedHuman.SetWeapon(GetWeaponID(actualUpgradeInfo.value));
        }

        if (upgradeTarget == CollectibleType.Human)
        {
            actualUpgradeInfo = populationUpgradeSettings.GetUpgradeInfo(GameManager.PopulationUpgradeIndex++);

            GameManager.PopulationValue = (int)actualUpgradeInfo.value;

            LevelGenerator.Instance.GenerateComposition();
        }

        GameManager.Instance.ChangeCurrency(-actualUpgradeInfo.price, true);

        GameManager.CryticalStageIndex = (GameManager.WeaponUpgradeIndex + GameManager.PopulationUpgradeIndex) / 2;

        print($" - Crytical stage: {GameManager.CryticalStageIndex}");

        LevelGenerator.Instance.GenerateComposition();
    }

    private void OnValidate()
    {
        if (_gameProgressionSettings.progressionStages.Count > 0)
        {
            for (int i = 0; i < _gameProgressionSettings.progressionStages.Count; i++)
            {
                if (i > 0)
                {
                    _gameProgressionSettings.progressionStages[i].initialLevelNumber = Mathf.Clamp(_gameProgressionSettings.progressionStages[i].initialLevelNumber, _gameProgressionSettings.progressionStages[i - 1].initialLevelNumber + 1, int.MaxValue);
                }

                if (i < _gameProgressionSettings.progressionStages.Count - 1)
                {
                    _gameProgressionSettings.progressionStages[i].title = $"Level {_gameProgressionSettings.progressionStages[i].initialLevelNumber}-{_gameProgressionSettings.progressionStages[i + 1].initialLevelNumber - 1}";
                }
                else
                {
                    _gameProgressionSettings.progressionStages[i].title = $"Level {_gameProgressionSettings.progressionStages[i].initialLevelNumber}+";
                }
            }
        }
    }
}
