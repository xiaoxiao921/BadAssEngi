using EntityStates.Engi.Mine;
using UnityEngine.Networking;

namespace BadAssEngi.Skills.Secondary.ClusterMine.MineStates.MainStateMachine
{
    public class PreDetonateCluster : BaseMineState
    {
        public override bool shouldStick => false;

        public override bool shouldRevertToWaitForStickOnSurfaceLost => false;

        public override void OnEnter()
        {
            base.OnEnter();
            transform.Find(PreDetonate.pathToPrepForExplosionChildEffect).gameObject.SetActive(true);
            rigidbody.AddForce(transform.forward * PreDetonate.detachForce);
            rigidbody.AddTorque(UnityEngine.Random.onUnitSphere * 200f);
        }

        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (NetworkServer.active && PreDetonate.duration <= fixedAge)
            {
                outer.SetNextState(new DetonateCluster());
            }
        }
    }
}