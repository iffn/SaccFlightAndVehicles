
using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;

[UdonBehaviourSyncMode(BehaviourSyncMode.None)]
public class SaccRaceToggleButton : UdonSharpBehaviour
{
    //Unity assignments
    [Tooltip("Can be used to set a default course -1 = none")]
    public int CurrentCourseSelection = -1;
    public GameObject EnableWhenNoneSelected;
    [SerializeField] RaceCanvasController ScoreboardTemplate;

    //Script sharing
    [HideInInspector] public SaccRacingTrigger[] RacingTriggers;
    [HideInInspector] public SaccRaceCourseAndScoreboard[] Races;
    
    //Runtime variables
    private bool Reverse = false;

    private void Start()
    {
        foreach (SaccRaceCourseAndScoreboard race in Races)
        {
            RaceCanvasController newScoreboard = GameObject.Instantiate(ScoreboardTemplate.gameObject, transform).transform.GetComponent<RaceCanvasController>();
            //newScoreboard.transform.localPosition = Vector3.up * 1.2f;
            race.AttachScoreboard(newScoreboard);
            race.Setup();
        }

        if (CurrentCourseSelection == -1) //-1 = all races disabled
        {
            foreach (SaccRaceCourseAndScoreboard race in Races)
            {
                race.gameObject.SetActive(false);
                race.LinkedScoreboard.SetActive(false);
            }
        }
        else
        {
            if (CurrentCourseSelection != -1)
            {
                foreach (SaccRacingTrigger RaceTrig in RacingTriggers)
                { 
                    RaceTrig.gameObject.SetActive(true);
                }
            }
        }

        SetRace();
    }

    /*
    public override void Interact() //ToDo: Check if still being used
    {
        NextRace();
    }
    */

    public void NextRace() //Referenced in Unity as a button function
    {
        if (CurrentCourseSelection != -1) 
        { 
            Races[CurrentCourseSelection].gameObject.SetActive(false);
            Races[CurrentCourseSelection].LinkedScoreboard.SetActive(false);
        }

        if (CurrentCourseSelection == Races.Length - 1)
        { 
            CurrentCourseSelection = -1; 
        }
        else 
        { 
            CurrentCourseSelection++; 
        }

        SetRace();
    }

    public void PreviousRace() //Referenced in Unity as a button function
    {
        if (CurrentCourseSelection != -1)
        {
            Races[CurrentCourseSelection].gameObject.SetActive(false);
            Races[CurrentCourseSelection].LinkedScoreboard.SetActive(false);
        }

        if (CurrentCourseSelection == -1)
        {
            CurrentCourseSelection = Races.Length - 1;
        }
        else 
        {
            CurrentCourseSelection--;
        }

        SetRace();
    }

    void SetRace()
    {
        if (CurrentCourseSelection != -1)//-1 = all races disabled
        {
            if (EnableWhenNoneSelected) { EnableWhenNoneSelected.SetActive(false); }
            SaccRaceCourseAndScoreboard race = Races[CurrentCourseSelection].GetComponent<SaccRaceCourseAndScoreboard>();
            race.gameObject.SetActive(true);
            race.LinkedScoreboard.SetActive(true);
            race.UpdateTimes();

            foreach (SaccRacingTrigger RaceTrig in RacingTriggers)
            {
                RaceTrig.gameObject.SetActive(true);
            }
        }
        else
        {
            if (EnableWhenNoneSelected) { EnableWhenNoneSelected.SetActive(true); }
            foreach (SaccRacingTrigger RaceTrig in RacingTriggers)
            {
                RaceTrig.gameObject.SetActive(false);
            }
        }

        foreach (SaccRacingTrigger RaceTrig in RacingTriggers)
        {
            RaceTrig.SetUpNewRace();
        }
    }

    public void ToggleReverse() //Referenced in Unity as a button function
    {
        if (!Reverse)
        {
            Reverse = true;
            foreach (SaccRacingTrigger RaceTrig in RacingTriggers)
            {
                RaceTrig.SetProgramVariable("_TrackForward", false);
            }
        }
        else
        {
            Reverse = false;
            foreach (SaccRacingTrigger RaceTrig in RacingTriggers)
            {
                RaceTrig.SetProgramVariable("_TrackForward", true);
            }
        }

        SetRace();
    }
}
