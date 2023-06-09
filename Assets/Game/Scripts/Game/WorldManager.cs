using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public static WorldManager Instance;

    public HumanController _humanPrefab;
    public List<MaterialSetData> _humanSkinSets;
    [Space]
    public List<WeaponSetInfo> _weaponSets;
    [Space]
    public EnvironmentSettings _environmentSettings;
    [Space]
    public ProgressionSettings _gameProgressionSettings;
    public BattlePathProgressionSettings _battlePathProgressionSettings;
    [Space]
    public UpgradeSettings _weaponUpgradeSettings;
    public UpgradeSettings _populationUpgradeSettings;

    public static HumanController humanPrefab;
    public static List<MaterialSetData> humanSkinSets;

    public static List<WeaponSetInfo> weaponSets;

    public static Pool<Material> humanMaterialPool;

    public static List<HumanController> yellowTeamHumans;
    public static List<HumanController> redTeamHumans;

    public static List<Weapon> weaponAssortment;

    public static EnvironmentSettings environmentSettings;

    public static ProgressionSettings gameProgressionSettings;
    public static BattlePathProgressionSettings battlePathProgressionSettings;

    public static UpgradeSettings weaponUpgradeSettings;
    public static UpgradeSettings populationUpgradeSettings;

    private static int themeIndex;
    private static int humanSkinSetIndex;

    private static UpgradeInfo actualUpgradeInfo;

    public static ThemeInfo CurrentTheme => environmentSettings.themes[themeIndex];

    public static int ThemeIndex => themeIndex;
    public static int HumanSkinSetIndex => humanSkinSetIndex;

    private void Awake()
    {
        Instance = this;

        environmentSettings = _environmentSettings;

        humanPrefab = _humanPrefab;
        humanSkinSets = _humanSkinSets;

        weaponSets = _weaponSets;

        themeIndex = GameManager.Instance.creativeMode ? CreativeManager.Instance.ThemeIndex : 0;
        humanSkinSetIndex = GameManager.Instance.creativeMode ? CreativeManager.Instance.HumanSkinSetIndex : 0;

        humanMaterialPool = new Pool<Material>(humanSkinSets[humanSkinSetIndex].materials);

        yellowTeamHumans = new List<HumanController>();
        redTeamHumans = new List<HumanController>();

        weaponAssortment = new List<Weapon>(humanPrefab.weaponSettings);

        gameProgressionSettings = _gameProgressionSettings;
        battlePathProgressionSettings = _battlePathProgressionSettings;

        weaponUpgradeSettings = _weaponUpgradeSettings;
        populationUpgradeSettings = _populationUpgradeSettings;

        RenderSettings.skybox = environmentSettings.themes[themeIndex].skyboxMaterial;

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
        }

        GameManager.Instance.ChangeCurrency(-actualUpgradeInfo.price, true);

        GameManager.CryticalStageIndex = (GameManager.WeaponUpgradeIndex + GameManager.PopulationUpgradeIndex) / 2;

        print($" - Crytical stage: {GameManager.CryticalStageIndex}");

        //LevelGenerator.Instance.GenerateComposition();
    }

    public static HumanController GetClosestHuman(HumanTeam team, Vector3 position)
    {
        HumanController[] requestedTeamHumans = team == HumanTeam.Yellow ? yellowTeamHumans.ToArray() : redTeamHumans.ToArray();

        HumanController closestHuman = null;

        float actualHumanSqrDistance = 0;
        float closestHumanSqrDistance = float.MaxValue;

        for (int i = 0; i < requestedTeamHumans.Length; i++)
        {
            actualHumanSqrDistance = (requestedTeamHumans[i].transform.position - position).sqrMagnitude;

            if (actualHumanSqrDistance < closestHumanSqrDistance)
            {
                closestHuman = requestedTeamHumans[i];

                closestHumanSqrDistance = actualHumanSqrDistance;
            }
        }

        return closestHuman;
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

    public static HumanController[] GetHumansBefore(HumanTeam team, float x)
    {
        HumanController[] requestedTeamHumans = team == HumanTeam.Yellow ? yellowTeamHumans.ToArray() : redTeamHumans.ToArray();

        List<HumanController> selectedHumans = new List<HumanController>();

        for (int i = 0; i < requestedTeamHumans.Length; i++)
        {
            if (requestedTeamHumans[i].transform.position.x < x)
            {
                selectedHumans.Add(requestedTeamHumans[i]);
            }
        }

        return selectedHumans.ToArray();
    }

    public static string[] GetHumanSkinSetTitles()
    {
        string[] titles = new string[humanSkinSets.Count];

        for (int i = 0; i < titles.Length; i++)
        {
            titles[i] = humanSkinSets[i].title;
        }

        return titles;
    }

    public static string[] GetWeaponSetTitles()
    {
        string[] titles = new string[weaponSets.Count];

        for (int i = 0; i < titles.Length; i++)
        {
            titles[i] = weaponSets[i].title;
        }

        return titles;
    }

    private void OnValidate()
    {
        if (_weaponSets.Count > 0)
        {
            for (int i = 0; i < _weaponSets.Count; i++)
            {
                _weaponSets[i].GetTitle();
            }
        }

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
