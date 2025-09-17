using SaccFlightAndVehicles;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SaccAirVehicle))]
public class SaccAirVehicleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        EditorGUILayout.HelpBox("Custom inspector running", MessageType.Info);

        DrawDefaultInspector();
    }
}
