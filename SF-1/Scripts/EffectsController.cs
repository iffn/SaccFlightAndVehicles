
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class EffectsController : UdonSharpBehaviour
{
    public GameObject VehicleMainObj;
    public EngineController EngineControl;
    public Transform JoyStick;
    public Transform FrontWheel;
    public ParticleSystem[] DisplaySmoke;
    public ParticleSystem CatapultSteam;
    private bool VehicleMainObjNull = true;
    private bool EngineControlNull = true;
    private bool JoyStickNull = true;
    [System.NonSerializedAttribute] public bool FrontWheelNull = true;
    private bool CatapultSteamNull = true;
    private bool DisplaySmokeNull = true;



    //these used to be synced variables, might move them back to EngineController some time
    [System.NonSerializedAttribute] public bool AfterburnerOn;
    [System.NonSerializedAttribute] public bool CanopyOpen = true;
    [System.NonSerializedAttribute] public bool GearUp = false;
    [System.NonSerializedAttribute] public bool Flaps = true;
    [System.NonSerializedAttribute] public bool HookDown = false;
    [System.NonSerializedAttribute] public bool Smoking = false;
    [UdonSynced(UdonSyncMode.Linear)] private Vector3 rotationinputs;

    private bool vapor;
    private float Gs_trail = 1000; //ensures it wont cause effects at first frame
    [System.NonSerializedAttribute] public Animator PlaneAnimator;
    [System.NonSerializedAttribute] public float AirbrakeLerper;
    [System.NonSerializedAttribute] public float DoEffects = 6f; //4 seconds before sleep so late joiners see effects if someone is already piloting
    private float brake;
    private Color SmokeColorLerper = Color.white;
    [System.NonSerializedAttribute] public bool LargeEffectsOnly = false;
    private float FullHealthDivider;
    private Vector3 OwnerRotationInputs;
    private void Start()
    {
        Assert(VehicleMainObj != null, "Start: VehicleMainObj != null");
        Assert(EngineControl != null, "Start: EngineControl != null");
        //should work withouth these
        Assert(JoyStick != null, "Start: JoyStick != null");
        Assert(FrontWheel != null, "Start: FrontWheel != null");


        /*         Assert(ElevonL != null, "Start: ElevonL != null");
                Assert(ElevonR != null, "Start: ElevonR != null");
                Assert(RuddervatorL != null, "Start: RuddervatorL != null");
                Assert(RuddervatorR != null, "Start: RuddervatorR != null");
         */


        if (VehicleMainObj != null) VehicleMainObjNull = false;
        if (EngineControl != null) EngineControlNull = false;
        if (JoyStick != null) JoyStickNull = false;
        if (FrontWheel != null) FrontWheelNull = false;
        if (CatapultSteam != null) CatapultSteamNull = false;
        if (DisplaySmoke.Length > 0) DisplaySmokeNull = false;


        /*         if (ElevonL != null) ElevonLNull = false;
                if (ElevonR != null) ElevonRNull = false;
                if (RuddervatorL != null) RuddervatorLNull = false;
                if (RuddervatorR != null) RuddervatorRNull = false;
         */

        FullHealthDivider = 1f / EngineControl.Health;

        PlaneAnimator = VehicleMainObj.GetComponent<Animator>();
    }
    private void Update()
    {
        if (DoEffects > 10) { return; }

        //if a long way away just skip effects except large vapor effects
        if (LargeEffectsOnly = (EngineControl.SoundControl.ThisFrameDist > 2000f && !EngineControl.IsOwner)) { LargeEffects(); return; }
        Effects();
        LargeEffects();
    }
    public void Effects()
    {
        Vector3 RotInputs = EngineControl.RotationInputs;
        float DeltaTime = Time.deltaTime;
        if (EngineControl.IsOwner)
        {
            OwnerRotationInputs = Vector3.MoveTowards(OwnerRotationInputs, EngineControl.RotationInputs, 7 * DeltaTime);
            PlaneAnimator.SetFloat("pitchinput", (OwnerRotationInputs.x * 0.5f) + 0.5f);
            PlaneAnimator.SetFloat("yawinput", (OwnerRotationInputs.y * 0.5f) + 0.5f);
            PlaneAnimator.SetFloat("rollinput", (OwnerRotationInputs.z * 0.5f) + 0.5f);
            PlaneAnimator.SetFloat("throttle", EngineControl.ThrottleInput);
            PlaneAnimator.SetFloat("engineoutput", EngineControl.EngineOutput);
        }
        else
        {
            float EngineOutput = EngineControl.EngineOutput;
            PlaneAnimator.SetFloat("pitchinput", (RotInputs.x * 0.5f) + 0.5f);
            PlaneAnimator.SetFloat("yawinput", (RotInputs.y * 0.5f) + 0.5f);
            PlaneAnimator.SetFloat("rollinput", (RotInputs.z * 0.5f) + 0.5f);
            PlaneAnimator.SetFloat("throttle", EngineOutput);//non-owners use value that is similar, but smoothed and would feel bad if the pilot used it himself
            PlaneAnimator.SetFloat("engineoutput", EngineOutput);
        }
        if (EngineControl.Occupied == true)
        {
            DoEffects = 0f;

            if (!FrontWheelNull)
            {
                if (EngineControl.Taxiing)
                {
                    FrontWheel.localRotation = Quaternion.Euler(new Vector3(0, -EngineControl.RotationInputs.y * 4f * (-Mathf.Min((EngineControl.Speed / 10), 1) + 1), 0));
                }
                else FrontWheel.localRotation = Quaternion.identity;
            }
        }
        else { DoEffects += DeltaTime; }


        PlaneAnimator.SetFloat("engineoutput", EngineControl.EngineOutput);

        vapor = (EngineControl.Speed > 20) ? true : false;// only make vapor when going above "20m/s", prevents vapour appearing when taxiing into a wall or whatever

        AirbrakeLerper = Mathf.Lerp(AirbrakeLerper, EngineControl.BrakeInput, 1.3f * DeltaTime);

        PlaneAnimator.SetFloat("health", EngineControl.Health * FullHealthDivider);
        PlaneAnimator.SetFloat("AoA", vapor ? Mathf.Abs(EngineControl.AngleOfAttack * 0.00555555556f /* Divide by 180 */ ) : 0);
        PlaneAnimator.SetFloat("brake", AirbrakeLerper);
    }

    private void LargeEffects()//large effects visible from a long distance
    {
        float DeltaTime = Time.deltaTime;
        PlaneAnimator.SetBool("gunfiring", EngineControl.IsFiringGun);
        if (EngineControl.Occupied)
        {
            if (Smoking && !DisplaySmokeNull)
            {
                SmokeColorLerper = Color.Lerp(SmokeColorLerper, EngineControl.SmokeColor_Color, 5 * DeltaTime);
                foreach (ParticleSystem smoke in DisplaySmoke)
                {
                    var main = smoke.main;
                    main.startColor = new ParticleSystem.MinMaxGradient(SmokeColorLerper, SmokeColorLerper * .8f);
                }
            }
        }

        //this is to finetune when wingtrails appear and disappear
        if (EngineControl.Gs >= Gs_trail) //Gs are increasing
        {
            Gs_trail = Mathf.Lerp(Gs_trail, EngineControl.Gs, 30f * DeltaTime);//apear fast when pulling Gs
        }
        else //Gs are decreasing
        {
            Gs_trail = Mathf.Lerp(Gs_trail, EngineControl.Gs, 2.7f * DeltaTime);//linger for a bit before cutting off
        }
        //("mach10", EngineControl.Speed / 343 / 10)
        PlaneAnimator.SetFloat("mach10", EngineControl.Speed * 0.000291545189504373f);//should be airspeed but nonlocal players don't have it
        //("Gs", vapor ? EngineControl.Gs / 50 : 0)
        PlaneAnimator.SetFloat("Gs", vapor ? EngineControl.Gs * 0.02f : 0);
        //("Gs_trail", vapor ? Gs_trail / 50 : 0);
        PlaneAnimator.SetFloat("Gs_trail", vapor ? Gs_trail * 0.02f : 0);
    }
    public void EffectsResetStatus()//called from enginecontroller.explode();
    {
        DoEffects = 6;
        PlaneAnimator.SetInteger("weapon", 4);
        PlaneAnimator.SetFloat("bombs", 1);
        PlaneAnimator.SetFloat("AAMs", 1);
        PlaneAnimator.SetFloat("AGMs", 1);
        PlaneAnimator.SetTrigger("respawn");//this animation disables EngineControl.dead after 5s
        PlaneAnimator.SetTrigger("instantgeardown");
        if (!FrontWheelNull) FrontWheel.localRotation = Quaternion.identity;
    }
    public void EffectsExplode()//called from enginecontroller.explode();
    {
        PlaneAnimator.SetTrigger("explode");
        PlaneAnimator.SetFloat("bombs", 1);
        PlaneAnimator.SetFloat("AAMs", 1);
        PlaneAnimator.SetFloat("AGMs", 1);
        PlaneAnimator.SetInteger("missilesincoming", 0);
        PlaneAnimator.SetInteger("weapon", 0);
        if (!FrontWheelNull) FrontWheel.localRotation = Quaternion.identity;
        DoEffects = 0f;//keep awake
        PlaneAnimator.SetBool("occupied", false);
    }
    public void EffectsLeavePlane()
    {
        Smoking = false;
        PlaneAnimator.SetBool("gunfiring", false);
        PlaneAnimator.SetBool("occupied", false);
        PlaneAnimator.SetInteger("missilesincoming", 0);
        PlaneAnimator.SetBool("localpilot", false);
    }
    private void Assert(bool condition, string message)
    {
        if (!condition)
        {
            Debug.LogWarning("Assertion failed : '" + GetType() + " : " + message + "'", this);
        }
    }
}