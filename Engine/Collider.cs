using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine
{
    public static class Collider
    {
        public static bool IsBoxCollide(Rectangle boxA, Rectangle boxB)
        {
            return boxA.Intersects(boxB);
        }

        public static bool IsPixelCollideForNpcObj(Point position, Rectangle region, Texture2D texture)
        {
            if (texture != null && region.Contains(position))
            {
                var offX = position.X - region.Left;
                var offY = position.Y - region.Top;
                var data = new Color[texture.Width*texture.Height];
                texture.GetData(data);
                if (!TextureGenerator.IsColorTransparentForNpcObj(data[offX + offY*texture.Width]))
                    return true;
            }
            return false;
        }
    }
}
