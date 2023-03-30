using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public static HumanController humanPrefab;

    public static List<Weapon> weaponAssortment;

    public static ProgressionSettings gameProgressionSettings;
    public static RewardProgressionSettings rewardProgressionSettings;

    public static UpgradeSettings weaponUpgradeSettings;
    public static UpgradeSettings populationUpgradeSettings;

    public HumanController _humanPrefab;
    [Space]
    public ProgressionSettings _gameProgressionSettings;
    public RewardProgressionSettings _rewardProgressionSettings;
    [Space]
    public UpgradeSettings _weaponUpgradeSettings;
    public UpgradeSettings _populationUpgradeSettings;

    private void Awake()
    {
        humanPrefab = _humanPrefab;

        weaponAssortment = new List<Weapon>(humanPrefab.weaponSettings);

        gameProgressionSettings = _gameProgressionSettings;
        rewardProgressionSettings = _rewardProgressionSettings;

        weaponUpgradeSettings = _weaponUpgradeSettings;
        populationUpgradeSettings = _populationUpgradeSettings;
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
