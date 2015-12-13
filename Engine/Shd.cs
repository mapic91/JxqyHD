using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine
{
    public class Shd : Mpc
    {
        private List<Color[]> _frameDatas = new List<Color[]>(); 

        protected override void LoadPalette(byte[] buf, ref int offset)
        {
            // Palette not existing in shd file
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
                var data = new Color[width * height];
                datastart += 8;
                var dataidx = 0;
                var dataend = datastart + datalen - 20;
                while (datastart < dataend)
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
                            data[dataidx++] = Color.Black * 0.6f;
                        }
                    }
                }
                _frameDatas.Add(data);
            }
        }

        public Shd(string path) : base(path)
        {

        }

        public override Texture2D GetFrame(int index)
        {
            //Use GetFrameData() instead
            return null;
        }

        public Color[] GetFrameData(int index)
        {
            if (index > 0 && index < _frameDatas.Count)
            {
                return _frameDatas[index];
            }
            return null;
        }
    }
}