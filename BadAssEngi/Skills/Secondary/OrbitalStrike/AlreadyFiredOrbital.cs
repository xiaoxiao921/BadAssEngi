using UnityEngine;

namespace BadAssEngi.Skills.Secondary.OrbitalStrike
{
    public class AlreadyFiredOrbital : MonoBehaviour
    {
        public bool Fired;

        public void Awake()
        {
            Fired = true;
        }
    }
}
