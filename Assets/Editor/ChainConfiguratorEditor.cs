using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ChainConfigurator))]
public class ChainConfiguratorEditor : Editor
{
    private ChainConfigurator _chainConfigurator;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (!_chainConfigurator)
        {
            _chainConfigurator = (ChainConfigurator)target;
        }

        GUILayout.Space(8f);

        if (GUILayout.Button("Create Chain"))
        {
            _chainConfigurator.Configurate();
        }

        if (GUILayout.Button("Clear"))
        {
            _chainConfigurator.Clear();
        }
    }
}
