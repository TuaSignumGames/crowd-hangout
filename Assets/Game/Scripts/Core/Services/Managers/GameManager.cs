using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Service<GameManager>
{
    public static Action OnGameSceneLoaded;

    public CreativeManager creativeManager;
    [Space]
    public bool creativeMode;

    private Evaluator _timeScaleEvaluator;

    private float _defaultFixedDeltaTime;
    private float _timeScaleValue;

    public static float Currency { get { return PlayerPrefs.GetFloat("COIN", 0); } set { PlayerPrefs.SetFloat("COIN", value); } }

    protected override void Awake()
    {
        base.Awake();

        _defaultFixedDeltaTime = Time.fixedDeltaTime;

        _timeScaleEvaluator = new Evaluator(MonoUpdateType.LateUpdate);
    }

    private void LateUpdate()
    {
        if (_timeScaleEvaluator.Iterating)
        {
            _timeScaleEvaluator.Iterate(ref _timeScaleValue);

            Time.fixedDeltaTime = _defaultFixedDeltaTime * _timeScaleValue;
            Time.timeScale = _timeScaleValue;
        }
    }

    public void InitializeLevel(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.buildIndex == AppManager.Instance.GameEntrySceneIndex)
        {
            print($" -- Scene loaded: {scene.name} ({scene.buildIndex})");

            LevelManager.Instance.InitializeLevel();

            UIManager.Instance.SetBackgroundTriggerEvent(UnityEngine.EventSystems.EventTriggerType.PointerDown, LevelManager.Instance.OnLevelStarted);

            if (OnGameSceneLoaded != null)
            {
                OnGameSceneLoaded();
            }
        }
    }

    public void ReloadGameScene()
    {
        UIManager.Instance.ResetView();

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void SetCurrency(float value, bool recount = false)
    {
        Currency = value;
    }

    public void ChangeCurrency(float increment)
    {
        SetCurrency(Currency + increment);
    }

    public void SetTimeScale(float value, float transitionDuration = 0)
    {
        if (transitionDuration == 0)
        {
            _timeScaleValue = value;

            Time.fixedDeltaTime = _defaultFixedDeltaTime * _timeScaleValue;
            Time.timeScale = _timeScaleValue;
        }
        else
        {
            _timeScaleEvaluator.Setup(Time.timeScale, value, transitionDuration, EvaluationType.Linear);
        }
    }

    private void OnValidate()
    {
        if (creativeManager)
        {
            creativeManager.gameObject.SetActive(creativeMode);
        }
    }
}
