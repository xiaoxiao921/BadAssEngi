using UnityEngine.Networking;

namespace BadAssEngi
{
    public class StopSound : NetworkBehaviour
    {
        public uint SoundId;

        void OnEnable()
        {
            SoundId = AkSoundEngine.PostEvent(SoundHelper.SeekerGrenadeFiring, gameObject);
        }

        void OnDisable()
        {
            AkSoundEngine.StopPlayingID(SoundId);
        }
    }
}
