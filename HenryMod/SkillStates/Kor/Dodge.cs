using EntityStates;
using KorMod.Modules;
using RoR2;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace KorMod.SkillStates.Kor
{
    public class Dodge : BaseSkillState
    {
        public static float duration = 0.5f;
        public static float initialSpeedCoefficient = 5f;
        public static float finalSpeedCoefficient = 2.5f;

        public static string dodgeSoundString = "HenryRoll";
        public static float dodgeFOV = EntityStates.Commando.DodgeState.dodgeFOV;

        private float rollSpeed;
        private Vector3 forwardDirection;
        private Animator animator;
        private Vector3 previousPosition;

        private float chaseDodgeTime = 1;
        private bool isChaseDodge = false;

        Vector3 aimDir;
        public override void OnEnter()
        {
            base.OnEnter();
            this.animator = base.GetModelAnimator();
            this.isChaseDodge = CheckIfChaseDodge();
            Chat.AddMessage($"Chase dodge: {isChaseDodge}");

            //if (base.isAuthority && base.inputBank && base.characterDirection)
            //{
            //    this.forwardDirection = ((base.inputBank.moveVector == Vector3.zero) ? base.characterDirection.forward : base.inputBank.moveVector).normalized;
            //}
            if(base.isAuthority && base.inputBank)
            {
                this.aimDir = base.inputBank.aimDirection;
            }

            //Vector3 rhs = base.characterDirection ? base.characterDirection.forward : this.forwardDirection;
            //Vector3 rhs2 = Vector3.Cross(Vector3.up, rhs);

            //float num = Vector3.Dot(this.aimDir, rhs);
            //float num2 = Vector3.Dot(this.aimDir, rhs2);

            this.RecalculateRollSpeed();

            if (base.characterMotor && base.characterDirection)
            {
                base.characterMotor.velocity.y = 0f;
                base.characterMotor.velocity.x = 0f;
                base.characterMotor.velocity = this.aimDir * this.rollSpeed;
            }

            Vector3 b = base.characterMotor ? base.characterMotor.velocity : Vector3.zero;
            this.previousPosition = base.transform.position - b;

            //base.PlayAnimation("FullBody, Override", "Roll", "Roll.playbackRate", Roll.duration);
            Util.PlaySound(Dodge.dodgeSoundString, base.gameObject);

            if (NetworkServer.active)
            {
                //base.characterBody.AddTimedBuff(Modules.Buffs.armorBuff, 3f * Dodge.duration);
                base.characterBody.AddBuff(RoR2Content.Buffs.HiddenInvincibility);
                base.gameObject.layer = LayerIndex.fakeActor.intVal;
                base.characterMotor.Motor.RebuildCollidableLayers();
            }
        }

        private void RecalculateRollSpeed()
        {
            this.rollSpeed = this.moveSpeedStat * Mathf.Lerp(Dodge.initialSpeedCoefficient, Dodge.finalSpeedCoefficient, base.fixedAge / Dodge.duration) *
                (isChaseDodge?2:1);
        }
        public bool CheckIfChaseDodge()
        {
            return this.characterBody.GetComponent<OnDamageDealt>().GetTime() < chaseDodgeTime;
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            this.RecalculateRollSpeed();
            Log.Debug($"Dir {aimDir}; charDir {base.characterDirection.forward}");

            if (base.characterDirection) base.characterDirection.forward = this.aimDir;
            if (base.cameraTargetParams) base.cameraTargetParams.fovOverride = Mathf.Lerp(Dodge.dodgeFOV, 60f, base.fixedAge / Dodge.duration);
            
            Vector3 normalized = (base.transform.position - this.previousPosition).normalized;
            Log.Debug($"norm {normalized}");
            if (base.characterMotor && base.characterDirection && normalized != Vector3.zero)
            {
                Vector3 vector = normalized * this.rollSpeed;
                Log.Debug($"vect {vector}");
                float d = Mathf.Max(Vector3.Dot(vector, this.aimDir), 0f);
                Log.Debug($"d {d}");
                vector = this.aimDir * d;
                //vector.y = 0f;
                
                base.characterMotor.velocity = vector;
            }
            this.previousPosition = base.transform.position;
            if(base.inputBank.moveVector.x < 0)
            {
                this.outer.SetNextStateToMain();
                return;
            }
            if (base.isAuthority && base.fixedAge >= Dodge.duration)
            {
                this.outer.SetNextStateToMain();
                return;
            }
        }

        public override void OnExit()
        {
            if (base.cameraTargetParams) base.cameraTargetParams.fovOverride = -1f;
            base.OnExit();

            base.characterBody.RemoveBuff(RoR2Content.Buffs.HiddenInvincibility);
            base.gameObject.layer = LayerIndex.defaultLayer.intVal;
            base.characterMotor.Motor.RebuildCollidableLayers();
            base.characterMotor.disableAirControlUntilCollision = false;
        }

        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(this.forwardDirection);
            writer.Write(this.isChaseDodge);
        }

        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            this.forwardDirection = reader.ReadVector3();
            this.isChaseDodge = reader.ReadBoolean();
        }
    }
}
