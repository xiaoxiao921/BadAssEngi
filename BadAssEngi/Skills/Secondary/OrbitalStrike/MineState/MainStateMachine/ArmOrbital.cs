using EntityStates.Engi.Mine;
using UnityEngine.Networking;

namespace BadAssEngi.Skills.Secondary.OrbitalStrike.MineState.MainStateMachine
{
    public class ArmOrbital : BaseMineState
    {
        public static float duration;

        public override bool shouldStick => true;

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (NetworkServer.active && Arm.duration <= fixedAge)
            {
                outer.SetNextState(new WaitForTargetOrbital());
            }
        }
    }
}