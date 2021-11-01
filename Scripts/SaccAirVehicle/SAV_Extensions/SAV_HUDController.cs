
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;
using UnityEngine.UI;

[UdonBehaviourSyncMode(BehaviourSyncMode.NoVariableSync)]
public class SAV_HUDController : UdonSharpBehaviour
{
    [Tooltip("Transform of the pilot seat's target eye position, HUDController is automatically moved to this position in Start() to ensure perfect alignment. Not required")]
    [SerializeField] private Transform PilotSeatAdjusterTarget;
    [SerializeField] private UdonSharpBehaviour SAVControl;
    [SerializeField] private Animator HUDAnimator;
    [SerializeField] private Text HUDText_G;
    [SerializeField] private Text HUDText_mach;
    [SerializeField] private Text HUDText_altitude;
    [SerializeField] private Text HUDText_knots;
    [SerializeField] private Text HUDText_knotsairspeed;
    [SerializeField] private Text HUDText_angleofattack;
    [Tooltip("Hud element that points toward the gruond")]
    [SerializeField] private Transform DownIndicator;
    [Tooltip("Hud element that shows pitch angle")]
    [SerializeField] private Transform ElevationIndicator;
    [Tooltip("Hud element that shows yaw angle")]
    [SerializeField] private Transform HeadingIndicator;
    [Tooltip("Hud element that shows vehicle's direction of movement")]
    [SerializeField] private Transform VelocityIndicator;
    private SaccEntity EntityControl;
    [Tooltip("Local distance projected forward for objects that move dynamically, only adjust if the hud is moved forward in order to make it appear smaller")]
    public float distance_from_head = 1.333f;
    private float maxGs = 0f;
    private Vector3 startingpos;
    private float check = 0;
    private int showvel;
    private Vector3 TargetDir = Vector3.zero;
    private Vector3 TargetSpeed;
    private float FullGunAmmoDivider;
    private Transform VehicleTransform;
    private SAV_EffectsController EffectsControl;
    private float SeaLevel;
    private Transform CenterOfMass;
    VRCPlayerApi localPlayer;
    private Vector3 Vel_Lerper;
    private float Vel_UpdateInterval;
    private float Vel_UpdateTime;
    private Vector3 Vel_PredictedCurVel;
    private Vector3 Vel_LastCurVel;
    private Vector3 Vel_NormalizedExtrapDir;
    private void Start()
    {

        EntityControl = (SaccEntity)SAVControl.GetProgramVariable("EntityControl");
        HUDAnimator = EntityControl.GetComponent<Animator>();
        VehicleTransform = EntityControl.transform;

        if (PilotSeatAdjusterTarget) { transform.position = PilotSeatAdjusterTarget.position; }

        SeaLevel = (float)SAVControl.GetProgramVariable("SeaLevel");
        CenterOfMass = EntityControl.CenterOfMass;

        localPlayer = Networking.LocalPlayer;
    }
    private void OnEnable()
    {
        maxGs = 0f;
    }
    private void LateUpdate()
    {
        float SmoothDeltaTime = Time.smoothDeltaTime;

        //Velocity indicator
        Vector3 currentvel = (Vector3)SAVControl.GetProgramVariable("CurrentVel");
        if (currentvel.magnitude < 2)
        {
            currentvel = -Vector3.up * 2;//straight down instead of spazzing out when moving very slow
        }
        else
        {
            //extrapolate CurrentVel and lerp towards it to smooth out the velocity indicator of non-owners
            if (currentvel != Vel_LastCurVel)
            {
                float tim = Time.time;
                Vel_UpdateInterval = tim - Vel_UpdateTime;
                Vel_NormalizedExtrapDir = (currentvel - Vel_LastCurVel) * (1 / Vel_UpdateInterval);
                Vel_LastCurVel = currentvel;
                Vel_UpdateTime = tim;
            }
            Vel_PredictedCurVel = currentvel + (Vel_NormalizedExtrapDir * (Time.time - Vel_UpdateTime));
        }

        if ((bool)SAVControl.GetProgramVariable("IsOwner"))
        {
            VelocityIndicator.position = transform.position + currentvel;
        }
        else
        {
            Vel_Lerper = Vector3.Lerp(Vel_Lerper, Vel_PredictedCurVel, 9f * Time.smoothDeltaTime);
            VelocityIndicator.position = transform.position + Vel_Lerper;
        }
        VelocityIndicator.localPosition = VelocityIndicator.localPosition.normalized * distance_from_head;
        /////////////////


        //Heading indicator
        Vector3 VehicleEuler = EntityControl.transform.rotation.eulerAngles;
        HeadingIndicator.localRotation = Quaternion.Euler(new Vector3(0, -VehicleEuler.y, 0));
        /////////////////

        //Elevation indicator
        ElevationIndicator.rotation = Quaternion.Euler(new Vector3(0, VehicleEuler.y, 0));
        /////////////////

        //Down indicator
        DownIndicator.localRotation = Quaternion.Euler(new Vector3(0, 0, -VehicleEuler.z));
        /////////////////

        //updating numbers 3~ times a second
        if (check > .3)//update text
        {
            if (Mathf.Abs(maxGs) < Mathf.Abs((float)SAVControl.GetProgramVariable("VertGs")))
            { maxGs = (float)SAVControl.GetProgramVariable("VertGs"); }
            HUDText_G.text = string.Concat(((float)SAVControl.GetProgramVariable("VertGs")).ToString("F1"), "\n", maxGs.ToString("F1"));
            HUDText_mach.text = (((float)SAVControl.GetProgramVariable("Speed")) / 343f).ToString("F2");
            HUDText_altitude.text = string.Concat((((Vector3)SAVControl.GetProgramVariable("CurrentVel")).y * 60 * 3.28084f).ToString("F0"), "\n", ((CenterOfMass.position.y - SeaLevel) * 3.28084f).ToString("F0"));
            HUDText_knots.text = (((float)SAVControl.GetProgramVariable("Speed")) * 1.9438445f).ToString("F0");
            HUDText_knotsairspeed.text = (((float)SAVControl.GetProgramVariable("AirSpeed")) * 1.9438445f).ToString("F0");

            if ((float)SAVControl.GetProgramVariable("Speed") < 2)
            {
                HUDText_angleofattack.text = System.String.Empty;
            }
            else
            {
                HUDText_angleofattack.text = ((float)SAVControl.GetProgramVariable("AngleOfAttack")).ToString("F0");
            }
            check = 0;
        }
        check += SmoothDeltaTime;
    }
}