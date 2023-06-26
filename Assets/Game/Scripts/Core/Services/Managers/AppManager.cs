using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using MoreMountains.NiceVibrations;
using SupersonicWisdomSDK;

public class AppManager : Service<AppManager>
{
    [SerializeField] private int _targetFramerate;
    [SerializeField] private int _gameEntrySceneIndex;

    private float _nextAvailableHapticTime;

    private bool _isFirstLaunch;
    private bool _isVibrationActive;

    public int GameEntrySceneIndex { get { return _gameEntrySceneIndex; } }

    public bool IsVibrationActive { get { return PlayerPrefs.GetInt("APP_VBR", 1) == 1; } set { PlayerPrefs.SetInt("APP_VBR", value == true ? 1 : 0); } }

    public bool IsFirstLaunch => _isFirstLaunch;

    public const float hapticBlockingTime = 0.05f;

    public override void Initialize()
    {
        Application.targetFrameRate = _targetFramerate;

        _isFirstLaunch = !PlayerPrefs.HasKey("APP_FLF");

        if (_isFirstLaunch)
        {
            PlayerPrefs.SetInt("APP_FLF", 1);
        }

//#if !UNITY_EDITOR

        SupersonicWisdom.Api.AddOnReadyListener(LoadGameScene);
        SupersonicWisdom.Api.Initialize();

//#endif

        _isVibrationActive = IsVibrationActive;

        base.Initialize();
    }

    public void LoadGameScene()
    {
        SceneManager.sceneLoaded += GameManager.Instance.InitializeLevel;
        SceneManager.LoadScene(_gameEntrySceneIndex);
    }

    public void PlayHaptic(HapticTypes hapticType)
    {
        if (_isVibrationActive && Time.realtimeSinceStartup >= _nextAvailableHapticTime)
        {
            _nextAvailableHapticTime = Time.realtimeSinceStartup + hapticBlockingTime;

            MMVibrationManager.Haptic(hapticType);
        }    
    }

    public void PlayVibration(long seconds)
    {
        if (_isVibrationActive)
        {
            MMVibrationManager.AndroidVibrate(seconds * 1000);
        }
    }

    public void SetVibration(bool enabled)
    {
        _isVibrationActive = enabled;

        IsVibrationActive = _isVibrationActive;
    }
}
