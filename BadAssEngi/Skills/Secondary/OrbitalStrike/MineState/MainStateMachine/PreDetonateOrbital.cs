using EntityStates.Engi.Mine;
using UnityEngine.Networking;

namespace BadAssEngi.Skills.Secondary.OrbitalStrike.MineState.MainStateMachine
{
    public class PreDetonateOrbital : BaseMineState
    {
        private static float _duration;

        public override bool shouldStick => false;

        public override bool shouldRevertToWaitForStickOnSurfaceLost => false;

        public override void OnEnter()
        {
            base.OnEnter();

            var _ = new PreDetonate();
            _duration = PreDetonate.duration;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (NetworkServer.active && _duration <= fixedAge)
            {
                outer.SetNextState(new DetonateOrbital());
            }
        }
    }
}