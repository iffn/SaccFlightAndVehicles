using SaccFlightAndVehicles;
using System.Collections.Generic;
using UnityEditor;

[CustomEditor(typeof(SaccAirVehicle))]
public class SaccAirVehicleEditor : Editor
{
    bool defaultFoldout = false;

    public SaccAirVehicle LinkedSaccAirVehicle => (SaccAirVehicle)target;
    bool NotPartOfScene => EditorUtility.IsPersistent(target);

    PerformanceFoldout performanceFoldout;
    CheckFoldout checkFoldout;

    public override void OnInspectorGUI()
    {
        // This object is in a scene (not a project prefab)
        defaultFoldout = EditorGUILayout.Foldout(defaultFoldout, "Default Inspector", true);
        if (defaultFoldout)
        {
            DrawDefaultInspector();
        }

        if(checkFoldout == null)
            checkFoldout = new CheckFoldout(true, LinkedSaccAirVehicle);
        checkFoldout.DrawAsFoldout();
        
        if(performanceFoldout == null)
            performanceFoldout = new PerformanceFoldout(false, LinkedSaccAirVehicle);
        performanceFoldout.DrawAsFoldout();

    }

    class PerformanceFoldout : Foldout
    {
        SaccAirVehicle linkedSaccAIrVehicle;

        public override string DisplayName => "Performance evaluation";

        public PerformanceFoldout(bool defaultFoldout, SaccAirVehicle linkedSaccAIrVehicle) : base(defaultFoldout)
        {
            this.linkedSaccAIrVehicle = linkedSaccAIrVehicle;
        }

        public override void DrawUI()
        {
            EditorGUILayout.HelpBox("Not yet implemented", MessageType.Info);
        }
    }

    class CheckFoldout : Foldout
    {
        SaccAirVehicle linkedSaccAIrVehicle;
        
        public override string DisplayName => "Check if valid";

        public CheckFoldout(bool foldoutOpen, SaccAirVehicle linkedSaccAIrVehicle) : base(foldoutOpen)
        {
            this.linkedSaccAIrVehicle = linkedSaccAIrVehicle;
        }

        int failureCount;
        List<string> checks;
        List<bool> worked;

        void AddCheck(string check, bool worked)
        {
            checks.Add(check);
            this.worked.Add(worked);

            if(!worked)
                failureCount++;
        }

        void DoChecks()
        {
            failureCount = 0;
            checks = new List<string>();
            worked = new List<bool>();

            AddCheck(
                $"{nameof(linkedSaccAIrVehicle.EntityControl)} assigned", 
                linkedSaccAIrVehicle.EntityControl != null);
        }

        void DrawChecks()
        {
            if (failureCount == 0)
            {
                EditorGUILayout.HelpBox("No problems detected", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    $"{failureCount} errors detected", 
                    MessageType.Error);
            }

            for (int i = 0; i< checks.Count; i++)
            {
                EditorGUILayout.Toggle(checks[i], worked[i]);
            }
        }

        public override void DrawUI()
        {
            DoChecks();

            DrawChecks();
        }
    }

    abstract class Foldout
    {
        public bool foldoutOpen;
        public abstract string DisplayName { get; }

        public abstract void DrawUI();

        public Foldout(bool foldoutOpen)
        {
            this.foldoutOpen = foldoutOpen;
        }

        public void DrawAsFoldout()
        {
            if (foldoutOpen)
            {
                foldoutOpen = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutOpen, $"{DisplayName}:");
                DrawUI();
            }
            else
            {
                foldoutOpen = EditorGUILayout.BeginFoldoutHeaderGroup(foldoutOpen, DisplayName); // Only show colon when foldout is open
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        }
    }
}
