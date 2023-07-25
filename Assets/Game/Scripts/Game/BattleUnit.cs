using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleUnit : MonoBehaviour
{
    public List<BattleUnitTeamInfo> teamSettings;
    public Transform[] positions;
    [Space]
    public Transform rangeMarkerContainer;
    public float elevationHeight;

    private Crowd garrisonCrew;

    private BattleUnitTeamInfo actualTeamInfo;

    private GameObject rangeMarkerCellOriginal;

    private Vector3 rangeMarkerCellPosition;

    private float gridCellSize;

    private int rangeMarkerArm;
    private int rangeMarkerSize;

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

        GenerateRangeMarker(humans[0].Weapon.attackDistance);

        garrisonCrew = new Crowd(humans);
    }

    public void SetTeam(HumanTeam team)
    {
        actualTeamInfo = teamSettings.Find((t) => t.teamType == team);

        for (int i = 0; i < teamSettings.Count; i++)
        {
            teamSettings[i].teamFlag.SetActive(teamSettings[i].teamType == team);
            teamSettings[i].teamFortification.SetActive(teamSettings[i].teamType == team);
        }
    }

    public void SetPicked(bool isPicked)
    {
        transform.localPosition = new Vector3(transform.localPosition.x, isPicked ? elevationHeight : 0, transform.localPosition.z);


    }

    public void SetRangeVisible(bool enabled)
    {

    }

    private void GenerateRangeMarker(float radius)
    {
        rangeMarkerCellOriginal = actualTeamInfo.teamRangeMarker.transform.GetChild(0).gameObject;

        gridCellSize = BattlePathGenerator.Instance.groundCellPrefab.transform.localScale.x;

        rangeMarkerArm = Mathf.RoundToInt(radius / gridCellSize);
        rangeMarkerSize = rangeMarkerArm * 2 + 1;

        float sqrRadius = radius * radius;

        for (int y = 0; y < rangeMarkerSize; y++)
        {
            for (int x = 0; x < rangeMarkerSize; x++)
            {
                if (x != rangeMarkerArm && y != rangeMarkerArm)
                {
                    rangeMarkerCellPosition = rangeMarkerCellOriginal.transform.position + new Vector3(x * (gridCellSize - rangeMarkerArm), 0, y * (gridCellSize - rangeMarkerArm));

                    if ((rangeMarkerCellPosition - rangeMarkerCellOriginal.transform.position).GetPlanarSqrMagnitude(Axis.Y) < sqrRadius)
                    {
                        Instantiate(rangeMarkerCellOriginal, rangeMarkerCellPosition, rangeMarkerCellOriginal.transform.rotation, rangeMarkerCellOriginal.transform.parent);
                    }
                }
            }
        }
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
