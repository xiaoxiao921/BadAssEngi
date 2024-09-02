using RoR2.Projectile;
using UnityEngine;

namespace BadAssEngi.Assets.SeekerMissileScripts
{
    class RocketSmokeControllerGearboxEdition : MonoBehaviour, IProjectileImpactBehavior
    {
        public void OnProjectileImpact(ProjectileImpactInfo impactInfo)
        {
            try
            {
                var ghostGo = this.GetComponent<ProjectileController>().ghost.gameObject;

                var particleSystems = ghostGo.GetComponentsInChildren<ParticleSystem>();

                var index = ghostGo.transform.childCount;
                var smoke = ghostGo.transform.GetChild(index - 1);

                foreach (var particleSystem in particleSystems)
                {
#pragma warning disable CS0618 // Type or member is obsolete
                    particleSystem.enableEmission = false;
#pragma warning restore CS0618 // Type or member is obsolete
                }

                smoke.SetParent(null);
                Destroy(smoke.gameObject, 3f);
            }
            catch (System.Exception e)
            {
                Log.Warning(e);
            }
        }
    }
}
