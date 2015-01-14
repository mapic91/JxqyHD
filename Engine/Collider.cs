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

        public static bool IsPixelCollideForNpcObj(Vector2 position, Rectangle region, Texture2D texture)
        {
            var point = new Point((int)position.X, (int)position.Y);
            if (texture != null && region.Contains(point))
            {
                var offX = point.X - region.Left;
                var offY = point.Y - region.Top;
                var data = new Color[texture.Width * texture.Height];
                texture.GetData(data);
                if (!TextureGenerator.IsColorTransparentForNpcObj(data[offX + offY * texture.Width]))
                    return true;
            }
            return false;
        }


        //isMakeATransparent: Make textureA pixel transparent to "transparentValue" if collided
        public static bool IsPixelcollide(Rectangle regionA, Texture2D textureA, Rectangle regionB, Texture2D textureB)
        {
            if (textureA != null && textureB != null)
            {
                var intersect = Rectangle.Intersect(regionA, regionB);
                if (!intersect.IsEmpty)
                {
                    var widthA = textureA.Width;
                    var heightA = textureA.Height;
                    var widthB = textureB.Width;
                    var heightB = textureB.Height;
                    var dataA = new Color[widthA * heightA];
                    var dataB = new Color[widthB * heightB];
                    textureA.GetData(dataA);
                    textureB.GetData(dataB);
                    var beginX = intersect.Left - regionA.Left;
                    var beginY = intersect.Top - regionA.Top;
                    var endX = beginX + intersect.Width;
                    var endY = beginY + intersect.Height;
                    for (var y = beginY; y < endY; y++)
                    {
                        for (var x = beginX; x < endX; x++)
                        {
                            if (dataA[y*widthA + x].A != 0)
                            {
                                var bX = x + regionA.Left - regionB.Left;
                                var bY = y + regionA.Top - regionB.Top;
                                if (dataB[bY*widthB + bX].A != 0)
                                    return true;
                            }
                        }
                    }

                }
            }
            return false;
        }

        //Make targetTexture's pixel transparent if collided with colliderTexture's pixel
        public static void MakePixelCollidedTransparent(Rectangle targetRegion, Texture2D targetTexture, Rectangle colliderRegion, Texture2D colliderTexture, float transparentValue = 0.5f)
        {
            if (targetTexture != null && colliderTexture != null)
            {
                var mask = 255*transparentValue;
                var intersect = Rectangle.Intersect(targetRegion, colliderRegion);
                if (!intersect.IsEmpty)
                {
                    var widthA = targetTexture.Width;
                    var heightA = targetTexture.Height;
                    var widthB = colliderTexture.Width;
                    var heightB = colliderTexture.Height;
                    var dataA = new Color[widthA * heightA];
                    var dataB = new Color[widthB * heightB];
                    targetTexture.GetData(dataA);
                    colliderTexture.GetData(dataB);
                    var beginX = intersect.Left - targetRegion.Left;
                    var beginY = intersect.Top - targetRegion.Top;
                    var endX = beginX + intersect.Width;
                    var endY = beginY + intersect.Height;
                    for (var y = beginY; y < endY; y++)
                    {
                        for (var x = beginX; x < endX; x++)
                        {
                            var index = y*widthA + x;
                            if (dataA[index].A > mask)
                            {
                                var bX = x + targetRegion.Left - colliderRegion.Left;
                                var bY = y + targetRegion.Top - colliderRegion.Top;
                                if (dataB[bY*widthB + bX].A != 0)
                                    dataA[index] *= transparentValue;
                            }
                        }
                    }
                    targetTexture.SetData(dataA);
                }
            }
        }
    }
}
