
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.Manual)]
public class SaccRaceCourseAndScoreboard : UdonSharpBehaviour
{
    //Unity assignments
    [Tooltip("Can be used by other scripts to get the races name.")]
    public string RaceName;
    [Tooltip("All checkpoint objects for this race in order, animations are sent to them as they are passed")]
    public GameObject[] RaceCheckpoints;
    [Tooltip("Parent of all objects related to this race, including scoreboard and checkpoints")]
    public bool AllowReverse = true;
    [SerializeField] UdonBehaviour[] LinkedCustomBehavior;

    //Runtime variables
    Text TimeText;
    [HideInInspector] public GameObject LinkedScoreboard;

    //Script sharing
    [HideInInspector] public SaccRacingTrigger ActiveRacingTrigger;

    public void AttachScoreboard(RaceCanvasController linkedController)
    {
        TimeText = linkedController.LinkedTimeText;
        linkedController.LinkedTitle.text = $"Race\n{RaceName}";
        LinkedScoreboard = linkedController.gameObject;
    }

    [System.NonSerializedAttribute, UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(InstanceRecord))] public string _InstanceRecord = "Instance Record : None";
    public string InstanceRecord
    {
        set
        {
            _InstanceRecord = value;
            UpdateTimes();
        }
        get => _InstanceRecord;
    }

    [System.NonSerializedAttribute, UdonSynced(UdonSyncMode.None), FieldChangeCallback(nameof(InstanceRecordReverse))] public string _InstanceRecordReverse = "(R)Instance Record : None";
    public string InstanceRecordReverse
    {
        set
        {
            _InstanceRecordReverse = value;
            UpdateTimes();
        }
        get => _InstanceRecordReverse;
    }
    
    //Synced variables
    [System.NonSerializedAttribute][UdonSynced(UdonSyncMode.None)] public float BestTime = Mathf.Infinity;
    [System.NonSerializedAttribute][UdonSynced(UdonSyncMode.None)] public float BestTimeReverse = Mathf.Infinity;

    //Script interactions
    [System.NonSerializedAttribute] public string MyRecord = "My Record : None";
    [System.NonSerializedAttribute] public string MyLastTime = "My Last Time : None";
    [System.NonSerializedAttribute] public string MyName = "No-one";
    [System.NonSerializedAttribute] public string MyRecordReverse = "(R)My Record : None";
    [System.NonSerializedAttribute] public string MyLastTimeReverse = "(R)My Last Time : None";
    [System.NonSerializedAttribute] public float MyRecordTime = Mathf.Infinity;
    [System.NonSerializedAttribute] public float MyRecordTimeReverse = Mathf.Infinity;
    [System.NonSerializedAttribute] public float MyTime = Mathf.Infinity;
    [System.NonSerializedAttribute] public float MyTimeReverse = Mathf.Infinity;
    [System.NonSerializedAttribute] public string MyVehicleType = "Vehicle";

    public void Setup()
    {
        UpdateMyLastTime();
        UpdateMyRecord();
        UpdateInstanceRecord();
        UpdateInstanceRecordReverse();
        UpdateTimes();
        if (Networking.LocalPlayer != null)
        {
            MyName = Networking.LocalPlayer.displayName;
        }
    }

    public void RaceStarted(SaccRacingTrigger activeRacingTrigger)
    {
        this.ActiveRacingTrigger = activeRacingTrigger;

        foreach (UdonBehaviour behavior in LinkedCustomBehavior)
        {
            behavior.SendCustomEvent("RaceStarted");
        }
    }

    public void CheckpointPassed()
    {
        foreach (UdonBehaviour behavior in LinkedCustomBehavior)
        {
            behavior.SendCustomEvent("CheckpointPassed");
        }
    }

    public void RaceFinished(bool newRecord)
    {
        if (newRecord)
        {
            foreach (UdonBehaviour behavior in LinkedCustomBehavior)
            {
                behavior.SendCustomEvent("RaceFinishedWithRecord");
            }
        }
        else
        {
            foreach (UdonBehaviour behavior in LinkedCustomBehavior)
            {
                behavior.SendCustomEvent("RaceFinishedWithoutRecord");
            }
        }
        
    }

    public void RaceCanceled()
    {
        foreach (UdonBehaviour behavior in LinkedCustomBehavior)
        {
            behavior.SendCustomEvent("RaceCanceled");
        }
    }

    public void UpdateMyLastTime()
    {
        MyLastTime = string.Concat("My Last Time : ", MyVehicleType, " : ", MyTime);

        if (AllowReverse)
        {
            MyLastTimeReverse = string.Concat("(R)My Last Time : ", MyVehicleType, " : ", MyTimeReverse);
        }
        else
        {
            MyLastTimeReverse = string.Empty;
        }
        UpdateTimes();
    }

    public void UpdateMyRecord()
    {
        MyRecord = string.Concat("My Record : ", MyVehicleType, " : ", MyTime);
        if (AllowReverse)
        {
            MyRecordReverse = string.Concat("(R)My Record : ", MyVehicleType, " : ", MyTimeReverse);
        }
        else
        {
            MyRecordReverse = string.Empty;
        }
        UpdateTimes();
    }

    public void UpdateInstanceRecord()
    {
        InstanceRecord = string.Concat("Instance Record : ", MyName, " : ", MyVehicleType, " : ", BestTime);
    }

    public void UpdateInstanceRecordReverse()
    {
        InstanceRecordReverse = string.Concat("(R)Instance Record : ", MyName, " : ", MyVehicleType, " : ", BestTimeReverse);
    }

    public void UpdateTimes()
    {
        TimeText.text = string.Concat(InstanceRecord, "\n", MyRecord, "\n", MyLastTime);
        if (AllowReverse)
        {
            TimeText.text = string.Concat(TimeText.text, "\n", InstanceRecordReverse, "\n", MyRecordReverse, "\n", MyLastTimeReverse);
        }
    }

    public override void OnPlayerJoined(VRCPlayerApi player)
    {
        RequestSerialization();
    }
}