using BadAssEngi.Assets.Sound;
using EntityStates.Engi.Mine;
using RoR2;
using RoR2.Projectile;
using UnityEngine.Networking;

namespace BadAssEngi.Skills.Secondary.SatchelMine.MineStates.MainStateMachine
{
	public class DetonateSatchel : BaseMineState
	{
        public override bool shouldStick => true;

        public override bool shouldRevertToWaitForStickOnSurfaceLost => false;

		public override void OnEnter()
		{
			base.OnEnter();

            AkSoundEngine.PostEvent(SoundHelper.SatchelMineExplosion, outer.gameObject);
			if (NetworkServer.active)
			{
				Explode();
			}
		}

		private void Explode()
		{
			var projectileDamage = GetComponent<ProjectileDamage>();
			var damageScale = 0f;
            var blastRadiusScale = 0f;

			BaseMineArmingState baseMineArmingState;
			if ((baseMineArmingState = (armingStateMachine != null ? armingStateMachine.state : null) as BaseMineArmingState) != null)
			{
				damageScale = baseMineArmingState.damageScale;
                blastRadiusScale = baseMineArmingState.blastRadiusScale;
			}

			var blastRadius = Detonate.blastRadius * blastRadiusScale;

			new BlastAttack
			{
				procChainMask = projectileController.procChainMask,
				procCoefficient = projectileController.procCoefficient,
				attacker = projectileController.owner,
				inflictor = gameObject,
				teamIndex = projectileController.teamFilter.teamIndex,
				baseDamage = projectileDamage.damage * damageScale,
				baseForce = Configuration.SatchelMineForce.Value,
				falloffModel = BlastAttack.FalloffModel.None,
				crit = projectileDamage.crit,
				radius = blastRadius,
				position = transform.position,
				damageColorIndex = projectileDamage.damageColorIndex,
				attackerFiltering = AttackerFiltering.AlwaysHit
			}.Fire();

			if (Detonate.explosionEffectPrefab)
			{
				EffectManager.SpawnEffect(Detonate.explosionEffectPrefab, new EffectData
				{
					origin = transform.position,
					rotation = transform.rotation,
					scale = blastRadius
				}, true);
			}

			Destroy(gameObject);
		}
	}
}
