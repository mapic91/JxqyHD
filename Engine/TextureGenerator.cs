using System;
using System.Collections.Generic;
using System.Linq;
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
            var trans = (byte)(transparent * 255);
            int width = texture2D.Width, height = texture2D.Height;
            var data = new Color[width * height];
            texture2D.GetData(data);
            var tex = new Texture2D(texture2D.GraphicsDevice, width, height);
            for (var idx = 0; idx < width * height; idx++)
            {
                data[idx].A = trans;
            }
            tex.SetData(data);
            return tex;
        }

        public static Texture2D AddOuterEdge(Texture2D texture2D, Color color)
        {
            if (texture2D == null) return null;
            int width = texture2D.Width, height = texture2D.Height;
            var bound = texture2D.Bounds;
            var total = (width) * (height);
            var data = new Color[total];
            texture2D.GetData(data);
            var edge = new List<int>();

            for (var i = 0; i < total; i++)
            {
                if (IsColorTransparentForNpcObj(data[i])) continue;
                var neighbers = new int[]
                {
                    i - width,
                    i - width + 1,
                    i + 1,
                    i + width + 1,
                    i + width,
                    i + width - 1,
                    i - 1,
                    i - width - 1
                };
                foreach (var neighber in neighbers)
                {
                    if (neighber >= 0 && neighber < total)
                    {
                        if (IsColorTransparentForNpcObj(data[neighber]))
                        {
                            edge.Add(i);
                            break;
                        }
                    }
                }
            }

            //Check top right bottom left
            var beginBottom = (height - 1)*width;
            for (var w = 0; w < width; w++)
            {
                if(!IsColorTransparentForNpcObj(data[w]))
                    edge.Add(w);
                if(!IsColorTransparentForNpcObj(data[beginBottom + w]))
                    edge.Add(beginBottom + w);
            }
            var beginLeft = 0;
            var beginRight = width - 1;
            for (var h = 0; h < height; h++)
            {
                if(!IsColorTransparentForNpcObj(data[beginLeft]))
                    edge.Add(beginLeft);
                if(!IsColorTransparentForNpcObj(data[beginRight]))
                    edge.Add(beginRight);
                beginLeft += width;
                beginRight += width;
            }

            foreach (var i in edge)
            {
                data[i] = color;
            }
            var tex = new Texture2D(texture2D.GraphicsDevice, width, height);
            tex.SetData(data);
            return tex;
        }

        public static bool IsColorTransparentForNpcObj(Color color)
        {
            var col = color;
            col.A = 0xFF;
            return (color.A == 0 || col == Color.Black);
        }
    }
}