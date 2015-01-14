using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine
{
    public static class TextureGenerator
    {
        //sourceTexture is unchanged
        public static Texture2D MakeTransparent(Texture2D sourceTexture, float transparent)
        {
            if (sourceTexture == null) return null;
            int width = sourceTexture.Width, height = sourceTexture.Height;
            var data = new Color[width * height];
            sourceTexture.GetData(data);
            var tex = new Texture2D(sourceTexture.GraphicsDevice, width, height);
            for (var idx = 0; idx < width * height; idx++)
            {
                data[idx] *= transparent;
            }
            tex.SetData(data);
            return tex;
        }

        public static Texture2D MakeTransparentFromTop(Texture2D sourceTexture, float opaquePercentFromBottom)
        {
            if (sourceTexture == null) return null;
            if (opaquePercentFromBottom < 0f) opaquePercentFromBottom = 0f;
            else if (opaquePercentFromBottom > 1f) opaquePercentFromBottom = 1f;
            int width = sourceTexture.Width, height = sourceTexture.Height;
            var data = new Color[width * height];
            sourceTexture.GetData(data);
            var tex = new Texture2D(sourceTexture.GraphicsDevice, width, height);
            var transHeight = sourceTexture.Height*(1 - opaquePercentFromBottom);
            for (var w = 0; w < width; w++)
            {
                for (var h = 0; h < transHeight; h++)
                {
                    data[h*width + w] *= 0;
                }
            }
            tex.SetData(data);
            return tex;
        }

        public static Texture2D ToGrayScale(Texture2D texture2D)
        {
            if (texture2D == null) return null;
            int width = texture2D.Width, height = texture2D.Height;
            var total = (width) * (height);
            var data = new Color[total];
            texture2D.GetData(data);
            for (var i = 0; i < total; i++)
            {
                var arg = (byte) ((data[i].R + data[i].G + data[i].B)/3);
                data[i].R = data[i].G = data[i].B = arg;
            }
            var tex = new Texture2D(texture2D.GraphicsDevice, width, height);
            tex.SetData(data);
            return tex;
        }

        public static Texture2D GetOuterEdge(Texture2D texture2D, Color color)
        {
            if (texture2D == null) return null;
            int width = texture2D.Width, height = texture2D.Height;
            var total = (width) * (height);
            var data = new Color[total];
            texture2D.GetData(data);
            var edge = new List<int>();

            for (var i = 0; i < total; i++)
            {
                if (!IsColorTransparentForNpcObj(data[i])) continue;
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
                        if (!IsColorTransparentForNpcObj(data[neighber]))
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

            data = new Color[total];
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
            return (color.A < 200);
        }

        public static Texture2D GetColorTexture(Color color, int width, int height)
        {
            if (width <= 0 || height <= 0) return null;
            var size = width*height;
            var data = new Color[size];
            for (var i = 0; i < size; i++)
                data[i] = color;
            var texture = new Texture2D(Globals.TheGame.GraphicsDevice, width, height);
            texture.SetData(data);
            return texture;
        }

        public static Asf[] GetSnowFlake()
        {
            var asfs = new Asf[4];
            var w = Color.White;
            var t = Color.Transparent;
            var data = new Color[3*3]
            {
                t, w, t,
                w, w, w,
                t, w, t
            };
            var texture = new Texture2D(Globals.TheGame.GraphicsDevice, 3, 3);
            texture.SetData(data);
            asfs[0] = new Asf(texture);
            data = new Color[2*2]
            {
                t, w,
                w, t
            };
            texture = new Texture2D(Globals.TheGame.GraphicsDevice, 2, 2);
            texture.SetData(data);
            asfs[1] = new Asf(texture);
            data = new Color[2*2]
            {
                w, t,
                t, w
            };
            texture = new Texture2D(Globals.TheGame.GraphicsDevice, 2, 2);
            texture.SetData(data);
            asfs[2] = new Asf(texture);
            data = new Color[1]
            {
                w
            };
            texture = new Texture2D(Globals.TheGame.GraphicsDevice, 1, 1);
            texture.SetData(data);
            asfs[3] = new Asf(texture);
            return asfs;
        }

        public static Texture2D GetRaniDrop()
        {
            var w = Color.Gray*0.3f;
            var l = Color.Gray * 0.2f;
            var data = new Color[1*20]
            {
                l,
                l,
                l,
                l,
                l,
                l,
                w,
                w,
                w,
                w,
                w,
                w,
                w,
                w,
                w,
                l,
                l,
                l,
                l,
                l
            };
            var texture = new Texture2D(Globals.TheGame.GraphicsDevice, 1, 20);
            texture.SetData(data);
            return texture;
        }
    }
}