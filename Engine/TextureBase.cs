using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine
{
    public abstract class TextureBase
    {
        private string _filePath;
        private Texture2D[] _frames;
        private int _frameCountsPerDirection = 1;

        protected bool _isOk;
        protected FileHead Head;
        protected Color[] Palette;

        #region Properties
        public bool IsOk
        {
            get { return _isOk; }
        }
        public int Width
        {
            get { return Head.GlobleWidth; }
        }
        public int Height
        {
            get { return Head.GlobleHeight; }
        }
        public int Interval
        {
            get { return Head.Interval; }
        }
        public int Bottom
        {
            get { return Head.Bottom; }
        }
        public int Left
        {
            get { return Head.Left; }
        }

        public int DirectionCounts
        {
            get { return Head.Direction; }
        }
        public Texture2D[] Frames
        {
            get { return _frames; }
            protected set { _frames = value; }
        }
        public int FrameCounts
        {
            get { return Head.FrameCounts; }
        }

        public int FrameCountsPerDirection
        {
            get { return _frameCountsPerDirection; }
            private set { _frameCountsPerDirection = value < 1 ? 1 : value; }
        }

        public string FilePath
        {
            get { return _filePath; }
            protected set { _filePath = value; }
        }

        #endregion Properties

        private void LoadTexture(string path)
        {
            try
            {
                FilePath = path;
                var buf = File.ReadAllBytes(path);
                var offset = 0;
                if (!LoadHead(buf, ref offset))
                {
                    Head.FrameCounts = 0;
                    return;
                }
                LoadPalette(buf, ref offset);
                LoadFrame(buf, ref offset);
                if (Head.FrameCounts < Head.Direction)
                {
                    Head.Direction = 1;
                }
                if (Head.Direction != 0)
                    FrameCountsPerDirection = FrameCounts / DirectionCounts;
                else FrameCountsPerDirection = FrameCounts;
                _isOk = true;
            }
            catch (Exception e)
            {
                Head.FrameCounts = 0;
                Log.LogFileLoadError("Texture", path, e);
            }
        }
        protected virtual void LoadPalette(byte[] buf, ref int offset)
        {
            Palette = new Color[Head.ColourCounts];
            for (var i = 0; i < Head.ColourCounts; i++)
            {
                Palette[i].B = buf[offset++];
                Palette[i].G = buf[offset++];
                Palette[i].R = buf[offset++];
                Palette[i].A = 0xFF;
                offset++;
            }
        }

        public void Load(string path)
        {
            LoadTexture(path);
        }

        protected virtual bool LoadHead(byte[] buf, ref int offset)
        {
            Frames = new Texture2D[Head.FrameCounts];
            return true;
        }
        protected abstract void LoadFrame(byte[] buf, ref int offset);

        public virtual Texture2D GetFrame(int index)
        {
            if (index >= 0 && index < FrameCounts)
                return Frames[index];
            return null;
        }

        protected struct FileHead
        {
            public int FramesDataLengthSum;
            public int GlobleWidth;
            public int GlobleHeight;
            public int FrameCounts;
            public int Direction;
            public int ColourCounts;
            public int Interval;
            public int Bottom;
            public int Left;
        }
    }
}
