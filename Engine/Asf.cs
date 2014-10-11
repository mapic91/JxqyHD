using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine
{
    public class Asf : TextureBase
    {
        protected override bool LoadHead(byte[] buf, ref int offset)
        {
            var headinfo = Encoding.GetEncoding(Globals.SimpleChinaeseCode).GetString(buf, 0, "ASF 1.0".Length);
            if (!headinfo.Equals("ASF 1.0")) return false;
            offset += 16;
            
            Head.GlobleWidth = Utils.GetLittleEndianIntegerFromByteArray(buf, ref offset);
            Head.GlobleHeight = Utils.GetLittleEndianIntegerFromByteArray(buf, ref offset);
            Head.FrameCounts = Utils.GetLittleEndianIntegerFromByteArray(buf, ref offset);
            Head.Direction = Utils.GetLittleEndianIntegerFromByteArray(buf, ref offset);
            Head.ColourCounts = Utils.GetLittleEndianIntegerFromByteArray(buf, ref offset);
            Head.Interval = Utils.GetLittleEndianIntegerFromByteArray(buf, ref offset);
            Head.Left = Utils.GetLittleEndianIntegerFromByteArray(buf, ref offset);
            Head.Bottom = Utils.GetLittleEndianIntegerFromByteArray(buf, ref offset);
            offset += 16;
            return true;
        }
        protected override void LoadFrame(byte[] buf, ref int offset)
        {
            var dataoffset = new int[Head.FrameCounts];
            var datalength = new int[Head.FrameCounts];
            for (var i = 0; i < Head.FrameCounts; i++)
            {
                dataoffset[i] = Utils.GetLittleEndianIntegerFromByteArray(buf, ref offset);
                datalength[i] = Utils.GetLittleEndianIntegerFromByteArray(buf, ref offset);
            }
            for (var j = 0; j < Head.FrameCounts; j++)
            {
                var datastart = dataoffset[j];
                var datalen = datalength[j];
                var width = Head.GlobleWidth;
                var height = Head.GlobleHeight;
                var data = new Color[width * height];
                var dataidx = 0;
                var dataend = datastart + datalen;
                while (datastart < dataend)
                {
                    var pixelcount = buf[datastart++];
                    var pixelalpha = buf[datastart++];
                    for (var k = 0; k < pixelcount; k++)
                    {
                        if (pixelalpha == 0)
                        {
                            data[dataidx++] = Color.Transparent;
                        }
                        else
                        {
                            data[dataidx++] = Palette[buf[datastart++]] * ((float)pixelalpha / (float)0xFF); 
                        }
                    }
                }
                var texture = new Texture2D(Globals.TheGame.GraphicsDevice, width, height);
                texture.SetData(data);
                Frames.Add(texture);
            }

            Palette = null;//palette can be released now
        }

        public Asf() { }

        public Asf(string path)
        {
            Load(path);
        }
    }
}
