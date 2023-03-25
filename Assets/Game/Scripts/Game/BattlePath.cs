using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattlePath
{
    public GameObject gameObject;

    public Transform transform;

    public List<BattlePathStage> stages;

    private Crowd playerCrowd;

    private int activeStageIndex;

    private bool isBattleActive;
    private bool isBattleStarted;

    public Vector3 position => transform.position;

    public BattlePath(GameObject pathGameObject)
    {
        gameObject = pathGameObject;

        transform = gameObject.transform;

        stages = new List<BattlePathStage>();
    }

    public void Update()
    {
        if (isBattleActive)
        {
            if (playerCrowd != null)
            {
                if (isBattleStarted)
                {
                    if (stages[activeStageIndex].GuardCrew.IsCombatCapable)
                    {
                        if (playerCrowd.MembersCount == 1)
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
                    if (playerCrowd.IsGrounded)
                    {
                        activeStageIndex = Mathf.Clamp(stages.IndexOf(DefineStage(playerCrowd.DefineMidpointXY())), 0, stages.Count - 1);

                        StartBattleOnActiveStage();

                        isBattleStarted = true;
                    }
                }
            }
        }
    }

    public void Enter(Crowd playerCrowd)
    {
        this.playerCrowd = playerCrowd;

        isBattleActive = true;
    }

    private void StartBattleOnActiveStage()
    {
        Debug.Log($" Player crowd DamageRate: {playerCrowd.Power}");

        //stages[0].GenerateGuard(10, playerCrowd.DamageRate * playerCrowd.MembersCount / 10);

        playerCrowd.Assault(stages[Mathf.Clamp(activeStageIndex, 0, stages.Count - 1)].GuardCrew.Defend(playerCrowd));
    }

    private void FinishBattle()
    {
        isBattleActive = false;

        playerCrowd.Members[0].isImmortal = true;

        CameraController.Instance.FocusOn(playerCrowd.Members[0].transform, LevelGenerator.Instance.battlePathSettings.finishView);

        playerCrowd.Stop();

        stages[activeStageIndex].GuardCrew.Stop();
    }

    private BattlePathStage DefineStage(Vector3 position)
    {
        for (int i = 0; i < stages.Count; i++)
        {
            if (position.x < stages[i].position.x)
            {
                return stages[Mathf.Clamp(i - 1, 0, stages.Count - 1)];
            }
        }

        return null;
    }
}
