using EntityStates.Engi.Mine;
using UnityEngine.Networking;

namespace BadAssEngi.Skills.Secondary.OrbitalStrike.MineState.ArmingStateMachine
{
    public class MineArmingWeakOrbital : BaseMineArmingState
    {
        public override void OnEnter()
        {
            CheckInitState();

            base.OnEnter();
        }

        private void CheckInitState()
        {
            var goodState = new MineArmingWeak();

            pathToChildToEnable = goodState.pathToChildToEnable;
            onEnterSfxPlaybackRate = goodState.onEnterSfxPlaybackRate;
            onEnterSfx = goodState.onEnterSfx;
            triggerRadius = goodState.triggerRadius;
            blastRadiusScale = goodState.blastRadiusScale;
            forceScale = goodState.forceScale;
            damageScale = goodState.damageScale;
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (NetworkServer.active && MineArmingWeak.duration <= fixedAge)
            {
                outer.SetNextState(new MineArmingFullOrbital());
            }
        }
    }
}