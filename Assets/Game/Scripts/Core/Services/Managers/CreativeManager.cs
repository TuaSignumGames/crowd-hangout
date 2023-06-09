using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CreativeManager : Service<CreativeManager>
{
    public GameObject creativeMenu;
    [Space]
    public CreativeMenuSwitchers switchers;
    public CreativeMenuInputFields fields;
    public CreativeMenuSliders sliders;
    [Space]
    public CreativeModeSettings creativeModeSettings;

    private CameraController cameraController;

    private Transform displacementContainer;

    public int LevelPatternIndex { get { return PlayerPrefs.GetInt("CM.LP.ID"); } set { PlayerPrefs.SetInt("CM.LP.ID", value); } }
    public int LevelLandscapeIndex { get { return PlayerPrefs.GetInt("CM.LL.ID"); } set { PlayerPrefs.SetInt("CM.LL.ID", value); } }
    public int HumanSkinSetIndex { get { return PlayerPrefs.GetInt("CM.HSS.ID"); } set { PlayerPrefs.SetInt("CM.HSS.ID", value); } }
    public int WeaponSetIndex { get { return PlayerPrefs.GetInt("CM.WS.ID"); } set { PlayerPrefs.SetInt("CM.WS.ID", value); } }
    public int ThemeIndex { get { return PlayerPrefs.GetInt("CM.T.ID"); } set { PlayerPrefs.SetInt("CM.T.ID", value); } }

    public int PopulationBase { get { return PlayerPrefs.GetInt("PLN.B", 1); } set { PlayerPrefs.SetInt("PLN.B", value); } }
    public int PopulationIncrement { get { return PlayerPrefs.GetInt("PLN.I", 1); } set { PlayerPrefs.SetInt("PLN.I", value); } }

    public float CameraYawOffset { get { return PlayerPrefs.GetFloat("CM.CAM.Y"); } set { PlayerPrefs.SetFloat("CM.CAM.Y", value); } }
    public float CameraPitchOffset { get { return PlayerPrefs.GetFloat("CM.CAM.P"); } set { PlayerPrefs.SetFloat("CM.CAM.P", value); } }
    public float CameraRollOffset { get { return PlayerPrefs.GetFloat("CM.CAM.R"); } set { PlayerPrefs.SetFloat("CM.CAM.R", value); } }
    public float CameraHorizontalOffset { get { return PlayerPrefs.GetFloat("CM.CAM.H"); } set { PlayerPrefs.SetFloat("CM.CAM.H", value); } }
    public float CameraVerticalOffset { get { return PlayerPrefs.GetFloat("CM.CAM.V"); } set { PlayerPrefs.SetFloat("CM.CAM.V", value); } }
    public float CameraDistanceOffset { get { return PlayerPrefs.GetFloat("CM.CAM.DST"); } set { PlayerPrefs.SetFloat("CM.CAM.DST", value); } }
    public float CameraFOV { get { return PlayerPrefs.GetFloat("CM.CAM.FOV", CameraController.Instance.camera.fieldOfView); } set { PlayerPrefs.SetFloat("CM.CAM.FOV", value); } }

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

    public void SwitchLevelPattern(int index)
    {
        LevelPatternIndex = index;

        GameManager.Instance.ReloadGameScene();
    }

    public void SwitchLevelLandscape(int index)
    {
        LevelLandscapeIndex = index;

        GameManager.Instance.ReloadGameScene();
    }

    public void SwitchTheme(int index)
    {
        ThemeIndex = index;

        GameManager.Instance.ReloadGameScene();
    }

    public void SwitchHumanSkinSet(int index)
    {
        HumanSkinSetIndex = index;

        GameManager.Instance.ReloadGameScene();
    }

    public void SwitchWeaponSet(int index)
    {
        WeaponSetIndex = index;

        GameManager.Instance.ReloadGameScene();
    }

    public void SetPopulationBase(string value)
    {
        PopulationBase = int.Parse(value);

        GameManager.Instance.ReloadGameScene();
    }

    public void SetPopulationIncrement(string value)
    {
        PopulationIncrement = int.Parse(value);

        GameManager.Instance.ReloadGameScene();
    }

    public void SetCameraYawOffset(float value)
    {
        CameraYawOffset = value;

        CameraController.Instance.displacementContainer.localEulerAngles = new Vector3(CameraController.Instance.displacementContainer.localEulerAngles.x, value, CameraController.Instance.displacementContainer.localEulerAngles.z);
    }

    public void SetCameraPitchOffset(float value)
    {
        CameraPitchOffset = value;

        CameraController.Instance.displacementContainer.localEulerAngles = new Vector3(value, CameraController.Instance.displacementContainer.localEulerAngles.y, CameraController.Instance.displacementContainer.localEulerAngles.z);
    }

    public void SetCameraRollOffset(float value)
    {
        CameraRollOffset = value;

        CameraController.Instance.displacementContainer.localEulerAngles = new Vector3(CameraController.Instance.displacementContainer.localEulerAngles.x, CameraController.Instance.displacementContainer.localEulerAngles.y, value);
    }

    public void SetCameraHorizontalOffset(float value)
    {
        CameraHorizontalOffset = value;

        CameraController.Instance.displacementContainer.localPosition = new Vector3(value, CameraController.Instance.displacementContainer.localPosition.y, CameraController.Instance.displacementContainer.localPosition.z);
    }

    public void SetCameraVerticalOffset(float value)
    {
        CameraVerticalOffset = value;

        CameraController.Instance.displacementContainer.localPosition = new Vector3(CameraController.Instance.displacementContainer.localPosition.x, value, CameraController.Instance.displacementContainer.localPosition.z);
    }

    public void SetCameraDistance(float value)
    {
        CameraDistanceOffset = value;

        CameraController.Instance.displacementContainer.localPosition = new Vector3(CameraController.Instance.displacementContainer.localPosition.x, CameraController.Instance.displacementContainer.localPosition.y, -value);
    }

    public void SetCameraFOV(float value)
    {
        CameraFOV = value;

        CameraController.Instance.camera.fieldOfView = value;
    }

    public void ClearAllSaves()
    {
        PlayerPrefs.DeleteAll();

        GameManager.Instance.ReloadGameScene();
    }

    private void SaveDefaultValues()
    {
        CameraDefaultFOV = CameraController.Instance.camera.fieldOfView;
    }

    private void ApplyCreativeSettings()
    {
        cameraController = CameraController.Instance;
        displacementContainer = cameraController.displacementContainer;

        SetCameraYawOffset(CameraYawOffset);
        SetCameraPitchOffset(CameraPitchOffset);
        SetCameraRollOffset(CameraRollOffset);
        SetCameraHorizontalOffset(CameraHorizontalOffset);
        SetCameraVerticalOffset(CameraVerticalOffset);
        SetCameraDistance(CameraDistanceOffset);
        SetCameraFOV(CameraFOV);
    }

    private void InitializeMenu()
    {
        switchers.levelPatternSwitcher.Initialize(LevelGenerator.Instance.levelSettings.GetStructureTitles(), LevelPatternIndex);
        switchers.levelLandscapeSwitcher.Initialize(LevelGenerator.Instance.levelSettings.GetLandscapeTitles(), LevelLandscapeIndex);
        switchers.humanSkinSetSwitcher.Initialize(WorldManager.GetHumanSkinSetTitles(), HumanSkinSetIndex);
        switchers.weaponSetSwitcher.Initialize(WorldManager.GetWeaponSetTitles(), WeaponSetIndex);
        switchers.themeSwitcher.Initialize(WorldManager.environmentSettings.GetThemeTitles(), ThemeIndex);

        fields.populationBaseField.text = PopulationBase.ToString();
        fields.populationIncrementField.text = PopulationIncrement.ToString();

        sliders.cameraYawOffsetSlider.Initialize(CameraYawOffset);
        sliders.cameraPitchOffsetSlider.Initialize(CameraPitchOffset);
        sliders.cameraRollOffsetSlider.Initialize(CameraRollOffset);
        sliders.cameraHorizontalOffsetSlider.Initialize(CameraHorizontalOffset);
        sliders.cameraVerticalOffsetSlider.Initialize(CameraVerticalOffset);
        sliders.cameraDistanceSlider.Initialize(CameraDistanceOffset);
        sliders.cameraFOVSlider.Initialize(CameraFOV, CameraDefaultFOV);
    }

    private IEnumerator InitializationCoroutine()
    {
        while (!LevelGenerator.Instance && !WorldManager.Instance) { yield return null; }

        if (AppManager.Instance.IsFirstLaunch)
        {
            SaveDefaultValues();
        }

        ApplyCreativeSettings();

        InitializeMenu();

        yield return null;
    }

    private IEnumerator SettingsApplyingCoroutine()
    {
        while (!LevelGenerator.Instance && !WorldManager.Instance) { yield return null; }

        ApplyCreativeSettings();

        yield return null;
    }

    [System.Serializable]
    public struct CreativeMenuSwitchers
    {
        public Switcher levelPatternSwitcher;
        public Switcher levelLandscapeSwitcher;
        public Switcher themeSwitcher;
        public Switcher humanSkinSetSwitcher;
        public Switcher weaponSetSwitcher;
    }

    [System.Serializable]
    public struct CreativeMenuInputFields
    {
        public InputField populationBaseField;
        public InputField populationIncrementField;
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
