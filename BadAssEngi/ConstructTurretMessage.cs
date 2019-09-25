using UnityEngine;
using UnityEngine.Networking;
// ReSharper disable All

namespace BadAssEngi
{
    class ConstructTurretMessage : MessageBase
    {
        public GameObject builder;
        public Vector3 position;
        public Quaternion rotation;

        public override void Serialize(NetworkWriter writer)
        {
            writer.Write(this.builder);
            writer.Write(this.position);
            writer.Write(this.rotation);
        }
        public override void Deserialize(NetworkReader reader)
        {
            this.builder = reader.ReadGameObject();
            this.position = reader.ReadVector3();
            this.rotation = reader.ReadQuaternion();
        }
    }
}
