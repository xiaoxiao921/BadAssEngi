using UnityEngine;

namespace BadAssEngi.Assets.SeekerMissileScripts
{
    class RocketSmokeController : MonoBehaviour
    {
        private Transform _smoke;
        private ParticleSystem[] _particleSystems;

        public void Awake()
        {
            _particleSystems = gameObject.GetComponentsInChildren<ParticleSystem>();

            var index = transform.childCount;
            _smoke = transform.GetChild(index - 1);
        }

        public void OnDisable()
        {
            foreach (var particleSystem in _particleSystems)
            {
                particleSystem.enableEmission = false;
            }

            _smoke.SetParent(null);
            Destroy(_smoke.gameObject, 3f);
        }
    }
}
