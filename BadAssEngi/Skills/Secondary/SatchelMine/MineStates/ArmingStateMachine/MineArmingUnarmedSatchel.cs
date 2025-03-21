﻿using EntityStates.Engi.Mine;

namespace BadAssEngi.Skills.Secondary.SatchelMine.MineStates.ArmingStateMachine
{
    public class MineArmingUnarmedSatchel : BaseMineArmingState
    {
        public override void OnEnter()
        {
            CheckInitState();

            base.OnEnter();
        }

        private void CheckInitState()
        {
            var goodState = new MineArmingUnarmed();

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
    }
}