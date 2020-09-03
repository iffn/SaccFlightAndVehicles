
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class AAMController : UdonSharpBehaviour
{
    public EngineController EngineControl;
    public float LockAngle;
    public float RotSpeed = 15;
    private EngineController TargetEngineControl;
    private bool LockHack = true;
    private float Lifetime = 0;
    private float StartLockAngle = 0;
    private Transform Target;
    private bool ColliderActive = false;
    private bool Exploding = false;
    private CapsuleCollider AAMCollider;
    private bool Owner = false;
    private bool LockedOn = false;
    void Start()
    {
        Target = EngineControl.AAMTargets[EngineControl.AAMTarget].transform;
        AAMCollider = gameObject.GetComponent<CapsuleCollider>();
        StartLockAngle = LockAngle;
        if (EngineControl.AAMTargets[EngineControl.AAMTarget].transform.parent != null)
        {
            TargetEngineControl = EngineControl.AAMTargets[EngineControl.AAMTarget].transform.parent.GetComponent<EngineController>();
            if (TargetEngineControl != null)
            {
                if (EngineControl.InEditor)
                    Locked();
                else
                    SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "Locked");
                LockedOn = true;
            }
        }

        if (EngineControl.InEditor || EngineControl.IsOwner)
        {
            Owner = true;
            LockHack = false;//don't do netcode help hack if owner
        }
        else
        {
            LockAngle = 180;//help missiles fired during a lagged turnfight actually fly towards their targets for the people who didn't fire them (for the first 2 seconds)
        }
    }
    void LateUpdate()
    {
        if (!ColliderActive)
        {
            if (Lifetime > 0.5f)
            {
                AAMCollider.enabled = true;
                ColliderActive = true;
            }
        }
        if (LockHack)
        {
            if (Lifetime > 2)
            {
                LockHack = false;
                LockAngle = StartLockAngle;
            }
        }
        if (Vector3.Angle(gameObject.transform.forward, (Target.position - gameObject.transform.position)) < LockAngle)
        {
            // homing to target, thx Guribo
            var missileToTargetVector = Target.position - gameObject.transform.position;
            var missileForward = gameObject.transform.forward;
            var targetDirection = missileToTargetVector.normalized;
            var rotationAxis = Vector3.Cross(missileForward, targetDirection);
            var deltaAngle = Vector3.Angle(missileForward, targetDirection);
            gameObject.transform.Rotate(rotationAxis, Mathf.Min(RotSpeed * Time.deltaTime, deltaAngle), Space.World);
        }
        else if (LockedOn)
        {
            if (EngineControl.InEditor)
                LockedOff();
            else
                SendCustomNetworkEvent(VRC.Udon.Common.Interfaces.NetworkEventTarget.All, "LockedOff");
            LockedOn = false;
        }
        Lifetime += Time.deltaTime;
        if (Lifetime > 40)
        {
            Destroy(gameObject);
        }
    }
    public void Locked()
    {
        if (TargetEngineControl.Piloting || TargetEngineControl.Passenger)
            TargetEngineControl.MissilesIncoming++;
    }
    public void LockedOff()
    {
        if (TargetEngineControl.Piloting || TargetEngineControl.Passenger)
            TargetEngineControl.MissilesIncoming = (int)Mathf.Max((float)TargetEngineControl.MissilesIncoming - 1f, 0);
    }
    private void OnCollisionEnter(Collision other)
    {
        if (!Exploding)
        {
            if (TargetEngineControl != null)
            {
                //damage particles inherit the velocity of the missile, so this should help them hit the target plane
                //this is why iskinematic is set 2 frameslater in the explode animation.
                gameObject.GetComponent<Rigidbody>().velocity = TargetEngineControl.CurrentVel;
            }

            //would rather do it like this but udon wont let me
            /*             if (TargetEngineControl != null)
                        {
                            //damage particles take the velocity of the target plane so they can hit it any speed
                            var vel = DamageParticles.velocityOverLifetime;
                            vel.enabled = true;
                            vel.space = ParticleSystemSimulationSpace.World;

                            AnimationCurve velcurvex = new AnimationCurve();
                            AnimationCurve velcurvey = new AnimationCurve();
                            AnimationCurve velcurvez = new AnimationCurve();
                            velcurvex.AddKey(0.0f, TargetEngineControl.CurrentVel.x);
                            velcurvey.AddKey(0.0f, TargetEngineControl.CurrentVel.y);
                            velcurvez.AddKey(0.0f, TargetEngineControl.CurrentVel.z);
                            vel.x = new ParticleSystem.MinMaxCurve(1.0f, velcurvex);
                            vel.x = new ParticleSystem.MinMaxCurve(1.0f, velcurvey);
                            vel.x = new ParticleSystem.MinMaxCurve(1.0f, velcurvez);
                        } */
            AAMCollider.enabled = false;
            Animator AGMani = gameObject.GetComponent<Animator>();
            if (EngineControl.InEditor)
            {
                AGMani.SetTrigger("explodeowner");
            }
            else
            {
                if (Owner)
                {
                    AGMani.SetTrigger("explodeowner");
                }
                else AGMani.SetTrigger("explode");
            }
            Lifetime = 30;//10 seconds to finish exploding
        }
    }
}
