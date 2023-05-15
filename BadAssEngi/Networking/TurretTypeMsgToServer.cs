using BadAssEngi.Skills.Special;
using R2API.Networking.Interfaces;
using UnityEngine.Networking;

namespace BadAssEngi.Networking
{
    internal struct TurretTypeMsgToServer : INetMessage
    {
        internal byte TurretTypeId;

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(TurretTypeId);
        }

        public void Deserialize(NetworkReader reader)
        {
            TurretTypeId = reader.ReadByte();
        }

        public void OnReceived()
        {
            // Destination : Server
            TurretTypeController.SenderTurretType = (TurretType) TurretTypeId;
        }
    }
}
