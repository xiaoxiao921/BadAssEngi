using BadAssEngi.Animations;
using BadAssEngi.Assets;
using R2API.Networking.Interfaces;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace BadAssEngi.Networking
{
    public struct AnimMsg : INetMessage
    {
        public int AnimId;
        public NetworkInstanceId EngiNetId;
        public NetworkInstanceId TempNetId;

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(AnimId);
            writer.Write(EngiNetId);
            writer.Write(TempNetId);
        }

        public void Deserialize(NetworkReader reader)
        {
            AnimId = reader.ReadInt32();
            EngiNetId = reader.ReadNetworkId();
            TempNetId = reader.ReadNetworkId();
        }

        public void OnReceived()
        {
            // Destination : all clients

            var animId = BaeAssets.EngiAnimations[AnimId];

            var origEngiBody = ClientScene.FindLocalObject(EngiNetId);
            var currentModel = origEngiBody.GetComponent<ModelLocator>().modelTransform;
            currentModel.localScale = new Vector3(0, 1, 0);

            var engiAnimated = Object.Instantiate(BaeAssets.PrefabEngiCustomAnimation, currentModel.position,
                currentModel.rotation * Quaternion.Euler(Vector3.up * -5));

            var animator = engiAnimated.GetComponent<Animator>();
            animator.Play(animId);

            EngiEmoteController.EngiNetIdToTempGO[EngiNetId] = engiAnimated;
            EngiEmoteController.EngiNetIdToSoundEvent[EngiNetId] = AkSoundEngine.PostEvent(animId, engiAnimated);

            AkSoundEngine.SetRTPCValue("Volume_Emote", Configuration.EmoteVolume.Value, engiAnimated);

            EngiEmoteController.NumberOfEmotePlaying++;
        }
    }
}