using System.Collections.Generic;
using BadAssEngi.Assets;
using BadAssEngi.Assets.Sound;
using EntityStates.Engi.Mine;
using EntityStates.Toolbot;
using RoR2;
using RoR2.Projectile;
using UnityEngine;
using UnityEngine.Networking;

namespace BadAssEngi.Skills.Secondary.ClusterMine.MineStates.MainStateMachine
{
    public class DetonateCluster : BaseMineState
    {
        private const int TotalBullets = 30;
        private const int MaxSprayBullets = 10;   // spray share when there are few targets
        private const int MinSprayBullets = 4;    // spray share never drops below this
        private const float BurstDuration = 0.15f;
        private const float TargetSearchRadius = 45f;

        private struct Shot
        {
            public int targetIndex;        // -1 = pure spray
            public Vector3 sprayDirection; // only used when targetIndex < 0
        }

        private ProjectileDamage _projectileDamage;

        // burst state (server only)
        private bool _burstPending;
        private Shot[] _plan;
        private List<HurtBox> _targets;
        private int _fired;
        private Vector3 _origin;
        private float _bulletDamage;
        private FireSpear _bulletTemplate;

        public override bool shouldStick => false;

        public override bool shouldRevertToWaitForStickOnSurfaceLost => false;

        public override void OnEnter()
        {
            CheckInitState();

            base.OnEnter();

            if (NetworkServer.active)
            {
                var currentRecursiveMine = gameObject.GetComponent<RecursiveMine>();

                _projectileDamage = GetComponent<ProjectileDamage>();

                if (currentRecursiveMine)
                {
                    if (currentRecursiveMine.RecursiveDepth < 2)
                    {
                        var ownerCharacterBody = projectileController.owner.GetComponent<CharacterBody>();

                        var aimDirection = ownerCharacterBody.inputBank.aimDirection;
                        var z = Random.Range(0f, 360f);
                        var z2 = Random.Range(0f, 360f);
                        var angVecLeft = Quaternion.Euler(-20, -90, z) * new Vector3(aimDirection.x, aimDirection.y, aimDirection.z).normalized;
                        var angVecRight = Quaternion.Euler(20, 90, z2) * new Vector3(aimDirection.x, aimDirection.y, aimDirection.z).normalized;

                        var minePrefab = currentRecursiveMine.RecursiveDepth == 0
                            ? BaeAssets.EngiClusterMineDepthOnePrefab
                            : BaeAssets.EngiClusterMineDepthTwoPrefab;

                        FireProjectileInfo fireProjectileInfo = default;
                        fireProjectileInfo.projectilePrefab = minePrefab;
                        fireProjectileInfo.position = transform.position;
                        fireProjectileInfo.owner = projectileController.owner;
                        fireProjectileInfo.damage = ownerCharacterBody.damage * Configuration.ClusterMineDamageCoefficient.Value;
                        fireProjectileInfo.force = _projectileDamage.force;
                        fireProjectileInfo.crit = RoR2.Util.CheckRoll(ownerCharacterBody.crit, ownerCharacterBody.master);
                        fireProjectileInfo.speedOverride = 18f;
                        fireProjectileInfo.damageTypeOverride = new DamageTypeCombo?(DamageTypeCombo.GenericSecondary);

                        fireProjectileInfo.rotation = RoR2.Util.QuaternionSafeLookRotation(angVecLeft);
                        ProjectileManager.instance.FireProjectile(fireProjectileInfo);

                        fireProjectileInfo.rotation = RoR2.Util.QuaternionSafeLookRotation(Vector3.up);
                        ProjectileManager.instance.FireProjectile(fireProjectileInfo);

                        fireProjectileInfo.rotation = RoR2.Util.QuaternionSafeLookRotation(angVecRight);
                        ProjectileManager.instance.FireProjectile(fireProjectileInfo);
                    }
                }

                Explode();
            }

            AkSoundEngine.PostEvent(SoundHelper.ClusterMineExplosion, outer.gameObject);

            // Server keeps the mine alive until the burst is done (see FixedUpdate).
            // Everyone else behaves exactly as before.
            if (!_burstPending)
                Destroy(gameObject);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (!NetworkServer.active || !_burstPending)
                return;

            // Owner gone mid-burst: stop instead of firing unowned bullets.
            if (!projectileController || !projectileController.owner)
            {
                _burstPending = false;
                Destroy(gameObject);
                return;
            }

            int due = Mathf.Min(TotalBullets, Mathf.CeilToInt(fixedAge / BurstDuration * TotalBullets));
            while (_fired < due)
                FireShot(_plan[_fired++]);

            if (_fired >= TotalBullets)
            {
                _burstPending = false;
                Destroy(gameObject);
            }
        }

