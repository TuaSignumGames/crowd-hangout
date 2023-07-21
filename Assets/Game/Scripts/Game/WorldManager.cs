using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public static HumanController humanPrefab;

    public static EnvironmentSettings environmentSettings;

    public static List<HumanController> yellowTeamHumans;
    public static List<HumanController> redTeamHumans;

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

        environmentSettings = _environmentSettings;

        yellowTeamHumans = new List<HumanController>();
        redTeamHumans = new List<HumanController>();

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

    public static void Upgrade(LevelElementType upgradeTarget)
    {
        if (upgradeTarget == LevelElementType.CollectibleWeapon)
        {
            actualUpgradeInfo = weaponUpgradeSettings.GetUpgradeInfo(GameManager.WeaponUpgradeIndex++);

            GameManager.TopWeaponPower = actualUpgradeInfo.value;

            HumanController.selectedHuman.SetWeapon(GetWeaponID(actualUpgradeInfo.value));
        }

        if (upgradeTarget == LevelElementType.CollectibleHuman)
        {
            actualUpgradeInfo = populationUpgradeSettings.GetUpgradeInfo(GameManager.PopulationUpgradeIndex++);

            GameManager.PopulationValue = (int)actualUpgradeInfo.value;
        }

        GameManager.Instance.ChangeCurrency(-actualUpgradeInfo.price, true);

        GameManager.CryticalStageIndex = (GameManager.WeaponUpgradeIndex + GameManager.PopulationUpgradeIndex) / 2;

        print($" - Crytical stage: {GameManager.CryticalStageIndex}");

        //LevelGenerator.Instance.GenerateComposition();
    }

    public static HumanController[] GetHumansAhead(HumanTeam team, float x)
    {
        HumanController[] requestedTeamHumans = team == HumanTeam.Yellow ? yellowTeamHumans.ToArray() : redTeamHumans.ToArray();

        List<HumanController> selectedHumans = new List<HumanController>();

        for (int i = 0; i < requestedTeamHumans.Length; i++)
        {
            if (requestedTeamHumans[i].transform.position.x > x)
            {
                selectedHumans.Add(requestedTeamHumans[i]);
            }
        }

        return selectedHumans.ToArray();
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
