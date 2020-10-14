using System.Collections;
using BadAssEngi.Skills.Secondary.ClusterMine.MineStates.MainStateMachine;
using RoR2;
using UnityEngine;

namespace BadAssEngi.Skills.Secondary.ClusterMine
{
    public class RecursiveMine : MonoBehaviour
    {
        public int RecursiveDepth;

        public void Awake()
        {
            if (RecursiveDepth == 0) return;

            var seconds = Random.Range(1.25f, 2.25f);
            StartCoroutine(DelayedExplosion(seconds));
        }

        private IEnumerator DelayedExplosion(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            EntityStateMachine.FindByCustomName(gameObject, "Main").SetNextState(new PreDetonateCluster());
        }
    }
}
