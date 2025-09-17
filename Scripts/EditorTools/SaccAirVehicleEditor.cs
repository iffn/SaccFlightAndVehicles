using SaccFlightAndVehicles;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SaccAirVehicle))]
public class SaccAirVehicleEditor : Editor
{
    bool defaultFoldout = false;

    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox("Custom inspector running", MessageType.Info);

        // This object is in a scene (not a project prefab)
        defaultFoldout = EditorGUILayout.Foldout(defaultFoldout, "Default Inspector", true);
        if (defaultFoldout)
        {
            DrawDefaultInspector();
        }
    }
}
