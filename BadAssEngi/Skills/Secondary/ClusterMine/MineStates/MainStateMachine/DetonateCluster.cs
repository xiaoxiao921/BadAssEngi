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
        private ProjectileDamage _projectileDamage;
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

                        var aimDirection =  ownerCharacterBody.inputBank.aimDirection;
                        var z = Random.Range(0f, 360f);
                        var z2 = Random.Range(0f, 360f);
                        var angVecLeft = Quaternion.Euler(-20, -90, z) * new Vector3(aimDirection.x, aimDirection.y, aimDirection.z).normalized;
                        var angVecRight = Quaternion.Euler(20, 90, z2) * new Vector3(aimDirection.x, aimDirection.y, aimDirection.z).normalized;

                        var minePrefab = currentRecursiveMine.RecursiveDepth == 0
                            ? BaeAssets.EngiClusterMineDepthOnePrefab
                            : BaeAssets.EngiClusterMineDepthTwoPrefab;

                        ProjectileManager.instance.FireProjectile(minePrefab, transform.position,
                            RoR2.Util.QuaternionSafeLookRotation(angVecLeft), projectileController.owner,
                            ownerCharacterBody.damage * Configuration.ClusterMineDamageCoefficient.Value, _projectileDamage.force,
                            RoR2.Util.CheckRoll(ownerCharacterBody.crit, ownerCharacterBody.master), DamageColorIndex.Default, null, 18f);

                        ProjectileManager.instance.FireProjectile(minePrefab, transform.position,
                            RoR2.Util.QuaternionSafeLookRotation(Vector3.up), projectileController.owner,
                            ownerCharacterBody.damage * Configuration.ClusterMineDamageCoefficient.Value, _projectileDamage.force,
                            RoR2.Util.CheckRoll(ownerCharacterBody.crit, ownerCharacterBody.master), DamageColorIndex.Default, null, 18f);

                        ProjectileManager.instance.FireProjectile(minePrefab, transform.position,
                            RoR2.Util.QuaternionSafeLookRotation(angVecRight), projectileController.owner,
                            ownerCharacterBody.damage * Configuration.ClusterMineDamageCoefficient.Value, _projectileDamage.force,
                            RoR2.Util.CheckRoll(ownerCharacterBody.crit, ownerCharacterBody.master), DamageColorIndex.Default, null, 18f);
                    }
                }

				Explode();
			}

            AkSoundEngine.PostEvent(SoundHelper.ClusterMineExplosion, outer.gameObject);
            Destroy(gameObject);
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
                baseForce = 0.1f,
                falloffModel = BlastAttack.FalloffModel.None,
                crit = _projectileDamage.crit,
                radius = Detonate.blastRadius,
                position = outer.transform.position,
                damageColorIndex = _projectileDamage.damageColorIndex
            }.Fire();

            var genericBullet = new FireSpear();

            // -90 to 90 for upper sphere
            /*for (float x = -90; x <= 180; x += 45)
            {
                for (float y = 0f; y <= 360f; y += 72)
                {
			        var z = Random.Range(0f, 50f);
                    SpawnClusterProjectile(baseMineArmingState, genericBullet, Quaternion.Euler(x, y, z) * Vector3.up);
                }
            }*/

            const int RandomPointCount = 30;
            for (int i = 0; i < RandomPointCount; i++)
            {
                //Vector3 randomDirection = Random.insideUnitSphere.normalized;

                float phi = i * (Mathf.PI * (3f - Mathf.Sqrt(5f))); // Fibonacci angle

                float y = 1 - (i / (float)(RandomPointCount - 1)) * 2; // Map to range [-1, 1]
                float radiusAtY = Mathf.Sqrt(1 - y * y); // Radius at current height

                float x = Mathf.Cos(phi) * radiusAtY;
                float z = Mathf.Sin(phi) * radiusAtY;

                Vector3 randomDirection = new Vector3(x, y, z);

                SpawnClusterProjectile(baseMineArmingState, genericBullet, randomDirection);
            }
        }

        private void SpawnClusterProjectile(BaseMineArmingState baseMineArmingState, FireSpear genericBullet, Vector3 direction)
        {
            var ray = new Ray(outer.gameObject.transform.position, direction);

            const float maxDistance = 99999f;
            new BulletAttack
            {
                aimVector = ray.direction,
                origin = ray.origin,
                owner = projectileController.owner,
                weapon = outer.gameObject,
                bulletCount = 1u,
                damage = _projectileDamage.damage * baseMineArmingState.damageScale,
                damageColorIndex = DamageColorIndex.Default,
                damageType = DamageType.Generic,
                falloffModel = BulletAttack.FalloffModel.None,
                force = 0.1f,
                HitEffectNormal = false,
                procChainMask = default,
                procCoefficient = 0.05f,
                maxDistance = maxDistance,
                radius = genericBullet.bulletRadius,
                isCrit = _projectileDamage.crit,
                muzzleName = genericBullet.muzzleName,
                minSpread = genericBullet.minSpread,
                maxSpread = genericBullet.maxSpread,
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
            var clusterVisual = Object.Instantiate(currentVisual, outer.gameObject.transform.position, Quaternion.LookRotation(direction));

            if (Configuration.ClusterMineVisualBouncing.Value)
            {
                var clusterComp =
                    clusterVisual.GetComponent<ClusterController>();

                clusterComp.Owner = projectileController.owner;
                clusterComp.Damage = _projectileDamage.damage * baseMineArmingState.damageScale;
            }

            NetworkServer.Spawn(clusterVisual);
        }
    }
}
