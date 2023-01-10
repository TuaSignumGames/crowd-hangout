using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using GameAnalyticsSDK;

public class LevelManager : Service<LevelManager>
{
    //public bool debugActiveLevel;

    public CreativeModeLevelSettings creativeModeSettings;

    private float _rewardAmount;

    private bool _isLevelStarted;
    private bool _isLevelFinished;
    private bool _isLevelPassed;

    private bool _isTutorialLevel;

    private bool _isTutorialLaunched;

    public int LevelIndex { get { return PlayerPrefs.GetInt("LVL_ID"); } private set { PlayerPrefs.SetInt("LVL_ID", value); } }
    public int LevelNumber { get { return PlayerPrefs.GetInt("LVL_NUM", 1); } private set { PlayerPrefs.SetInt("LVL_NUM", value); } }

    public bool IsLevelStarted => _isLevelStarted;
    public bool IsLevelFinished => _isLevelFinished;

    public bool IsTutorialLevel => _isTutorialLevel;

    public void InitializeLevel()
    {
        _isLevelStarted = false;
        _isLevelFinished = false;
        _isTutorialLaunched = false;

        StartCoroutine(LevelInitializationCoroutine());
    }

    private void Start()
    {

    }

    private void LateUpdate()
    {
        if (_isLevelStarted)
        {

        }
    }

    public void OnLevelStarted()
    {
        if (!_isLevelStarted)
        {
            StartCoroutine(LevelStartingCoroutine());

            GameAnalytics.NewProgressionEvent(GAProgressionStatus.Start, "level_" + LevelNumber.ToString("D3"));
        }
    }

    public void OnLevelFinished(bool success)
    {
        if (!_isLevelFinished)
        {
            _isLevelFinished = true;
            _isLevelPassed = success;

            if (_isLevelPassed)
            {
                GameAnalytics.NewProgressionEvent(GAProgressionStatus.Complete, "level_" + LevelNumber.ToString("D3"));

                AppManager.Instance.PlayHaptic(MoreMountains.NiceVibrations.HapticTypes.Success);
            }
            else
            {
                GameAnalytics.NewProgressionEvent(GAProgressionStatus.Fail, "level_" + LevelNumber.ToString("D3"));

                AppManager.Instance.PlayHaptic(MoreMountains.NiceVibrations.HapticTypes.Failure);
            }

            //UIRewardPanel.Instance.SetReward(_rewardAmount);

            //UIManager.Instance.SetBackgroundTriggerEvent(UnityEngine.EventSystems.EventTriggerType.PointerDown, GameManager.Instance.ReloadGameScene);

            UIManager.Instance.ChangeState(success ? UIState.Success : UIState.Fail);
        }
    }

    public void LoadLevel(int levelIndex)
    {
        LevelIndex = levelIndex; //Mathf.Clamp(levelIndex, 0, LevelGenerator.Instance.levels.Length - 1);

        GameManager.Instance.ReloadGameScene();
    }

    public void LoadNextLevel()
    {
        if (_isLevelPassed)
        {
            LevelIndex++;
            LevelNumber++;
        }

        GameManager.Instance.ReloadGameScene();
    }

    public void MultiplyReward(float multiplier)
    {
        _rewardAmount *= multiplier;

        UIScreenBlocker.Instance.ShowImmediate();
        UIRewardPanel.Instance.Recount(_rewardAmount, onRecountCompleted: () => { AppManager.Instance.PlayHaptic(MoreMountains.NiceVibrations.HapticTypes.Success); DOVirtual.DelayedCall(1.5f, () => LoadNextLevel()); });
    }

    public void LaunchTutorial()
    {
        if (!_isTutorialLaunched)
        {
            StartCoroutine(TutorialShowingCoroutine());
        }
    }

    public void AddReward(float increment)
    {
        _rewardAmount += increment;
    }

    private IEnumerator LevelInitializationCoroutine()
    {
        while (false) { yield return null; }

        /*
        if (debugActiveLevel)
        {

        }
        */
        //_isTutorialLevel = false;

        UIManager.Instance.ChangeState(GameManager.Instance.creativeMode ? UIState.Empty : UIState.Start);

        UILevelTitle.Instance.SetLevelNumber(LevelNumber);
        UIProgressBar.Instance.Initialize(LevelNumber);
    }

    private IEnumerator LevelStartingCoroutine()
    {
        // TODO Pre-start code

        _isLevelStarted = true;

        if (GameManager.Instance.creativeMode) //&& !CreativeManager.Instance.creativeModeSettings.actionPhaseUI)
        {
            UIManager.Instance.ChangeState(UIState.Empty);
        }
        else
        {
            UIManager.Instance.ChangeState(UIState.ActionPhase);
        }

        yield return null;
    }

    private IEnumerator TutorialShowingCoroutine()
    {
        _isTutorialLaunched = true;

        // TODO Tutorial processing code

        yield return null;
    }

    [System.Serializable]
    public struct CreativeModeLevelSettings
    {
        public int levelID;
        [Space]
        public int skinID;
        public int themeID;
    }
}
