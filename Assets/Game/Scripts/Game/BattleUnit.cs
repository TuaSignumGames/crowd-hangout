using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleUnit : MonoBehaviour
{
    public List<BattleUnitTeamInfo> teamSettings;
    public BattleUnitRangeSettings rangeSettings;
    public BattleUnitUISettings UISettings;
    [Space]
    public Transform[] positions;

    private BattlePathCellularStage stage;

    private BattlePathCell groundCell;
    private BattlePathCell stageGridCell;

    private Crowd garrisonCrew;

    private BattleUnitTeamInfo actualTeamInfo;

    private List<BattlePathCell> rangeCells;

    private List<BattleUnit> reachableBattleUnits;

    private BattleUnit targetBattleUnit;

    private GameObject rangeMarkerOriginal;

    private GameObject[,] rangeMarkers;

    private Transform requiredPosition;

    private Vector3 rangeMarkerCellPosition;

    private float gridCellSize;

    private int range;
    private int rangeGridSize;

    private int weaponID;

    private int nextAvailablePositionIndex;

    public Crowd Garrison => garrisonCrew;

    public BattlePathCell GroundCell => groundCell;

    public HumanTeam Team => actualTeamInfo.teamType;

    public int WeaponLevel => weaponID;

    private void LateUpdate()
    {
        if (targetBattleUnit)
        {
            if (targetBattleUnit.Garrison.MembersCount == 0)
            {
                UpdateBehavior();
            }
        }

        UISettings.healthBar.SetValue(garrisonCrew.DefineTotalHealthFactor());
    }

    public void GenerateGarrison(HumanTeam team, int weaponLevel)
    {
        List<HumanController> garrisonHumans = new List<HumanController>();

        HumanController humanInstance = null;

        for (int i = 0; i < positions.Length; i++)
        {
            humanInstance = Instantiate(WorldManager.humanPrefab, positions[i]);

            humanInstance.Initialize(team, weaponLevel);

            humanInstance.transform.position = positions[i].position;

            garrisonHumans.Add(humanInstance);
        }

        FormGarrison(garrisonHumans);
    }

    public void FormGarrison(List<HumanController> humans)
    {
        SetTeam(humans[0].team);

        weaponID = WorldManager.GetWeaponID(humans[0].Weapon.Power);

        for (int i = 0; i < rangeSettings.weaponRanges.Length; i++)
        {
            UISettings.weaponRangeIcons[i].SetActive(i == weaponID);
        }

        range = rangeSettings.weaponRanges[weaponID];

        GenerateRangeGrid(range);

        for (int i = 0; i < positions.Length; i++)
        {
            humans[i].defaultContainer = transform.parent;

            humans[i].Weapon.Distance = range * gridCellSize + gridCellSize;

            humans[i].PrepareToBattle(positions[i].position, positions[i].forward);
        }

        garrisonCrew = new Crowd(humans);

        UISettings.healthBar.SetValue(garrisonCrew.DefineTotalHealthFactor());
    }

    public void ApplyGarrison(Crowd crew)
    {
        garrisonCrew = crew;

        SetTeam(crew.Members[0].team);

        weaponID = WorldManager.GetWeaponID(crew.Members[0].Weapon.Power);

        for (int i = 0; i < rangeSettings.weaponRanges.Length; i++)
        {
            UISettings.weaponRangeIcons[i].SetActive(i == weaponID);
        }

        range = rangeSettings.weaponRanges[weaponID];

        GenerateRangeGrid(range);

        for (int i = 0; i < crew.MembersCount; i++)
        {
            crew.Members[i].defaultContainer = transform.parent;

            crew.Members[i].Weapon.Distance = range * gridCellSize + gridCellSize;

            requiredPosition = positions[(int)Mathf.Repeat(nextAvailablePositionIndex++, positions.Length)];

            crew.Members[i].transform.SetParent(requiredPosition);

            crew.Members[i].DropToBattle(requiredPosition.position);
        }

        UISettings.healthBar.SetValue(garrisonCrew.DefineTotalHealthFactor());
    }

    public void UpdateBehavior()
    {
        reachableBattleUnits = new List<BattleUnit>();

        UpdateRange(groundCell);

        for (int i = 0; i < rangeCells.Count; i++)
        {
            if (rangeCells[i].BattleUnit && rangeCells[i].BattleUnit.Team == (Team == HumanTeam.Yellow ? HumanTeam.Red : HumanTeam.Yellow) && rangeCells[i].BattleUnit.Garrison.MembersCount > 0)
            {
                reachableBattleUnits.Add(rangeCells[i].BattleUnit);
            }
        }

        if (reachableBattleUnits.Count > 0)
        {
            reachableBattleUnits.Sort((a, b) => (a.transform.position - transform.position).GetPlanarSqrMagnitude(Axis.Y).CompareTo((b.transform.position - transform.position).GetPlanarSqrMagnitude(Axis.Y)));

            targetBattleUnit = reachableBattleUnits[0];

            garrisonCrew.Defend(targetBattleUnit.Garrison);
        }
        else
        {
            garrisonCrew.Stop();

            targetBattleUnit = null;
        }
    }

    public void SetTeam(HumanTeam team)
    {
        actualTeamInfo = teamSettings.Find((t) => t.teamType == team);

        for (int i = 0; i < teamSettings.Count; i++)
        {
            if (teamSettings[i].teamFlag)
            {
                teamSettings[i].teamFlag.SetActive(teamSettings[i].teamType == team);
            }

            if (teamSettings[i].teamFortification)
            {
                teamSettings[i].teamFortification.SetActive(teamSettings[i].teamType == team);
            }

            if (teamSettings[i].teamRangeMarker)
            {
                teamSettings[i].teamRangeMarker.SetActive(teamSettings[i].teamType == team);
            }
        }
    }

    public void PlaceAt(BattlePathCell cell)
    {
        groundCell = cell;

        groundCell.RegisterBattleUnit(this);

        transform.position = groundCell.Position;
    }

    public void UpdateRange(BattlePathCell originCell)
    {
        rangeCells = new List<BattlePathCell>();

        rangeSettings.container.transform.position = originCell.Position;

        for (int y = 0; y < rangeGridSize; y++)
        {
            for (int x = 0; x < rangeGridSize; x++)
            {
                if (rangeMarkers[x, y])
                {
                    stageGridCell = originCell.Address.stage.TryGetCell(originCell.Address.x + x - range, originCell.Address.y + y - range);

                    rangeMarkers[x, y].SetActive(stageGridCell != null && stageGridCell.Type == BattlePathCellType.Ground);

                    if (stageGridCell != null)
                    {
                        rangeCells.Add(stageGridCell);
                    }
                }
            }
        }

        //rangeSettings.container.SetActive(true);
    }

    public void SetPicked(bool isPicked)
    {
        for (int i = 0; i < garrisonCrew.MembersCount; i++)
        {
            garrisonCrew.Members[i].enabled = !isPicked;
        }

        transform.localPosition = new Vector3(transform.localPosition.x, isPicked ? rangeSettings.pickElevationHeight : 0, transform.localPosition.z);

        SetRangeVisible(isPicked);
    }

    public void SetRangeVisible(bool enabled)
    {
        rangeSettings.container.SetActive(enabled);
    }

    private void GenerateRangeGrid(int range)
    {
        rangeMarkerOriginal = actualTeamInfo.teamRangeMarker.transform.GetChild(0).gameObject;

        gridCellSize = BattlePathGenerator.Instance.groundCellPrefab.transform.localScale.x;

        rangeGridSize = range * 2 + 1;

        rangeMarkers = new GameObject[rangeGridSize, rangeGridSize];

        float sqrRadius = range * range * gridCellSize * gridCellSize;

        for (int y = 0; y < rangeGridSize; y++)
        {
            for (int x = 0; x < rangeGridSize; x++)
            {
                if (x != range || y != range)
                {
                    rangeMarkerCellPosition = rangeMarkerOriginal.transform.position + new Vector3((x - range) * gridCellSize, 0, (y - range) * gridCellSize);

                    if ((rangeMarkerCellPosition - rangeMarkerOriginal.transform.position).GetPlanarSqrMagnitude(Axis.Y) <= sqrRadius)
                    {
                        rangeMarkers[x, y] = Instantiate(rangeMarkerOriginal, rangeMarkerCellPosition, rangeMarkerOriginal.transform.rotation, rangeMarkerOriginal.transform.parent);
                    }
                }
            }
        }

        rangeMarkerOriginal.SetActive(false);

        rangeSettings.container.transform.localEulerAngles = new Vector3(0, 90f, 0);

        rangeSettings.container.transform.SetParent(BattlePathGenerator.Instance.transform);
    }

    private void OnValidate()
    {
        if (teamSettings.Count > 0)
        {
            for (int i = 0; i < teamSettings.Count; i++)
            {
                teamSettings[i].title = teamSettings[i].teamType.ToString();
            }

            //SetTeam(team);
        }
    }
}

[System.Serializable]
public class BattleUnitTeamInfo
{
    [HideInInspector]
    public string title;

    public HumanTeam teamType;
    [Space]
    public GameObject teamFlag;
    public GameObject teamFortification;
    public GameObject teamRangeMarker;
}

[System.Serializable]
public class BattleUnitRangeSettings
{
    public GameObject container;
    public int[] weaponRanges;
    [Space]
    public float pickElevationHeight;
}

[System.Serializable]
public class BattleUnitUISettings
{
    public GameObject weaponIconContainer;
    public GameObject[] weaponRangeIcons;
    public ProgressBar healthBar;

    public void UpdateWeaponIconDirection()
    {
        weaponIconContainer.transform.forward = CameraController.Instance.camera.transform.position - weaponIconContainer.transform.position;
    }

    public void UpdateHealthBarDirection()
    {
        healthBar.Update();
    }
}
