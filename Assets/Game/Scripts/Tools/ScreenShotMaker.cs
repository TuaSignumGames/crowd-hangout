using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenShotMaker : MonoBehaviour
{


    private int screenshotIndex;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            Screen.SetResolution(512, 512, true);

            screenshotIndex = PlayerPrefs.GetInt("SSM_ID");

            ScreenCapture.CaptureScreenshot(string.Format($"Assets/Screenshots/screen_{screenshotIndex}.jpg"));

            PlayerPrefs.SetInt("SSM_ID", ++screenshotIndex);
        }
    }
}
