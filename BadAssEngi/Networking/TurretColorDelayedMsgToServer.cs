using BadAssEngi.Skills.Special;
using R2API.Networking.Interfaces;
using UnityEngine;
using UnityEngine.Networking;

namespace BadAssEngi.Networking
{
    internal struct TurretColorDelayedMsgToServer : INetMessage
    {
        internal Color Color;

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(Color);
        }

        public void Deserialize(NetworkReader reader)
        {
            Color = reader.ReadColor();
        }

        public void OnReceived()
        {
            TurretTypeController.LatestTurretColorReceived = Color;
        }
    }
}
