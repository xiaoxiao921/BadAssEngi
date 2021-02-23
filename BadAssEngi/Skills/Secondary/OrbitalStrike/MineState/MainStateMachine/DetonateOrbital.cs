using BadAssEngi.Assets;
using BadAssEngi.Assets.Sound;
using EntityStates.Engi.Mine;
using UnityEngine;
using UnityEngine.Networking;

namespace BadAssEngi.Skills.Secondary.OrbitalStrike.MineState.MainStateMachine
{
	public class DetonateOrbital : BaseMineState
	{
        public override bool shouldStick => false;

        public override bool shouldRevertToWaitForStickOnSurfaceLost => false;

		public override void OnEnter()
		{
			base.OnEnter();
            AkSoundEngine.PostEvent(SoundHelper.OrbitalStrikeSound, outer.gameObject);
            if (NetworkServer.active)
			{
				Explode();
			}
		}

		private void Explode()
		{
            var alreadyFired = outer.gameObject.GetComponent<AlreadyFiredOrbital>();

            if (alreadyFired && alreadyFired.Fired)
                return;

            var orbitalStrike = Object.Instantiate(BaeAssets.PrefabOrbitalStrike, outer.transform.position,
                Quaternion.LookRotation(Vector3.forward, Vector3.up));
            NetworkServer.Spawn(orbitalStrike);
            
            outer.gameObject.AddComponent<AlreadyFiredOrbital>();

            var orbitalStrikeController = orbitalStrike.GetComponent<OrbitalStrikeController>();

            orbitalStrikeController.OwnerTeam = projectileController.teamFilter.teamIndex;
            orbitalStrikeController.Owner = projectileController.owner;
            orbitalStrikeController.InitCB();

            Object.Destroy(orbitalStrike.gameObject, OrbitalStrikeController.TotalTime + 10);
            Object.Destroy(gameObject, OrbitalStrikeController.TotalTime);
        }
	}
}
