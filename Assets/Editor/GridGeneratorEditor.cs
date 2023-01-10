using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GridGenerator))]
public class GridGeneratorEditor : Editor
{
    private GridGenerator _generator;

    private GUICustomDrawer _drawer;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        _generator = (GridGenerator)target;
        _drawer = new GUICustomDrawer();

        EditorGUILayout.Space();

        if (_drawer.DrawButton("Generate"))
        {
            _generator.Generate();
        }
    }
}
