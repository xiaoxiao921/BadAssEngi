using RoR2.Projectile;
using UnityEngine;

namespace BadAssEngi.Skills.Primary.SeekerMissile
{
    public class SwarmFix : MonoBehaviour
    {
        private void OnDestroy()
        {
            if (!gameObject)
                return;

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