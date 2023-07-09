using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BattlePathGenerator))]
public class BattlePathGeneratorEditor : Editor
{
    private BattlePathGenerator generator;

    private GUICustomDrawer drawer = new GUICustomDrawer();

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        generator = (BattlePathGenerator)target;

        EditorGUILayout.Space();

        if (drawer.DrawButton("Generate"))
        {
            generator.Generate();
        }
    }
}
