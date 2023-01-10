using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine;

public class SceneSwitcherEditor
{
    static Scene requestScene;

    static string loadingScenePath = EditorBuildSettings.scenes[0].path;
    static string gameScenePath = EditorBuildSettings.scenes[1].path;

    [MenuItem("SceneSwitcher/Play Loading Scene _%r")]
    static void PlayLoadingScene()
    {
        requestScene = EditorSceneManager.GetActiveScene();

        EditorSceneManager.SaveScene(requestScene);

        EditorSceneManager.OpenScene(loadingScenePath);
        EditorApplication.EnterPlaymode();

        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    [MenuItem("SceneSwitcher/Switch Scene _%w")]
    static void SwitchScene()
    {
        EditorSceneManager.SaveOpenScenes();
        EditorSceneManager.OpenScene(EditorSceneManager.GetActiveScene().path == loadingScenePath ? gameScenePath : loadingScenePath);
    }

    static void OnPlayModeStateChanged(PlayModeStateChange playModeState)
    {
        if (playModeState == PlayModeStateChange.EnteredEditMode)
        {
            EditorSceneManager.OpenScene(requestScene.path);
        }
    }
}
