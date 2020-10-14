using R2API.Networking.Interfaces;
using UnityEngine;
using UnityEngine.Networking;

namespace BadAssEngi.Networking
{
    internal struct PlaySoundMsg : INetMessage
    {
        internal string SoundName;
        internal NetworkInstanceId NetId;

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(SoundName);
            writer.Write(NetId);
        }

        public void Deserialize(NetworkReader reader)
        {
            SoundName = reader.ReadString();
            NetId = reader.ReadNetworkId();
        }

        public void OnReceived()
        {
            // Destinations all clients

            var soundEmitter = ClientScene.FindLocalObject(NetId);
            if (soundEmitter)
            {
                AkSoundEngine.PostEvent(SoundName, soundEmitter);

                // ugly but easiest fix for sound not firing
                if (soundEmitter.name.Equals("EngiMine(Clone)") ||
                    soundEmitter.name.Equals("EngiSeekerGrenadeProjectile(Clone)") ||
                    soundEmitter.name.Equals("EngiGrenadeProjectile(Clone)"))
                {
                    Object.Destroy(soundEmitter);
                }
            }
        }
    }
}
