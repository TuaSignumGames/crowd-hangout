using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleUnit : MonoBehaviour
{
    public HumanTeam team;
    public List<BattleUnitTeamInfo> teamSettings;
    public int weaponLevel;

    private BattleUnitTeamInfo actualTeamInfo;

    public void SetTeam(HumanTeam team)
    {
        this.team = team;

        //actualTeamInfo = teamSettings.Find((t) => t.teamType == team);

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
