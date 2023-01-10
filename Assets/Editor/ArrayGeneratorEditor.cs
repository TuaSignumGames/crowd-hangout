using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ArrayGenerator))]
public class ArrayGeneratorEditor : Editor
{
    private ArrayGenerator _generator;

    private GUICustomDrawer _drawer;

    public override void OnInspectorGUI()
    {
        _generator = (ArrayGenerator)target;

        _drawer = new GUICustomDrawer();

        base.OnInspectorGUI();

        EditorGUILayout.Space();

        if (_drawer.DrawButton("Generate"))
        {
            _generator.Generate();
        }

        if (_drawer.DrawButton("Clear"))
        {
            _generator.ClearArray();
        }
    }
}
