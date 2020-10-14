using BadAssEngi.Util;
using R2API.Networking.Interfaces;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace BadAssEngi.Networking
{
    internal struct TurretColorMsg : INetMessage
    {
        internal Color Color;
        internal NetworkInstanceId NetId;

        public void Serialize(NetworkWriter writer)
        {
            writer.Write(Color);
            writer.Write(NetId);
        }

        public void Deserialize(NetworkReader reader)
        {
            Color = reader.ReadColor();
            NetId = reader.ReadNetworkId();
        }

        public void OnReceived()
        {
            var gameObject = ClientScene.FindLocalObject(NetId);
            if (!gameObject)
                return;

            var modelLocator = gameObject.GetComponent<ModelLocator>();
            if (!modelLocator)
                return;
            
            var characterModel = modelLocator.modelTransform.GetComponent<CharacterModel>();
            if (!characterModel)
                return;

            var colorVec = new Vector3(Color.r, Color.g, Color.b);
            var originalColor = colorVec.x == -1 && colorVec.y == -1 && colorVec.z == -1;
                
            foreach (var rendererInfo in characterModel.baseRendererInfos)
            {
                var material = rendererInfo.defaultMaterial;

                foreach (var id in material.GetTexturePropertyNameIDs())
                {
                    var texture = material.GetTexture(id) as Texture2D;

                    if (texture)
                    {
                        if (BadAssEngi.OrigTurretTexture == null)
                        {
                            var tmp = RenderTexture.GetTemporary(texture.width, texture.height);
                            Graphics.Blit(texture, tmp);
                            var previous = RenderTexture.active;
                            RenderTexture.active = tmp;
                            BadAssEngi.OrigTurretTexture = new Texture2D(texture.width, texture.height);
                            BadAssEngi.OrigTurretTexture.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
                            BadAssEngi.OrigTurretTexture.Apply();
                            RenderTexture.active = previous;
                            RenderTexture.ReleaseTemporary(tmp);
                        }

                        if (originalColor)
                        {
                            material.SetTexture(id, BadAssEngi.OrigTurretTexture);
                        }
                        else
                        {
                            material.SetTexture(id, TextureUtil.ReplaceWithRamp(texture, colorVec, -15f));
                        }
                    }
                }
            }
        }
    }
}
