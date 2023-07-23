using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleUnit : MonoBehaviour
{
    public List<BattleUnitTeamInfo> teamSettings;
    public Transform[] positions;

    private Crowd garrisonCrew;

    private BattleUnitTeamInfo actualTeamInfo;

    public void GenerateGarrison(int weaponLevel)
    {
        List<HumanController> garrisonHumans = new List<HumanController>();

        HumanController humanInstance = null;

        for (int i = 0; i < positions.Length; i++)
        {
            humanInstance = Instantiate(WorldManager.humanPrefab, positions[i]);

            humanInstance.transform.position = positions[i].position;
            humanInstance.transform.forward = positions[i].forward;

            garrisonHumans.Add(humanInstance);
        }

        FormGarrison(garrisonHumans);
    }

    public void FormGarrison(List<HumanController> humans)
    {
        for (int i = 0; i < positions.Length; i++)
        {
            humans[i].AI.Defend(positions[i].position);
        }

        garrisonCrew = new Crowd(humans);
    }

    public void SetTeam(HumanTeam team)
    {
        for (int i = 0; i < teamSettings.Count; i++)
        {
            teamSettings[i].teamFlag.SetActive(teamSettings[i].teamType == team);
            teamSettings[i].teamFortification.SetActive(teamSettings[i].teamType == team);
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
}
