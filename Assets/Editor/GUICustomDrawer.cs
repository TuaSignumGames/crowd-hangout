using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class GUICustomDrawer
{
    private GUIStyle _labelStyle;

    private bool _guiButtonPressed;

    public bool DrawButton(string text, float height = 25f, bool enabled = true)
    {
        GUI.enabled = enabled;

        _guiButtonPressed = GUILayout.Button(text, GUILayout.Height(height));

        GUI.enabled = true;

        return _guiButtonPressed;
    }

    public bool DrawButton(string text, float width, float height, bool enabled = true)
    {
        GUILayout.BeginHorizontal();

        GUILayout.FlexibleSpace();

        GUI.enabled = enabled;

        _guiButtonPressed = GUILayout.Button(text, GUILayout.Width(width), GUILayout.Height(height));

        GUI.enabled = true;

        GUILayout.FlexibleSpace();

        GUILayout.EndHorizontal();

        return _guiButtonPressed;
    }

    public bool DrawPopup(string popupTitle, string[] options, ref int selectedOptionIndex)
    {
        EditorGUI.BeginChangeCheck();

        selectedOptionIndex = EditorGUILayout.Popup(popupTitle, selectedOptionIndex, options);

        return EditorGUI.EndChangeCheck();
    }

    public void DrawLine()
    {
        EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 1), new Color(0.078f, 0.078f, 0.078f));
    }

    public void DrawLine(string text)
    {
        _labelStyle = new GUIStyle();

        _labelStyle.normal.textColor = Color.white;
        _labelStyle.fontStyle = FontStyle.Bold;

        GUILayout.Label(text, _labelStyle);

        EditorGUI.DrawRect(EditorGUILayout.GetControlRect(false, 1), new Color(0.078f, 0.078f, 0.078f));
    }
}
