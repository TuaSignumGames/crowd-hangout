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
        if (playerCrowd != null)
        {
            if (isBattleStarted)
            {
                if (stages[activeStageIndex].GuardCrew.IsCombatCapable)
                {
                    if (!playerCrowd.IsCombatCapable)
                    {
                        // TODO Claim stage reward -> Finish level 
                    }
                }
                else
                {
                    activeStageIndex++;

                    StartBattleOnActiveStage();
                }
            }
            else
            {
                if (playerCrowd.IsGrounded)
                {
                    activeStageIndex = stages.IndexOf(DefineStage(playerCrowd.DefineMidpointXY()));

                    StartBattleOnActiveStage();

                    isBattleStarted = true;
                }
            }
        }
    }

    public void Enter(Crowd playerCrowd)
    {
        this.playerCrowd = playerCrowd;
    }

    private void StartBattleOnActiveStage()
    {
        playerCrowd.Assault(stages[Mathf.Clamp(activeStageIndex, 0, stages.Count - 1)].GuardCrew.Defend(playerCrowd));
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
