using System.Collections;
using EntityStates.Engi.Mine;
using RoR2;
using UnityEngine;

namespace BadAssEngi
{
    internal class RecursiveMine : MonoBehaviour
    {
        public int RecursiveDepth;

        public void Awake()
        {
            if (RecursiveDepth == 0) return;

            var seconds = Random.Range(1.25f, 2.25f);
            StartCoroutine(DelayedExplosion(seconds));
        }

        public void Init()
        {
            RecursiveDepth = 0;
        }

        private IEnumerator DelayedExplosion(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            EntityStateMachine.FindByCustomName(gameObject, "Main").SetNextState(new PreDetonate());
        }
    }
}
