using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
//using GameAnalyticsSDK;
//using Facebook.Unity;
using MoreMountains.NiceVibrations;

public class AppManager : Service<AppManager>//, IGameAnalyticsATTListener
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

        _isVibrationActive = IsVibrationActive;

        base.Initialize();
    }

    private void Start()
    {
        /*
        if (Application.platform == RuntimePlatform.IPhonePlayer)
        {
            GameAnalytics.RequestTrackingAuthorization(this);
        }
        else
        {
            GameAnalytics.Initialize();
        }
        
        if (FB.IsInitialized)
        {
            FB.ActivateApp();
        }
        else
        {
            FB.Init(FbInitCallback, (isUnityShown) => { });
        }
        */
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
    /*
    public void GameAnalyticsATTListenerNotDetermined()
    {
        GameAnalytics.Initialize();
    }

    public void GameAnalyticsATTListenerRestricted()
    {
        GameAnalytics.Initialize();
    }

    public void GameAnalyticsATTListenerDenied()
    {
        GameAnalytics.Initialize();
    }

    public void GameAnalyticsATTListenerAuthorized()
    {
        GameAnalytics.Initialize();
    }
    
    private void FbInitCallback()
    {
        if (FB.IsInitialized)
        {
            FB.ActivateApp();
        }
        else
        {
            Debug.Log("Failed to Initialize the Facebook SDK");
        }

        Invoke(nameof(RegisterAppForNetworkAttribution), 1);
    }
    
    private void RegisterAppForNetworkAttribution()
    {
        #if UNITY_IOS

        Unity.Advertisement.IosSupport.SkAdNetworkBinding.SkAdNetworkRegisterAppForNetworkAttribution();

        #endif
    }
    */
}
