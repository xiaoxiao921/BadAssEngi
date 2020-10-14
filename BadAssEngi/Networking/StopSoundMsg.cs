using R2API.Networking.Interfaces;
using UnityEngine.Networking;

namespace BadAssEngi.Networking
{
    internal struct StopSoundMsg : INetMessage
    {
        internal uint soundPlayId;

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(soundPlayId);
        }

        public void Deserialize(NetworkReader reader)
        {
            soundPlayId = reader.ReadUInt32();
        }

        public void OnReceived()
        {
            // Destinations all clients

            AkSoundEngine.StopPlayingID(soundPlayId);
        }
    }
}
