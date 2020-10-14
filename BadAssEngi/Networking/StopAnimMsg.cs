using BadAssEngi.Animations;
using R2API.Networking.Interfaces;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace BadAssEngi.Networking
{
    public struct StopAnimMsg : INetMessage
    {
        public NetworkInstanceId EngiNetId;

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(EngiNetId);
        }

        public void Deserialize(NetworkReader reader)
        {
            EngiNetId = reader.ReadNetworkId();
        }

        public void OnReceived()
        {
            // Destination : all clients

            var origEngiBody = ClientScene.FindLocalObject(EngiNetId);
            var currentModel = origEngiBody.GetComponent<ModelLocator>().modelTransform;
            currentModel.localScale = Vector3.one;

            if (EngiEmoteController.EngiNetIdToTempGO.ContainsKey(EngiNetId))
            {
                var animObject = EngiEmoteController.EngiNetIdToTempGO[EngiNetId];
                Object.Destroy(animObject);

                EngiEmoteController.EngiNetIdToTempGO.Remove(EngiNetId);
            }

            if (EngiEmoteController.EngiNetIdToSoundEvent.ContainsKey(EngiNetId))
            {
                AkSoundEngine.StopPlayingID(EngiEmoteController.EngiNetIdToSoundEvent[EngiNetId]);

                EngiEmoteController.EngiNetIdToSoundEvent.Remove(EngiNetId);
            }

            EngiEmoteController.NumberOfEmotePlaying--;
        }
    }
}