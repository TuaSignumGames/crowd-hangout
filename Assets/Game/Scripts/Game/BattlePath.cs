using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattlePath
{
    public static BattlePath Instance;

    public GameObject gameObject;

    public Transform transform;

    public List<BattlePathStage> stages;

    private Crowd playerCrew;

    private int activeStageIndex;

    private bool isBattleActive;
    private bool isBattleStarted;

    public BattlePathStage ActiveStage => stages[activeStageIndex];

    public Crowd PlayerCrew => playerCrew;
    public Crowd GuardCrew => ActiveStage.GuardCrew;

    public Vector3 Position => transform.position;

    public Vector3 StageSize => stages[0].Size;

    public bool IsBattleActive => isBattleActive;

    public BattlePath(GameObject pathGameObject)
    {
        Instance = this;

        gameObject = pathGameObject;

        transform = gameObject.transform;

        stages = new List<BattlePathStage>();
    }

    public void Update()
    {
        if (isBattleActive)
        {
            if (playerCrew != null)
            {
                if (isBattleStarted)
                {
                    if (stages[activeStageIndex].GuardCrew.IsCombatCapable)
                    {
                        if (playerCrew.MembersCount == 1)
                        {
                            // TODO Claim stage reward -> Finish level 

                            FinishBattle();
                        }
                    }
                    else if (activeStageIndex < stages.Count - 1)
                    {
                        activeStageIndex = Mathf.Clamp(activeStageIndex + 1, 0, stages.Count - 1);

                        StartBattleOnActiveStage();
                    }
                }
                else
                {
                    if (playerCrew.IsGrounded)
                    {
                        activeStageIndex = Mathf.Clamp(stages.IndexOf(DefineStage(playerCrew.DefineMidpointXY())), 0, stages.Count - 1);

                        StartBattleOnActiveStage();

                        isBattleStarted = true;
                    }
                }
            }
        }
        else
        {
            stages[activeStageIndex].Update();
        }
    }

    public void Enter(Crowd playerCrowd)
    {
        this.playerCrew = playerCrowd;

        isBattleActive = true;
    }

    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    private void StartBattleOnActiveStage()
    {
        Debug.Log($" Player crowd DamageRate: {playerCrew.Power}");

        //stages[0].GenerateGuard(10, playerCrowd.DamageRate * playerCrowd.MembersCount / 10);

        playerCrew.Assault(stages[Mathf.Clamp(activeStageIndex, 0, stages.Count - 1)].GuardCrew.Defend(playerCrew));
    }

    private void FinishBattle()
    {
        isBattleActive = false;

        LevelGenerator.Instance.FinishBattle();
    }

    private BattlePathStage DefineStage(Vector3 position)
    {
        for (int i = 0; i < stages.Count; i++)
        {
            if (position.x < stages[i].Position.x)
            {
                return stages[Mathf.Clamp(i - 1, 0, stages.Count - 1)];
            }
        }

        return null;
    }
}
