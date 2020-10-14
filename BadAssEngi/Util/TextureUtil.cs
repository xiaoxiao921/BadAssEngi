using UnityEngine;

namespace BadAssEngi.Util
{
    internal static class TextureUtil
    {
        internal static Texture2D ReplaceWithRamp(Texture2D origTex, Vector3 vec, float startGrad)
        {
            var texture2D = new Texture2D(origTex.width, origTex.height, TextureFormat.RGBA32, false);
            var num = Mathf.CeilToInt(startGrad * 255f);
            var num2 = texture2D.width - num;
            var color = new Color32(0, 0, 0, 0);
            var c = new Color32(0, 0, 0, 0);
            for (var i = 0; i < texture2D.width; i++)
            {
                if (i >= num)
                {
                    var num3 = ((float)i - num) / num2;
                    c.r = (byte)Mathf.RoundToInt(255f * num3 * vec.x);
                    c.g = (byte)Mathf.RoundToInt(255f * num3 * vec.y);
                    c.b = (byte)Mathf.RoundToInt(255f * num3 * vec.z);
                    c.a = (byte)Mathf.RoundToInt(128f * num3);
                }
                else
                {
                    c = color;
                }
                for (var j = 0; j < texture2D.height; j++)
                {
                    texture2D.SetPixel(i, j, c);
                }
            }
            texture2D.wrapMode = TextureWrapMode.Clamp;
            texture2D.Apply();
            return texture2D;
        }
    }
}
