using BadAssEngi.Assets;
using EntityStates;
using EntityStates.Engi.EngiWeapon;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace BadAssEngi.Skills.Secondary.SatchelMine.EngiStates
{
    public class FireSatchelMines : BaseState
    {
        private static GameObject _effectPrefab;

        private static string _throwMineSoundString;

        private static float DamageCoefficient => Configuration.SatchelMineDamageCoefficient.Value;

        private float _force;

        private const float BaseDuration = 0.75f;
        private float _duration;

        public override void OnEnter()
        {
            CheckInitState();

            base.OnEnter();
            RoR2.Util.PlaySound(_throwMineSoundString, gameObject);
            _duration = BaseDuration / attackSpeedStat;
            var aimRay = GetAimRay();
            StartAimMode(aimRay);
            if (GetModelAnimator())
            {
                var num = _duration * 0.3f;
                PlayCrossfade("Gesture, Additive", "FireMineRight", "FireMine.playbackRate", _duration + num, 0.05f);
            }
            const string muzzleName = "MuzzleCenter";
            if (_effectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(_effectPrefab, gameObject, muzzleName, false);
            }
            if (isAuthority)
            {
                FireProjectileInfo fireProjectileInfo = default;
                fireProjectileInfo.projectilePrefab = BaeAssets.EngiSatchelMinePrefab;
                fireProjectileInfo.position = aimRay.origin;
                fireProjectileInfo.rotation = RoR2.Util.QuaternionSafeLookRotation(aimRay.direction);
                fireProjectileInfo.owner = gameObject;
                fireProjectileInfo.damage = damageStat * DamageCoefficient;
                fireProjectileInfo.force = _force;
                fireProjectileInfo.crit = RoR2.Util.CheckRoll(critStat, characterBody.master);
                fireProjectileInfo.damageTypeOverride = new DamageTypeCombo?(DamageTypeCombo.GenericSecondary);
                ProjectileManager.instance.FireProjectile(fireProjectileInfo);
            }
        }

        private void CheckInitState()
        {
            if (_effectPrefab)
            {
                return;
            }

            var goodState = new FireMines();

            _effectPrefab = FireMines.effectPrefab;

            _throwMineSoundString = FireMines.throwMineSoundString;

            _force = goodState.force;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (fixedAge >= _duration && isAuthority)
            {
                outer.SetNextStateToMain();
            }
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
