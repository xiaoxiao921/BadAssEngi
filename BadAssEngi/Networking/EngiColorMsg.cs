using BadAssEngi.Assets;
using BadAssEngi.Util;
using R2API.Networking.Interfaces;
using RoR2;
using UnityEngine;
using UnityEngine.Networking;

namespace BadAssEngi.Networking
{
    internal struct EngiColorMsg : INetMessage
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
            var characterBody = ClientScene.FindLocalObject(NetId).GetComponent<CharacterBody>();
            var characterModel = characterBody.modelLocator.modelTransform
                .GetComponent<CharacterModel>();

            var colorVec = new Vector3(Color.r, Color.g, Color.b);
            var originalColor = colorVec.x == -1 && colorVec.y == -1 && colorVec.z == -1;

            var infoSize = characterModel.baseRendererInfos.Length;
            var rendererInfo = characterModel.baseRendererInfos[infoSize - 1];

            var material = rendererInfo.defaultMaterial;

            foreach (var id in material.GetTexturePropertyNameIDs())
            {
                var texture = material.GetTexture(id) as Texture2D;
                if (texture)
                {
                    if (texture.name.ToLower().Contains("diffuse"))
                    {
                        var altSkin = characterBody.master.loadout.bodyLoadoutManager.GetSkinIndex(BadAssEngi.EngiBodyIndex) != 0;
                        if (altSkin && BadAssEngi.OrigAltEngiTexture == null)
                        {
                            var tmp = RenderTexture.GetTemporary(texture.width, texture.height);
                            Graphics.Blit(texture, tmp);
                            var previous = RenderTexture.active;
                            RenderTexture.active = tmp;
                            BadAssEngi.OrigAltEngiTexture = new Texture2D(texture.width, texture.height);
                            BadAssEngi.OrigAltEngiTexture.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
                            BadAssEngi.OrigAltEngiTexture.Apply();
                            RenderTexture.active = previous;
                            RenderTexture.ReleaseTemporary(tmp);
                        }
                        else if (BadAssEngi.OrigEngiTexture == null)
                        {
                            var tmp = RenderTexture.GetTemporary(texture.width, texture.height);
                            Graphics.Blit(texture, tmp);
                            var previous = RenderTexture.active;
                            RenderTexture.active = tmp;
                            BadAssEngi.OrigEngiTexture = new Texture2D(texture.width, texture.height);
                            BadAssEngi.OrigEngiTexture.ReadPixels(new Rect(0, 0, tmp.width, tmp.height), 0, 0);
                            BadAssEngi.OrigEngiTexture.Apply();
                            RenderTexture.active = previous;
                            RenderTexture.ReleaseTemporary(tmp);
                        }

                        if (originalColor)
                        {
                            material.SetTexture(id, altSkin ? BadAssEngi.OrigAltEngiTexture : BadAssEngi.OrigEngiTexture);
                        }
                        else
                        {
                            material.SetTexture(id, TextureUtil.ReplaceWithRamp(texture, colorVec, 0f));
                        }
                        var engiMaterial = rendererInfo.defaultMaterial;
                        var engiCustomMeshRenderer = BaeAssets.PrefabEngiCustomAnimation.GetComponentInChildren<SkinnedMeshRenderer>();
                        engiCustomMeshRenderer.material = engiMaterial;
                    }
                }
            }
        }
    }
}
