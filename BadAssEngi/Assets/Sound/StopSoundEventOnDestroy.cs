using UnityEngine;

namespace BadAssEngi.Assets.Sound
{
    public class StopSoundEventOnDestroy : MonoBehaviour
    {
        public uint SoundId;

        void OnDestroy()
        {
            AkSoundEngine.StopPlayingID(SoundId);
        }
    }
}