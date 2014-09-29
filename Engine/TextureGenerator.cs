using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine
{
    public static class TextureGenerator
    {
        //texture2D is unchanged
        public static Texture2D MakeTransparent(Texture2D texture2D, float transparent)
        {
            if (texture2D == null) return null;
            var trans = (byte)(transparent*255);
            int width = texture2D.Width, height = texture2D.Height;
            var data = new Color[width*height];
            texture2D.GetData(data);
            var tex = new Texture2D(texture2D.GraphicsDevice, width, height);
            for (var idx = 0; idx < width*height; idx++)
            {
                data[idx].A = trans;
            }
            tex.SetData(data);
            return tex;
        }
    }
}