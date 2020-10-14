using UnityEngine;

namespace BadAssEngi.Assets.Sound
{
    public class StopSound : MonoBehaviour
    {
        public uint SoundId;
        public bool Played;
        public string SoundEventToPlay = SoundHelper.SeekerGrenadeFiring;

        void OnEnable()
        {
            if (!Played)
            {
                Played = true;
                SoundId = AkSoundEngine.PostEvent(SoundEventToPlay, gameObject);
            }
        }

        void OnDisable()
        {
            AkSoundEngine.StopPlayingID(SoundId);
            Destroy(gameObject);
        }
    }
}
