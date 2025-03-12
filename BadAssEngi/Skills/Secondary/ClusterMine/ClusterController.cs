using System.Collections.Generic;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

// ReSharper disable UnusedMember.Local
// ReSharper disable once ClassNeverInstantiated.Global

namespace BadAssEngi.Skills.Secondary.ClusterMine
{
    public class ClusterController : MonoBehaviour
    {
        public GameObject Owner;
        public float Damage;

        private Renderer[] _clusterRenderers;
        private List<Material> _materials;

        private float _startTime, _totalTime;
        private const int TotalTime = 3;
        private const int TotalTimeBounce = 15;

        private void Awake()
        {
            _startTime = Time.time;
            _totalTime = Configuration.ClusterMineVisualBouncing.Value ? TotalTimeBounce : TotalTime;

            _clusterRenderers = gameObject.GetComponentsInChildren<Renderer>();
            _materials = new List<Material>();
        }

        private void Update()
        {
            if (Time.time > _startTime + _totalTime)
                Destroy(gameObject);
        }

        private void OnParticleCollision(GameObject other)
        {
            if (!Configuration.ClusterMineVisualBouncing.Value)
                return;

            if (!NetworkServer.active)
                return;

            var r = (int)Random.Range(0, 2.99f);
            var g = (int)Random.Range(0, 2.99f);
            var b = (int)Random.Range(0, 2.99f);

            if (r == 2)
            {
                g = (int)Random.Range(0, 1.99f);

                if (g == 1)
                    b = 0;
                else
                    b = (int)Random.Range(0, 1.99f);
            }
            else if (g == 2)
            {
                r = (int)Random.Range(0, 1.99f);

                if (r == 1)
                    b = 0;
                else
                    b = (int)Random.Range(0, 1.99f);
            }
            else if (b == 2)
            {
                r = (int)Random.Range(0, 1.99f);

                if (r == 1)
                    g = 0;
                else
                    g = (int)Random.Range(0, 1.99f);
            }

            var color = new Color(r, g, b);
            foreach (var clusterRenderer in _clusterRenderers)
            {
                _materials.Clear();
                clusterRenderer.GetMaterials(_materials);
                foreach (var material in _materials)
                {
                    material.color = color;
                }
            }

            var hurtBox = other.GetComponent<HurtBox>();
            if (!hurtBox)
                return;

            var healthComponent = hurtBox.healthComponent;

            if (healthComponent && healthComponent.GetComponent<TeamComponent>().teamIndex != TeamIndex.Player)
            {
                var damageInfo = new DamageInfo
                {
                    damage = Damage,
                    position = transform.position,
                    force = Vector3.zero,
                    damageColorIndex = DamageColorIndex.Default,
                    crit = false,
                    attacker = Owner,
                    inflictor = gameObject,
                    damageType = DamageTypeCombo.GenericSecondary,
                    procCoefficient = 0f,
                    procChainMask = default
                };

                healthComponent.TakeDamage(damageInfo);
            }
        }
    }
}
