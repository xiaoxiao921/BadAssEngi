using BadAssEngi.Assets;
using BadAssEngi.Skills.Primary.SeekerMissile;
using EntityStates;
using EntityStates.Engi.EngiWeapon;
using RoR2;
using RoR2.Projectile;
using UnityEngine;

namespace BadAssEngi.Skills.Primary.States
{
    internal class FireSeekerMissile : BaseState
    {
        private static GameObject _effectPrefab;

        public int GrenadeCountMax;
        private static float DamageCoefficient => Configuration.SeekerMissileDamageCoefficient.Value;

        private Ray _projectileRay;

        private Transform _modelTransform;

        private float _fireTimer;

        private int _grenadeCount;

        public override void OnEnter()
        {
            CheckInitState();

            base.OnEnter();
            _modelTransform = GetModelTransform();
            StartAimMode();
        }

        private static void CheckInitState()
        {
            if (_effectPrefab)
            {
                return;
            }

            _ = new FireGrenades();

            _effectPrefab = FireGrenades.effectPrefab;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            _fireTimer -= Time.fixedDeltaTime;
            var num = FireGrenades.fireDuration / attackSpeedStat / GrenadeCountMax;
            if (_fireTimer <= 0f && _grenadeCount < GrenadeCountMax)
            {
                _fireTimer += num;
                if (_grenadeCount % 2 == 0)
                {
                    FireMissile("MuzzleLeft");
                    PlayCrossfade("Gesture Left Cannon, Additive", "FireGrenadeLeft", 0.1f);
                }
                else
                {
                    FireMissile("MuzzleRight");
                    PlayCrossfade("Gesture Right Cannon, Additive", "FireGrenadeRight", 0.1f);
                }
                _grenadeCount++;
            }

            if (isAuthority && _grenadeCount >= GrenadeCountMax)
            {
                outer.SetNextStateToMain();
            }
        }

        private void FireMissile(string targetMuzzle)
        {
            _projectileRay = GetAimRay();

            if (_modelTransform)
            {
                var component = _modelTransform.GetComponent<ChildLocator>();
                if (component)
                {
                    var childMuzzleTransform = component.FindChild(targetMuzzle);
                    if (childMuzzleTransform)
                    {
                        _projectileRay.origin = childMuzzleTransform.position;
                    }
                }
            }

            AddRecoil(-1f * FireGrenades.recoilAmplitude, -2f * FireGrenades.recoilAmplitude,
                -1f * FireGrenades.recoilAmplitude, 1f * FireGrenades.recoilAmplitude);
            if (FireGrenades.effectPrefab)
            {
                EffectManager.SimpleMuzzleFlash(FireGrenades.effectPrefab, gameObject, targetMuzzle, false);
            }

            if (isAuthority)
            {
                var missileTracker = characterBody.GetComponent<MissileTracker>();
                if (!missileTracker)
                {
                    missileTracker = characterBody.gameObject.AddComponent<MissileTracker>();
                }
                var currentTargetHurtBox = missileTracker.trackingTarget;
                var target = currentTargetHurtBox ? currentTargetHurtBox.gameObject : null;
                FireMissileProjectile(target, this, BaeAssets.PrefabEngiSwarmRocket, targetMuzzle);
            }

            characterBody.AddSpreadBloom(FireGrenades.spreadBloomValue);
        }

        private static void FireMissileProjectile(GameObject target, BaseState entityState, GameObject projectilePrefab, string targetMuzzle)
        {
            Vector3 missileOrig = default;
            var component = entityState.outer.commonComponents.modelLocator.modelTransform.GetComponent<ChildLocator>();
            if (component)
            {
                var childTransform = component.FindChild(targetMuzzle);
                if (childTransform)
                {
                    missileOrig = childTransform.position;
                }
            }

            FireProjectileInfo fireProjectileInfo = default;
            fireProjectileInfo.position = missileOrig == default ? entityState.outer.commonComponents.inputBank.aimOrigin : missileOrig;
            fireProjectileInfo.rotation = Quaternion.LookRotation(entityState.outer.commonComponents.inputBank.aimDirection);
            fireProjectileInfo.crit = RoR2.Util.CheckRoll(entityState.critStat, entityState.outer.commonComponents.characterBody.master);
            fireProjectileInfo.damage = entityState.damageStat * FireSeekerMissile.DamageCoefficient;
            fireProjectileInfo.damageColorIndex = DamageColorIndex.Default;
            fireProjectileInfo.damageTypeOverride = new DamageTypeCombo?(DamageTypeCombo.GenericPrimary);
            fireProjectileInfo.owner = entityState.outer.gameObject;
            fireProjectileInfo.projectilePrefab = projectilePrefab;
            if (target)
                fireProjectileInfo.target = target.gameObject;
            ProjectileManager.instance.FireProjectile(fireProjectileInfo);
        }

        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}