        private void CheckInitState()
        {
            if (string.IsNullOrEmpty(enterSoundString))
            {
                var goodState = new Detonate();
                enterSoundString = goodState.enterSoundString;
            }
        }

        private void Explode()
        {
            var baseMineArmingState = (BaseMineArmingState)armingStateMachine.state;

            //if (armingStateMachine. == 0) // why it happens ?
            //  baseMineArmingState.damageScale = 1f;

            new BlastAttack
            {
                procChainMask = projectileController.procChainMask,
                procCoefficient = projectileController.procCoefficient,
                attacker = projectileController.owner,
                inflictor = outer.gameObject,
                teamIndex = projectileController.teamFilter.teamIndex,
                attackerFiltering = AttackerFiltering.AlwaysHit,
                baseDamage = _projectileDamage.damage * 10f * baseMineArmingState.damageScale,
                damageType = DamageTypeCombo.GenericSecondary,
                baseForce = 0.1f,
                falloffModel = BlastAttack.FalloffModel.None,
                crit = _projectileDamage.crit,
                radius = Detonate.blastRadius,
                position = outer.transform.position,
                damageColorIndex = _projectileDamage.damageColorIndex
            }.Fire();

            // snapshot everything the burst needs; the arming state may change during the burst
            _bulletTemplate = new FireSpear();
            _origin = transform.position;
            _bulletDamage = _projectileDamage.damage * baseMineArmingState.damageScale;
            _targets = FindTargets(_origin);
            _plan = BuildPlan(_targets.Count);
            _fired = 0;
            _burstPending = true;
        }

        private List<HurtBox> FindTargets(Vector3 origin)
        {
            var targets = new List<HurtBox>();
            var enemyTeams = TeamMask.GetEnemyTeams(projectileController.teamFilter.teamIndex);

            new SphereSearch
            {
                origin = origin,
                radius = TargetSearchRadius,
                mask = LayerIndex.entityPrecise.mask,
                queryTriggerInteraction = QueryTriggerInteraction.UseGlobal
            }
            .RefreshCandidates()
            .FilterCandidatesByHurtBoxTeam(enemyTeams)
            .FilterCandidatesByDistinctHurtBoxEntities()
            .GetHurtBoxes(targets);

            // drop dead targets and ones behind world geometry
            targets.RemoveAll(hb =>
                !hb.healthComponent || !hb.healthComponent.alive ||
                Physics.Linecast(origin, hb.collider.bounds.center,
                    LayerIndex.world.mask, QueryTriggerInteraction.Ignore));

            // sorted here: SphereSearch.OrderByDistance never compares the last candidate
            targets.Sort((a, b) =>
                (a.collider.bounds.center - origin).sqrMagnitude
                    .CompareTo((b.collider.bounds.center - origin).sqrMagnitude));

            return targets;
        }

