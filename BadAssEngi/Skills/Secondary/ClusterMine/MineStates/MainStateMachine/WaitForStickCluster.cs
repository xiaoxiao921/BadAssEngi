using BadAssEngi.Skills.Secondary.ClusterMine.MineStates.ArmingStateMachine;
using EntityStates.Engi.Mine;
using UnityEngine.Networking;

namespace BadAssEngi.Skills.Secondary.ClusterMine.MineStates.MainStateMachine
{
    public class WaitForStickCluster : BaseMineState
    {
        public override bool shouldStick => true;

        public override bool shouldRevertToWaitForStickOnSurfaceLost => false;

        public override void OnEnter()
        {
            base.OnEnter();
            if (NetworkServer.active)
            {
                armingStateMachine.SetNextState(new MineArmingUnarmedCluster());
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (NetworkServer.active && projectileStickOnImpact.stuck)
            {
                outer.SetNextState(new ArmCluster());
            }
        }
    }
}