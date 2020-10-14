using EntityStates.Engi.Mine;
using UnityEngine.Networking;

namespace BadAssEngi.Skills.Secondary.SatchelMine.MineStates.MainStateMachine
{
    public class PreDetonateSatchel : BaseMineState
    {
        public override bool shouldStick => true;

        public override bool shouldRevertToWaitForStickOnSurfaceLost => false;

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (NetworkServer.active && PreDetonate.duration <= fixedAge)
            {
                outer.SetNextState(new DetonateSatchel());
            }
        }
    }
}