        private static Shot[] BuildPlan(int targetCount)
        {
            // At least one bullet per target (nearest first), clamped so some spray always remains.
            int homing = targetCount > 0
                ? Mathf.Clamp(targetCount, TotalBullets - MaxSprayBullets, TotalBullets - MinSprayBullets)
                : 0;
            int spray = TotalBullets - homing;

            var plan = new Shot[TotalBullets];
            var sprayRotation = Random.rotation; // different spray pattern each detonation
            int sprayIndex = 0;
            int homingIndex = 0;

            for (int i = 0; i < TotalBullets; i++)
            {
                // spreads the spray slots evenly through the burst so both kinds are interleaved
                bool isSpray = (i + 1) * spray / TotalBullets > i * spray / TotalBullets;

                if (isSpray)
                {
                    float phi = sprayIndex * (Mathf.PI * (3f - Mathf.Sqrt(5f))); // Fibonacci angle
                    float y = spray > 1 ? 1f - sprayIndex / (float)(spray - 1) * 2f : 0f;
                    float r = Mathf.Sqrt(1f - y * y);
                    plan[i] = new Shot
                    {
                        targetIndex = -1,
                        sprayDirection = sprayRotation * new Vector3(Mathf.Cos(phi) * r, y, Mathf.Sin(phi) * r)
                    };
                    sprayIndex++;
                }
                else
                {
                    // round-robin: every target gets one before anyone gets a second;
                    // leftovers go to the nearest ones because the list is sorted by distance
                    plan[i] = new Shot { targetIndex = homingIndex++ % targetCount };
                }
            }

            return plan;
        }

        private void FireShot(Shot shot)
        {
            Vector3 direction;

            if (shot.targetIndex < 0)
            {
                direction = shot.sprayDirection;
            }
            else
            {
                direction = Random.onUnitSphere; // fallback if every target died mid-burst
                var target = FindAliveTarget(shot.targetIndex);
                if (target)
                {
                    // aim point is computed now, so the burst follows moving targets
                    var toTarget = GetAimPoint(target) - _origin;
                    if (toTarget.sqrMagnitude > 0.0001f)
                        direction = toTarget.normalized;
                }
            }

            SpawnClusterProjectile(direction);
        }

        // If the planned target died, shift to the next living one so the shot isn't wasted.
        private HurtBox FindAliveTarget(int startIndex)
        {
            for (int k = 0; k < _targets.Count; k++)
            {
                var hb = _targets[(startIndex + k) % _targets.Count];
                if (hb && hb.healthComponent && hb.healthComponent.alive)
                    return hb;
            }
            return null;
        }

        private static Vector3 GetAimPoint(HurtBox hurtBox)
        {
            var bounds = hurtBox.collider.bounds;
            return bounds.center + Vector3.Scale(Random.insideUnitSphere, bounds.extents * 0.5f);
        }

        private void SpawnClusterProjectile(Vector3 direction)
        {
            const float maxDistance = 99999f;
            new BulletAttack
            {
                aimVector = direction,
                origin = _origin,
                owner = projectileController.owner,
                weapon = outer.gameObject,
                bulletCount = 1u,
                damage = _bulletDamage,
                damageColorIndex = DamageColorIndex.Default,
                damageType = DamageTypeCombo.GenericSecondary,
                falloffModel = BulletAttack.FalloffModel.None,
                force = 0.1f,
                HitEffectNormal = false,
                procChainMask = default,
                procCoefficient = 0.05f,
                maxDistance = maxDistance,
                radius = _bulletTemplate.bulletRadius,
                isCrit = _projectileDamage.crit,
                muzzleName = _bulletTemplate.muzzleName,
                minSpread = _bulletTemplate.minSpread,
                maxSpread = _bulletTemplate.maxSpread,
                hitEffectPrefab = null,
                smartCollision = true,
                sniper = false,
                spreadPitchScale = 0.5f,
                spreadYawScale = 1f,
                tracerEffectPrefab = null
            }.Fire();

            var currentVisual = Configuration.ClusterMineVisualBouncing.Value
                ? BaeAssets.PrefabEngiClusterMineVisualBounce
                : BaeAssets.PrefabEngiClusterMineVisual;
            var clusterVisual = Object.Instantiate(currentVisual, _origin, Quaternion.LookRotation(direction));

            if (Configuration.ClusterMineVisualBouncing.Value)
            {
                var clusterComp = clusterVisual.GetComponent<ClusterController>();

                clusterComp.Owner = projectileController.owner;
                clusterComp.Damage = _bulletDamage;
            }

            NetworkServer.Spawn(clusterVisual);
        }
    }
}