using BadAssEngi.Assets.Sound;
using BadAssEngi.Util;
using R2API.Networking.Interfaces;
using UnityEngine;
using UnityEngine.Networking;

namespace BadAssEngi.Networking
{
    internal struct PlaySoundAndDestroyMsg : INetMessage
    {
        internal string SoundName;
        internal NetworkInstanceId NetId;
        internal float DelayInSecondDestroy;

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(SoundName);
            writer.Write(NetId);
            writer.Write(DelayInSecondDestroy);
        }

        public void Deserialize(NetworkReader reader)
        {
            SoundName = reader.ReadString();
            NetId = reader.ReadNetworkId();
            DelayInSecondDestroy = reader.ReadSingle();
        }

        public void OnReceived()
        {
            // Destinations all clients

            var soundEmitter = ClientScene.FindLocalObject(NetId);
            if (soundEmitter)
            {
                var stopSoundEventOnDestroy = soundEmitter.AddComponent<StopSoundEventOnDestroy>();
                stopSoundEventOnDestroy.SoundId = AkSoundEngine.PostEvent(SoundName, soundEmitter);
                if (DelayInSecondDestroy == 0)
                {
                    Object.Destroy(soundEmitter);
                }
                else
                {
                    BadAssEngi.Instance.StartCoroutine(CoroutineUtil.DelayedDestroy(soundEmitter, DelayInSecondDestroy));
                }
            }
        }
    }
}
