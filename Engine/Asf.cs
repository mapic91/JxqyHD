using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine
{
    public class Asf : TextureBase
    {
        private byte[] _fileBuffer;
        private int[] _dataoffset;
        private int[] _datalength;
        private bool _allCached;
        private void CheckALlFrameCached()
        {
            for (var i = 0; i < FrameCounts; i++)
            {
                if (Frames[i] == null) return;
            }
            _allCached = true;

            //free resurce
            Palette = null;
            _fileBuffer = null;
            _dataoffset = null;
            _datalength = null;
        }

        private Texture2D DecodeFrame(int index)
        {
            if (index < 0 || index >= FrameCounts) return null;
            //If is already decoded retun cached frame
            if (Frames[index] != null) return Frames[index];
            try
            {
                var datastart = _dataoffset[index];
                var datalen = _datalength[index];
                var width = Head.GlobleWidth;
                var height = Head.GlobleHeight;
                var data = new Color[width * height];
                var dataidx = 0;
                var dataend = datastart + datalen;
                while (datastart < dataend)
                {
                    var pixelcount = _fileBuffer[datastart++];
                    var pixelalpha = _fileBuffer[datastart++];
                    for (var k = 0; k < pixelcount; k++)
                    {
                        if (pixelalpha == 0)
                        {
                            data[dataidx++] = Color.Transparent;
                        }
                        else
                        {
                            data[dataidx++] = Palette[_fileBuffer[datastart++]] * ((float)pixelalpha / (float)0xFF);
                        }
                    }
                }
                var texture = new Texture2D(Globals.TheGame.GraphicsDevice, width, height);
                texture.SetData(data);
                return texture;
            }
            catch (Exception)
            {
                //file corrupt
                return null;
            }
        }

        protected override bool LoadHead(byte[] buf, ref int offset)
        {
            var headinfo = Globals.SimpleChineseEncoding.GetString(buf, 0, "ASF 1.0".Length);
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
            return base.LoadHead(buf, ref offset);
        }
        protected override void LoadFrame(byte[] buf, ref int offset)
        {
            _fileBuffer = buf;
            _dataoffset = new int[Head.FrameCounts];
            _datalength = new int[Head.FrameCounts];
            for (var i = 0; i < Head.FrameCounts; i++)
            {
                _dataoffset[i] = Utils.GetLittleEndianIntegerFromByteArray(buf, ref offset);
                _datalength[i] = Utils.GetLittleEndianIntegerFromByteArray(buf, ref offset);
            }
        }

        public override Texture2D GetFrame(int index)
        {
            if (index < 0 || index >= FrameCounts) return null;
            if (_allCached) return Frames[index];

            var texture = DecodeFrame(index);
            if (texture != null)
            {
                Frames[index] = texture;
                CheckALlFrameCached();
            }
            return texture;
        }

        public Asf() { }

        public Asf(string path)
        {
            _allCached = false;
            Load(path);
        }

        public Asf(Texture2D texture)
        {
            if (texture == null) return;
            Frames = new Texture2D[] { texture };
            Head.GlobleWidth = texture.Width;
            Head.GlobleHeight = texture.Height;
            Head.Direction = 1;
            Head.FrameCounts = 1;
            _allCached = true;
            _isOk = true;
        }
    }
}
