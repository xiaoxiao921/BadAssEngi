using UnityEngine.Networking;

namespace BadAssEngi
{
    public class SoundMsg : MessageBase
    {
        public string SoundName;
        public NetworkInstanceId NetId;

        public override void Deserialize(NetworkReader reader)
        {
            SoundName = reader.ReadString();
            NetId = reader.ReadNetworkId();
        }
        
        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(SoundName);
            writer.Write(NetId);
        }
    }
}
