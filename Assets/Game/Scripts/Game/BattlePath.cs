using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattlePath
{
    public static BattlePath Instance;

    public BattlePathSettings settings;

    public GameObject gameObject;

    public Transform transform;

    public List<BattlePathStage> stages;

    private Crowd playerCrew;

    private Vector3 previousTouchPosition;

    private float stepCounter;
    private float decrementationDelta;
    private float statsResettingTime;

    private float drawPathLenght;
    private float drawPathIncrement;

    private int activeStageIndex;

    private bool isBattleActive;
    private bool isBattleStarted;

    public BattlePathStage ActiveStage => stages[activeStageIndex];

    public Crowd PlayerCrew => playerCrew;
    public Crowd GuardCrew => ActiveStage.GuardCrew;

    public Vector3 Position => transform.position;

    public Vector3 StageSize => stages[0].Size;

    public bool IsBattleActive => isBattleActive;

    public BattlePath(GameObject pathGameObject, BattlePathSettings battlePathSettings)
    {
        Instance = this;

        settings = battlePathSettings;

        gameObject = pathGameObject;

        transform = gameObject.transform;

        stages = new List<BattlePathStage>();

        decrementationDelta = settings.statBoosterSettings.decrementationSpeed * Time.fixedDeltaTime;
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
                        if (playerCrew.MembersCount == 0)
                        {
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
            /*
            if (InputManager.touch)
            {
                UIStatBoosterPoint.Instance.transform.position = InputManager.touchPosition;

                TryIncreasePlayerCrewStats();
            }
            else
            {
                if (stepCounter > 0 && Time.timeSinceLevelLoad > statsResettingTime)
                {
                    stepCounter -= decrementationDelta;

                    UpdatePlayerCrewStatMultipliers(stepCounter);
                }
            }
            */

            if (InputManager.touch)
            {
                previousTouchPosition = InputManager.touchPosition;
            }

            if (InputManager.touchPresent)
            {
                UIStatBoosterPoint.Instance.transform.position = InputManager.touchPosition;

                drawPathIncrement = (InputManager.touchPosition - previousTouchPosition).GetPlanarMagnitude(Axis.Z);

                drawPathLenght += drawPathIncrement;

                if (drawPathIncrement > 5f)
                {
                    stepCounter++;

                    TryIncreasePlayerCrewStats();
                }
                else if (stepCounter > 0 && Time.timeSinceLevelLoad > statsResettingTime)
                {
                    stepCounter -= decrementationDelta;

                    UpdatePlayerCrewStatMultipliers(stepCounter);
                }

                previousTouchPosition = InputManager.touchPosition;
            }
            else
            {
                if (stepCounter > 0 && Time.timeSinceLevelLoad > statsResettingTime)
                {
                    stepCounter -= decrementationDelta;

                    UpdatePlayerCrewStatMultipliers(stepCounter);
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
        playerCrew = playerCrowd;

        UIStatBoosterPoint.Instance.SetVisible(true);

        isBattleActive = true;
    }

    public void TryIncreasePlayerCrewStats()
    {
        UpdatePlayerCrewStatMultipliers(++stepCounter);

        UIStatBoosterPoint.Instance.Pulse();

        //AppManager.Instance.PlayHaptic(MoreMountains.NiceVibrations.HapticTypes.Selection);

        statsResettingTime = Time.timeSinceLevelLoad + settings.statBoosterSettings.incrementationTimeout;
    }

    public BattlePathStage DefineStage(Vector3 position)
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

    public void SetActive(bool isActive)
    {
        gameObject.SetActive(isActive);
    }

    public void Clear()
    {
        for (int i = 0; i < stages.Count; i++)
        {
            GameObject.Destroy(stages[i].gameObject);
        }
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

        UIStatBoosterPoint.Instance.SetVisibleImmediate(false);

        LevelGenerator.Instance.FinishBattle();
    }

    private void UpdatePlayerCrewStatMultipliers(float multiplicationStep)
    {
        playerCrew.MultiplyMotionSpeed(settings.statBoosterSettings.motionSpeedBoosterData.GetMultiplier(multiplicationStep));
        playerCrew.MultiplyDamageRate(settings.statBoosterSettings.damageRateBoosterData.GetMultiplier(multiplicationStep));
    }
}
