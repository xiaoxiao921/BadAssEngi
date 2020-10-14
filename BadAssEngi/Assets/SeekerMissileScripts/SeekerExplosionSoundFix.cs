using BadAssEngi.Assets.Sound;
using UnityEngine;

namespace BadAssEngi.Assets.SeekerMissileScripts
{
    public class SeekerExplosionSoundFix : MonoBehaviour
    {
        public uint SoundId;
        public bool Played;
        public string SoundEventToPlay = SoundHelper.RocketTurretExplosion;

        void Start()
        {
            if (!Played)
            {
                Played = true;
                SoundId = AkSoundEngine.PostEvent(SoundEventToPlay, gameObject);

                StartCoroutine(Util.CoroutineUtil.DelayedMethod(2f, () =>
                {
                    AkSoundEngine.StopPlayingID(SoundId);
                    Destroy(gameObject);
                }));
            }
        }
    }
}