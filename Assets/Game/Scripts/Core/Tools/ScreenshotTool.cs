using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ScreenshotTool : MonoBehaviour
{
    public KeyCode screenshotKey;
    public string iOSDirectory;
    public string androidDirectory;

    private string _screenshotPath;

    #if UNITY_EDITOR

    public int ScreenShotIndex { get { return EditorPrefs.GetInt($"{(int)EditorUserBuildSettings.activeBuildTarget}_SSI", 0); } set { EditorPrefs.SetInt($"{(int)EditorUserBuildSettings.activeBuildTarget}_SSI", value); } }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        switch (EditorUserBuildSettings.activeBuildTarget)
        {
            case BuildTarget.iOS: _screenshotPath = iOSDirectory; break;
            case BuildTarget.Android: _screenshotPath = androidDirectory; break;
        }

        print($" - Screenshots directory: {_screenshotPath}");
    }


    private void LateUpdate()
    {
        if (Input.GetKeyDown(screenshotKey))
        {
            ScreenCapture.CaptureScreenshot($"{_screenshotPath}/Screenshot_{ScreenShotIndex++}.png");

            print($" - Screenshot_{ScreenShotIndex - 1}.png has been created");
        }
    }

    #endif
}
