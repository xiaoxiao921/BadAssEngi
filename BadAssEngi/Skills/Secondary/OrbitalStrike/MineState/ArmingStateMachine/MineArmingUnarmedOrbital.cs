using EntityStates.Engi.Mine;

namespace BadAssEngi.Skills.Secondary.OrbitalStrike.MineState.ArmingStateMachine
{
    public class MineArmingUnarmedOrbital : BaseMineArmingState
    {
        public override void OnEnter()
        {
            CheckInitState();

            base.OnEnter();
        }

        private void CheckInitState()
        {
            var goodState = (MineArmingUnarmed) Instantiate(typeof(MineArmingUnarmed));

            pathToChildToEnable = goodState.pathToChildToEnable;
            onEnterSfxPlaybackRate = goodState.onEnterSfxPlaybackRate;
            onEnterSfx = goodState.onEnterSfx;
            triggerRadius = goodState.triggerRadius;
            blastRadiusScale = goodState.blastRadiusScale;
            forceScale = goodState.forceScale;
            damageScale = goodState.damageScale;
        }
    }
}