using EntityStates.Engi.Mine;
using UnityEngine.Networking;

namespace BadAssEngi.Skills.Secondary.SatchelMine.MineStates.MainStateMachine
{
    public class ArmSatchel : BaseMineState
    {
        public override bool shouldStick => true;

        public override void OnEnter()
        {
            CheckInitState();

            base.OnEnter();
        }

        private void CheckInitState()
        {
            var goodState = (Arm) Instantiate(typeof(Arm));

            if (string.IsNullOrEmpty(enterSoundString))
            {
                enterSoundString = goodState.enterSoundString;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (NetworkServer.active && Arm.duration <= fixedAge)
            {
                outer.SetNextState(new WaitForTargetSatchel());
            }
        }
    }
}