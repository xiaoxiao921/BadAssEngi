using BadAssEngi.Skills.Secondary.OrbitalStrike.MineState.ArmingStateMachine;
using EntityStates.Engi.Mine;
using UnityEngine.Networking;

namespace BadAssEngi.Skills.Secondary.OrbitalStrike.MineState.MainStateMachine
{
    public class WaitForStickOrbital : BaseMineState
    {
        public override bool shouldStick => true;

        public override bool shouldRevertToWaitForStickOnSurfaceLost => false;

        public override void OnEnter()
        {
            base.OnEnter();
            if (NetworkServer.active)
            {
                armingStateMachine.SetNextState(new MineArmingUnarmedOrbital());
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (NetworkServer.active && projectileStickOnImpact.stuck)
            {
                outer.SetNextState(new ArmOrbital());
            }
        }
    }
}