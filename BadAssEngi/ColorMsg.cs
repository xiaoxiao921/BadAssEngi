using UnityEngine;
using UnityEngine.Networking;

namespace BadAssEngi
{
    public class ColorMsg : MessageBase
    {
        public Color Color;
        public NetworkInstanceId NetId;

        public override void Deserialize(NetworkReader reader)
        {
            Color = reader.ReadColor();
            NetId = reader.ReadNetworkId();
        }
        
        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(Color);
            writer.Write(NetId);
        }
    }
}
