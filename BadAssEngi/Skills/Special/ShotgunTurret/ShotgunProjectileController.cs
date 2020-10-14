using RoR2.Projectile;
using UnityEngine;

namespace BadAssEngi.Skills.Special.ShotgunTurret
{
    public class ShotgunProjectileController : MonoBehaviour
    {
        public new Transform transform;
        public Rigidbody rigidbody;
        public float giveupTimer = 8f;
        public float deathTimer = 10f;
        public float timer;

        private void Awake()
        {
            transform = base.transform;
            rigidbody = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (!gameObject)
                return;
            timer += Time.fixedDeltaTime;

            if (timer < giveupTimer)
            {
                rigidbody.velocity = transform.forward * 100f;
            }
            if (timer > deathTimer)
            {
                Destroy(gameObject);
                Destroy(gameObject.GetComponent<ProjectileController>().ghost.gameObject);
            }
        }

        private void OnDestroy()
        {
            var pc = gameObject.GetComponent<ProjectileController>();
            if (pc)
            {
                var ghost = pc.ghost;
                if (ghost && ghost.gameObject)
                {
                    Destroy(ghost.gameObject);
                }
            }
        }
    }
}
