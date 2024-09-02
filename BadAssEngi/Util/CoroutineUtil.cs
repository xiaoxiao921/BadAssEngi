using System;
using System.Collections;
using BadAssEngi.Assets.Sound;
using BadAssEngi.Skills.Special;
using RoR2;
using UnityEngine;
using Object = UnityEngine.Object;

// ReSharper disable UnusedMember.Global

namespace BadAssEngi.Util
{
    internal static class CoroutineUtil
    {
        internal static IEnumerator DelayedMethod(float seconds, Action method)
        {
            yield return new WaitForSeconds(seconds);

            method();
        }

        internal static IEnumerator DelayedSound(string soundName, GameObject soundEmitter, float seconds)
        {
            yield return new WaitForSeconds(seconds);

            var atkspd = soundEmitter.GetComponent<CharacterMaster>().GetBodyObject().GetComponent<CharacterBody>()
                .attackSpeed;
            soundEmitter.GetComponent<BadAssTurret>().SoundGunId = RoR2.Util.PlaySound(soundName, soundEmitter,
                SoundHelper.TurretRTPCAttackSpeed, atkspd);
            Log.Debug("Railgun Turret Attack Speed : " + atkspd);

        }

        internal static IEnumerator DelayedDestroy(GameObject gameObject, float seconds)
        {
            yield return new WaitForSeconds(seconds);

            Object.Destroy(gameObject);
        }
    }
}
