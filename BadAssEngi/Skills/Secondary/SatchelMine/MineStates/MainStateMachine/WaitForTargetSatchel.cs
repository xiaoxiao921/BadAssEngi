using BadAssEngi.Skills.Secondary.SatchelMine.MineStates.ArmingStateMachine;
using EntityStates.Engi.Mine;
using RoR2.Projectile;
using UnityEngine.Networking;

namespace BadAssEngi.Skills.Secondary.SatchelMine.MineStates.MainStateMachine
{
	public class WaitForTargetSatchel : BaseMineState
	{
        private ProjectileSphereTargetFinder _targetFinder;

        private ProjectileTargetComponent _projectileTargetComponent;

        public override bool shouldStick => true;

        public override void OnEnter()
        {
            base.OnEnter();

            _projectileTargetComponent = GetComponent<ProjectileTargetComponent>();
            _targetFinder = GetComponent<ProjectileSphereTargetFinder>();

            if (NetworkServer.active)
            {
                _targetFinder.enabled = true;
                armingStateMachine.SetNextState(new MineArmingWeakSatchel());
            }
        }

        public override void OnExit()
        {
            if (_targetFinder)
            {
                _targetFinder.enabled = false;
            }

            base.OnExit();
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();

            if (NetworkServer.active && _targetFinder)
            {
                if (_projectileTargetComponent.target)
                {
                    outer.SetNextState(new PreDetonateSatchel());
                }

                BaseMineArmingState baseMineArmingState;
                if ((baseMineArmingState = (armingStateMachine != null ? armingStateMachine.state : null) as BaseMineArmingState) != null)
                {
                    _targetFinder.enabled = baseMineArmingState.triggerRadius != 0f;
                    _targetFinder.lookRange = baseMineArmingState.triggerRadius;
                }
            }
        }
	}
}
