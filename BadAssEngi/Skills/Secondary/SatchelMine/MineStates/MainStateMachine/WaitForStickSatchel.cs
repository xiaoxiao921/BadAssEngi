using BadAssEngi.Skills.Secondary.SatchelMine.MineStates.ArmingStateMachine;
using EntityStates.Engi.Mine;
using UnityEngine.Networking;

namespace BadAssEngi.Skills.Secondary.SatchelMine.MineStates.MainStateMachine
{
    public class WaitForStickSatchel : BaseMineState
    {
        public override bool shouldStick => true;

        public override bool shouldRevertToWaitForStickOnSurfaceLost => false;

        public override void OnEnter()
        {
            base.OnEnter();
            if (NetworkServer.active)
            {
                armingStateMachine.SetNextState(new MineArmingUnarmedSatchel());
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (NetworkServer.active && projectileStickOnImpact.stuck)
            {
                outer.SetNextState(new ArmSatchel());
            }
        }
    }
}