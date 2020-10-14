using EntityStates.Engi.Mine;
using UnityEngine.Networking;

namespace BadAssEngi.Skills.Secondary.SatchelMine.MineStates.ArmingStateMachine
{
    public class MineArmingWeakSatchel : BaseMineArmingState
    {
        public override void OnEnter()
        {
            CheckInitState();

            base.OnEnter();
        }

        private void CheckInitState()
        {
            var goodState = (MineArmingWeak) Instantiate(typeof(MineArmingWeak));

            if (string.IsNullOrEmpty(pathToChildToEnable))
            {
                pathToChildToEnable = goodState.pathToChildToEnable;
                onEnterSfxPlaybackRate = goodState.onEnterSfxPlaybackRate;
                onEnterSfx = goodState.onEnterSfx;
                triggerRadius = goodState.triggerRadius;
                blastRadiusScale = goodState.blastRadiusScale;
                forceScale = goodState.forceScale;
                damageScale = goodState.damageScale;
            }
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (NetworkServer.active && MineArmingWeak.duration <= fixedAge)
            {
                outer.SetNextState(new MineArmingFullSatchel());
            }
        }
    }
}