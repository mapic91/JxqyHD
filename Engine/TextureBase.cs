using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine
{
    public abstract class TextureBase
    {
        private readonly List<Texture2D> _frames = new List<Texture2D>();
        private bool _isOk;
        private int _frameCountsPerDirection;

        protected FileHead Head;
        protected Color[] Palette;
        protected Game Game;

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
        public List<Texture2D> Frames
        {
            get { return _frames; }
        }
        public int FrameCounts
        {
            get { return _frames.Count; }
        }

        public int FrameCountsPerDirection
        {
            get { return _frameCountsPerDirection; }
        }

        #endregion Properties

        private void LoadTexture(string path)
        {
            try
            {
                var buf = File.ReadAllBytes(path);
                var offset = 0;
                if (!LoadHead(buf, ref offset)) return;
                LoadPalette(buf, ref offset);
                LoadFrame(buf, ref offset);
                if (Head.Direction != 0)
                    _frameCountsPerDirection = FrameCounts/DirectionCounts;
                else _frameCountsPerDirection = FrameCounts;
                _isOk = true;
            }
            catch (Exception e)
            {
                Log.LogMessageToFile(string.Format("{0}load failed:{1}", path, e));
            }
        }
        private void LoadPalette(byte[] buf, ref int offset)
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

        public void Load(Game game, string path)
        {
            this.Game = game;
            LoadTexture(path);
        }
        
        protected abstract bool LoadHead(byte[] buf, ref int offset);
        protected abstract void LoadFrame(byte[] buf, ref int offset);

        public Texture2D GetFrame(int index)
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
