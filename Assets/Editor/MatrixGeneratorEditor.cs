using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MatrixGenerator))]
public class MatrixGeneratorEditor : Editor
{
    private MatrixGenerator _generator;

    private GUICustomDrawer _drawer;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        _generator = (MatrixGenerator)target;
        _drawer = new GUICustomDrawer();

        EditorGUILayout.Space();

        if (_drawer.DrawButton("Generate"))
        {
            _generator.Generate();

            //Undo.RegisterCreatedObjectUndo(newTrackPart.gameObject, $"Create {newTrackPart.gameObject.name}");
        }
    }
}
