using BadAssEngi.Assets;
using R2API.Networking.Interfaces;
using UnityEngine.Networking;

namespace BadAssEngi.Networking
{
    internal struct RebarColorMsg : INetMessage
    {
        internal byte Id;

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(Id);
        }

        public void Deserialize(NetworkReader reader)
        {
            Id = reader.ReadByte();
        }

        public void OnReceived()
        {
            if (Id == 1)
            {
                BaeAssets.RailGunTrailMaterial.SetColor(152, BaeAssets.EngiRebarColor);
            }

            if (Id == 2)
            {
                BaeAssets.RailGunTrailMaterial.SetColor(152, BaeAssets.OrigRebarColor);
            }

            if (Id == 3)
            {
                BaeAssets.RailGunTrailMaterial.SetColor(152, BaeAssets.EngiRebarColor);
            }
        }
    }
}
