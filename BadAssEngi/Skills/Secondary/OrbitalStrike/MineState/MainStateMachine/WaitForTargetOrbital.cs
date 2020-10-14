using BadAssEngi.Skills.Secondary.OrbitalStrike.MineState.ArmingStateMachine;
using EntityStates.Engi.Mine;
using RoR2.Projectile;
using UnityEngine.Networking;

namespace BadAssEngi.Skills.Secondary.OrbitalStrike.MineState.MainStateMachine
{
	public class WaitForTargetOrbital : BaseMineState
	{
        private ProjectileSphereTargetFinder targetFinder;

        private ProjectileTargetComponent projectileTargetComponent;


		public override bool shouldStick => true;

		public override void OnEnter()
		{
			base.OnEnter();

			projectileTargetComponent = GetComponent<ProjectileTargetComponent>();
			targetFinder = GetComponent<ProjectileSphereTargetFinder>();

			if (NetworkServer.active)
			{
				targetFinder.enabled = true;
				armingStateMachine.SetNextState(new MineArmingWeakOrbital());
			}
		}

		public override void OnExit()
		{
			if (targetFinder)
			{
				targetFinder.enabled = false;
			}

			base.OnExit();
		}

		public override void FixedUpdate()
		{
			base.FixedUpdate();

			if (NetworkServer.active && targetFinder)
			{
				if (projectileTargetComponent.target)
				{
					outer.SetNextState(new PreDetonateOrbital());
				}

				BaseMineArmingState baseMineArmingState;
				if ((baseMineArmingState = (armingStateMachine != null ? armingStateMachine.state : null) as BaseMineArmingState) != null)
				{
					targetFinder.enabled = baseMineArmingState.triggerRadius != 0f;
					targetFinder.lookRange = baseMineArmingState.triggerRadius;
				}
			}
		}
	}
}
