using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreativeManager : Service<CreativeManager>
{
    public GameObject creativeMenu;
    [Space]
    public CreativeMenuSwitchers switchers;
    public CreativeMenuSliders sliders;
    [Space]
    public CreativeModeSettings creativeModeSettings;

    private CameraController cameraController;

    private Transform displacementContainer;

    public int SkyboxIndex { get { return PlayerPrefs.GetInt("CM.SB.ID"); } set { PlayerPrefs.SetInt("CM.SB.ID", value); } }
    public int BackgroundIndex { get { return PlayerPrefs.GetInt("CM.BG.ID"); } set { PlayerPrefs.SetInt("CM.BG.ID", value); } }
    public int TrackBorderIndex { get { return PlayerPrefs.GetInt("CM.TB.ID"); } set { PlayerPrefs.SetInt("CM.TB.ID", value); } }

    public float CameraYawOffset { get { return PlayerPrefs.GetFloat("CM.CAM.Y"); } set { PlayerPrefs.SetFloat("CM.CAM.Y", value); } }
    public float CameraPitchOffset { get { return PlayerPrefs.GetFloat("CM.CAM.P"); } set { PlayerPrefs.SetFloat("CM.CAM.P", value); } }
    public float CameraRollOffset { get { return PlayerPrefs.GetFloat("CM.CAM.R"); } set { PlayerPrefs.SetFloat("CM.CAM.R", value); } }
    public float CameraHorizontalOffset { get { return PlayerPrefs.GetFloat("CM.CAM.H"); } set { PlayerPrefs.SetFloat("CM.CAM.H", value); } }
    public float CameraVerticalOffset { get { return PlayerPrefs.GetFloat("CM.CAM.V"); } set { PlayerPrefs.SetFloat("CM.CAM.V", value); } }
    public float CameraDistanceOffset { get { return PlayerPrefs.GetFloat("CM.CAM.DST"); } set { PlayerPrefs.SetFloat("CM.CAM.DST", value); } }
    //public float CameraFOV { get { return PlayerPrefs.GetFloat("CM.CAM.FOV", CameraController.Instance.MainCamera.fieldOfView); } set { PlayerPrefs.SetFloat("CM.CAM.FOV", value); } }

    public float CameraDefaultFOV { get { return PlayerPrefs.GetFloat("CM.CAM.FOV_D"); } set { PlayerPrefs.SetFloat("CM.CAM.FOV_D", value); } }

    public override void Initialize()
    {
        base.Initialize();

        GameManager.OnGameSceneLoaded += () => StartCoroutine(SettingsApplyingCoroutine());

        StartCoroutine(InitializationCoroutine());
    }

    public void SetMenuActive(bool isActive)
    {
        creativeMenu.SetActive(isActive);
    }

    public void SwitchUniqueLevel(int index)
    {
        LevelManager.Instance.LoadLevel(index);
    }

    public void SwitchSkybox(int index)
    {
        SkyboxIndex = index;

        //WorldManager.Instance.ApplySkybox(index);
    }

    public void SwitchBackground(int index)
    {
        BackgroundIndex = index;

        //WorldManager.Instance.ApplyBackground(index);
    }

    public void SwitchTrackBorder(int index)
    {
        TrackBorderIndex = index;

        //WorldManager.Instance.ApplyTrackBorder(index);
    }

    public void SwitchSkinColor(int index)
    {
        //WorldManager.Instance.ApplySkinColor(index);
    }

    public void SwitchScooterUpgrade(int index)
    {
        //WorldManager.Instance.ApplyScooterUpgrade(index);
    }

    public void SwitchGraphicsTheme(int index)
    {
        //WorldManager.Instance.ApplyGraphicsTheme(index);
    }

    public void SetCameraYawOffset(float value)
    {
        CameraYawOffset = value;

        //displacementContainer.localEulerAngles = new Vector3(cameraController.DisplacementContainer.localEulerAngles.x, value, cameraController.DisplacementContainer.localEulerAngles.z);
    }

    public void SetCameraPitchOffset(float value)
    {
        CameraPitchOffset = value;

        //displacementContainer.localEulerAngles = new Vector3(value, cameraController.DisplacementContainer.localEulerAngles.y, cameraController.DisplacementContainer.localEulerAngles.z);
    }

    public void SetCameraRollOffset(float value)
    {
        CameraRollOffset = value;

        //displacementContainer.localEulerAngles = new Vector3(cameraController.DisplacementContainer.localEulerAngles.x, cameraController.DisplacementContainer.localEulerAngles.y, value);
    }

    public void SetCameraHorizontalOffset(float value)
    {
        CameraHorizontalOffset = value;

        displacementContainer.localPosition = new Vector3(value, displacementContainer.localPosition.y, displacementContainer.localPosition.z);
    }

    public void SetCameraVerticalOffset(float value)
    {
        CameraVerticalOffset = value;

        displacementContainer.localPosition = new Vector3(displacementContainer.localPosition.x, value, displacementContainer.localPosition.z);
    }

    public void SetCameraDistance(float value)
    {
        CameraDistanceOffset = value;

        displacementContainer.localPosition = new Vector3(displacementContainer.localPosition.x, displacementContainer.localPosition.y, -value);
    }

    public void SetCameraFOV(float value)
    {
        //CameraFOV = value;

        //cameraController.ResetFOV(value);
    }

    public void ClearAllSaves()
    {
        PlayerPrefs.DeleteAll();

        GameManager.Instance.ReloadGameScene();
    }

    private void SaveDefaultValues()
    {
        //CameraDefaultFOV = CameraFOV;
    }

    private void ApplyCreativeSettings()
    {
        /*
        cameraController = CameraController.Instance;
        displacementContainer = cameraController.DisplacementContainer;

        SwitchSkybox(SkyboxIndex);
        SwitchBackground(BackgroundIndex);
        SwitchTrackBorder(TrackBorderIndex);
        SwitchSkinColor(PlayerController.SkinColorIndex);
        SwitchScooterUpgrade(PlayerController.UpgradeIndex);

        SetCameraYawOffset(CameraYawOffset);
        SetCameraPitchOffset(CameraPitchOffset);
        SetCameraRollOffset(CameraRollOffset);
        SetCameraHorizontalOffset(CameraHorizontalOffset);
        SetCameraVerticalOffset(CameraVerticalOffset);
        SetCameraDistance(CameraDistanceOffset);
        SetCameraFOV(CameraFOV);
        */
    }

    private void InitializeMenu()
    {
        /*
        switchers.uniqueLevelSwitcher.Initialize(LevelGenerator.Instance.GetLevelTitles(), LevelManager.Instance.LevelIndex);
        switchers.skyboxSwitcher.Initialize(WorldManager.Instance.GetSkyboxTitles(), SkyboxIndex);
        switchers.backgroundSwitcher.Initialize(WorldManager.Instance.GetBackgroundTitles(), BackgroundIndex);
        switchers.trackBorderSwitcher.Initialize(WorldManager.Instance.GetTrackBorderTitles(), TrackBorderIndex);
        switchers.skinColorSwitcher.Initialize(WorldManager.Instance.GetScooterColors(), PlayerController.SkinColorIndex);
        switchers.upgradeSwitcher.Initialize(WorldManager.Instance.GetScooterUpgradeTitles(), PlayerController.UpgradeIndex);

        sliders.cameraYawOffsetSlider.Initialize(CameraYawOffset);
        sliders.cameraPitchOffsetSlider.Initialize(CameraPitchOffset);
        sliders.cameraRollOffsetSlider.Initialize(CameraRollOffset);
        sliders.cameraHorizontalOffsetSlider.Initialize(CameraHorizontalOffset);
        sliders.cameraVerticalOffsetSlider.Initialize(CameraVerticalOffset);
        sliders.cameraDistanceSlider.Initialize(CameraDistanceOffset);
        sliders.cameraFOVSlider.Initialize(CameraFOV, CameraDefaultFOV);
        */
    }

    private IEnumerator InitializationCoroutine()
    {
        /*
        while (!LevelGenerator.Instance && !WorldManager.Instance) { yield return null; }

        if (AppManager.Instance.IsFirstLaunch)
        {
            SaveDefaultValues();
        }

        ApplyCreativeSettings();

        InitializeMenu();
        */

        yield return null;
    }

    private IEnumerator SettingsApplyingCoroutine()
    {
        /*
        while (!LevelGenerator.Instance && !WorldManager.Instance) { yield return null; }

        ApplyCreativeSettings();
        */

        yield return null;
    }

    [System.Serializable]
    public struct CreativeMenuSwitchers
    {
        public Switcher uniqueLevelSwitcher;
        public Switcher skyboxSwitcher;
        public Switcher backgroundSwitcher;
        public Switcher trackBorderSwitcher;
        public Switcher skinColorSwitcher;
        public Switcher upgradeSwitcher;
    }

    [System.Serializable]
    public struct CreativeMenuSliders
    {
        public QuickSlider cameraYawOffsetSlider;
        public QuickSlider cameraPitchOffsetSlider;
        public QuickSlider cameraRollOffsetSlider;
        public QuickSlider cameraHorizontalOffsetSlider;
        public QuickSlider cameraVerticalOffsetSlider;
        public QuickSlider cameraDistanceSlider;
        public QuickSlider cameraFOVSlider;
    }

    [System.Serializable]
    public class CreativeModeSettings
    {
        public bool actionPhaseUI;
    }
}
