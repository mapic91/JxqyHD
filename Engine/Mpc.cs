using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine
{
    public class Mpc : TextureBase
    {
        protected override bool LoadHead(byte[] buf, ref int offset)
        {
            var headinfo = Globals.SimpleChinaeseEncoding.GetString(buf, 0, "MPC File Ver".Length);
            if (!headinfo.Equals("MPC File Ver")) return false;
            offset = 64;
            Head.FramesDataLengthSum = Utils.GetLittleEndianIntegerFromByteArray(buf, ref offset);
            Head.GlobleWidth = Utils.GetLittleEndianIntegerFromByteArray(buf, ref offset);
            Head.GlobleHeight = Utils.GetLittleEndianIntegerFromByteArray(buf, ref offset);
            Head.FrameCounts = Utils.GetLittleEndianIntegerFromByteArray(buf, ref offset);
            Head.Direction = Utils.GetLittleEndianIntegerFromByteArray(buf, ref offset);
            Head.ColourCounts = Utils.GetLittleEndianIntegerFromByteArray(buf, ref offset);
            Head.Interval = Utils.GetLittleEndianIntegerFromByteArray(buf, ref offset);
            Head.Bottom = Utils.GetLittleEndianIntegerFromByteArray(buf, ref offset);
            offset += 32;
            return base.LoadHead(buf, ref offset);
        }       
        protected override void LoadFrame(byte[] buf, ref int offset)
        {
            var databeg = offset + 4 * Head.FrameCounts;
            var dataoffset = new int[Head.FrameCounts];
            for (var i = 0; i < Head.FrameCounts; i++)
                dataoffset[i] = Utils.GetLittleEndianIntegerFromByteArray(buf, ref offset);
            for (var j = 0; j < Head.FrameCounts; j++)
            {
                var datastart = databeg + dataoffset[j];
                var datalen = Utils.GetLittleEndianIntegerFromByteArray(buf, ref datastart);
                var width = Utils.GetLittleEndianIntegerFromByteArray(buf, ref datastart);
                var height = Utils.GetLittleEndianIntegerFromByteArray(buf, ref datastart);
                var data = new Color[width*height];
                datastart += 8;
                var dataidx = 0;
                var dataend = datastart + datalen - 20;
                while(datastart < dataend)
                {
                    if (buf[datastart] > 0x80)
                    {
                        var transparentcount = buf[datastart] - 0x80;
                        for (var ti = 0; ti < transparentcount; ti++)
                        {
                            data[dataidx++] = Color.Transparent;
                        }
                        datastart++;
                    }
                    else
                    {
                        var colorcount = buf[datastart++];
                        for (var ci = 0; ci < colorcount; ci++)
                        {
                            data[dataidx++] = Palette[buf[datastart++]];
                        }
                    }
                }
                var texture = new Texture2D(Globals.TheGame.GraphicsDevice, width, height);
                texture.SetData(data);
                Frames[j] = texture;
            }

            Palette = null;//palette can be released now
        }

        public Mpc(string path)
        {
            Load(path);
        }

        public new Texture2D GetFrame(int index)
        {
            if (index >= 0 && index < FrameCounts)
                return Frames[index%FrameCounts];
            return null;
        }
    }
}
