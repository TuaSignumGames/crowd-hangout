using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LevelGenerator))]
public class LevelGeneratorEditor : Editor
{
    private LevelGenerator generator;

    private GUICustomDrawer drawer = new GUICustomDrawer();

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        /*
        generator = (LevelGenerator)target;

        EditorGUILayout.Space();

        if (drawer.DrawButton("Generate"))
        {
            GenerateLevel();
        }
        */
    }

    private void GenerateLevel()
    {
        generator.GenerateFromEditor(false, false);
    }
}
