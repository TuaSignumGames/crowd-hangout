using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleUnit : MonoBehaviour
{
    public List<BattleUnitTeamInfo> teamSettings;
    public BattleUnitRangeSettings rangeSettings;
    [Space]
    public Transform[] positions;

    private BattlePathCell groundCell;
    private BattlePathCell stageGridCell;

    private Crowd garrisonCrew;

    private BattleUnitTeamInfo actualTeamInfo;

    private GameObject rangeMarkerCellOriginal;

    private GameObject[,] rangeGridCells;

    private Vector3 rangeMarkerCellPosition;

    private float gridCellSize;

    private int range;
    private int rangeGridSize;

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

        for (int i = 0; i < positions.Length; i++)
        {
            humans[i].defaultContainer = positions[i];

            humans[i].PrepareToBattle(positions[i].position, positions[i].forward);

            humans[i].AI.Defend(positions[i]);
        }

        range = rangeSettings.weaponRanges[WorldManager.GetWeaponID(humans[0].Weapon.Power)];

        GenerateRangeGrid(range);

        garrisonCrew = new Crowd(humans);
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

        transform.position = groundCell.Position;
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

        if (enabled)
        {
            for (int y = 0; y < rangeGridSize; y++)
            {
                for (int x = 0; x < rangeGridSize; x++)
                {
                    if (rangeGridCells[x, y])
                    {
                        stageGridCell = BattlePathGenerator.Instance.ActualStage.TryGetCell(groundCell.Address.x + x - range, groundCell.Address.y + y - range);

                        rangeGridCells[x, y].SetActive(stageGridCell != null && stageGridCell.Type == BattlePathCellType.Ground);
                    }
                }
            }
        }
    }

    private void GenerateRangeGrid(int range)
    {
        rangeMarkerCellOriginal = actualTeamInfo.teamRangeMarker.transform.GetChild(0).gameObject;

        gridCellSize = BattlePathGenerator.Instance.groundCellPrefab.transform.localScale.x;

        rangeGridSize = range * 2 + 1;

        rangeGridCells = new GameObject[rangeGridSize, rangeGridSize];

        float sqrRadius = range * range * gridCellSize * gridCellSize;

        for (int y = 0; y < rangeGridSize; y++)
        {
            for (int x = 0; x < rangeGridSize; x++)
            {
                if (x != range || y != range)
                {
                    rangeMarkerCellPosition = rangeMarkerCellOriginal.transform.position + new Vector3((x - range) * gridCellSize, 0, (y - range) * gridCellSize);

                    if ((rangeMarkerCellPosition - rangeMarkerCellOriginal.transform.position).GetPlanarSqrMagnitude(Axis.Y) <= sqrRadius)
                    {
                        rangeGridCells[x, y] = Instantiate(rangeMarkerCellOriginal, rangeMarkerCellPosition, rangeMarkerCellOriginal.transform.rotation, rangeMarkerCellOriginal.transform.parent);
                    }
                }
            }
        }

        rangeSettings.container.transform.localPosition = new Vector3(0, -rangeSettings.pickElevationHeight, 0);
        rangeSettings.container.transform.localEulerAngles = new Vector3(0, 90f, 0);
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
