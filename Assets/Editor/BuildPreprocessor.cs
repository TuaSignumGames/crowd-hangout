using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class BuildPreprocessor : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        NetworkHub networkHub = GameObject.FindObjectOfType<NetworkHub>();

        if (networkHub)
        {
            networkHub.hubServerAddress = NetworkManager.GetLocalAddress().ToString();
            networkHub.hubServerPort = EditorPrefs.GetInt(NetworkHub.previousHubServerPortKey);

            Debug.Log($" - BuildPreprocessor: Network Hub server address added to client ({networkHub.hubServerAddress} : {networkHub.hubServerPort})");
        }
    }
}
