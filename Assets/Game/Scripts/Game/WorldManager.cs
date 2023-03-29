using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public static HumanController humanPrefab;

    public static List<Weapon> weaponAssortment;

    public static ProgressionSettings progressionSettings;

    public static UpgradeSettings weaponUpgradeSettings;
    public static UpgradeSettings populationUpgradeSettings;

    public HumanController _humanPrefab;
    [Space]
    public ProgressionSettings _progressionSettings;
    [Space]
    public UpgradeSettings _weaponUpgradeSettings;
    public UpgradeSettings _populationUpgradeSettings;

    private void Awake()
    {
        humanPrefab = _humanPrefab;

        weaponAssortment = new List<Weapon>(humanPrefab.weaponSettings);

        progressionSettings = _progressionSettings;

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
        if (_progressionSettings.progressionStages.Count > 0)
        {
            for (int i = 0; i < _progressionSettings.progressionStages.Count; i++)
            {
                if (i > 0)
                {
                    _progressionSettings.progressionStages[i].initialLevelNumber = Mathf.Clamp(_progressionSettings.progressionStages[i].initialLevelNumber, _progressionSettings.progressionStages[i - 1].initialLevelNumber + 1, int.MaxValue);
                }

                if (i < _progressionSettings.progressionStages.Count - 1)
                {
                    _progressionSettings.progressionStages[i].title = $"Level {_progressionSettings.progressionStages[i].initialLevelNumber}-{_progressionSettings.progressionStages[i + 1].initialLevelNumber - 1}";
                }
                else
                {
                    _progressionSettings.progressionStages[i].title = $"Level {_progressionSettings.progressionStages[i].initialLevelNumber}+";
                }
            }
        }
    }
}
