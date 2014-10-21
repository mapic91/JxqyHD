using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace Engine
{
    using StateMapList = Dictionary<int, ResStateInfo>;
    public class Obj: Sprite
    {
        private string _objName;
        private int _kind;
        private int _dir;
        private int _damage;
        private int _frame;
        private int _height;
        private int _lum;
        private StateMapList _objFile;
        private string _scriptFile;
        private SoundEffect _wavFile;
        private int _offX;
        private int _offY;

        #region Public properties

        public string ObjName
        {
            get { return _objName; }
            set { _objName = value; }
        }

        public int Kind
        {
            get { return _kind; }
            set { _kind = value; }
        }

        public int Dir
        {
            get { return _dir; }
            set { _dir = value; }
        }

        public int Damage
        {
            get { return _damage; }
            set { _damage = value; }
        }

        public int Frame
        {
            get { return _frame; }
            set { _frame = value; }
        }

        public int Height
        {
            get { return _height; }
            set { _height = value; }
        }

        public int Lum
        {
            get { return _lum; }
            set { _lum = value; }
        }

        public StateMapList ObjFile
        {
            get { return _objFile; }
            set { _objFile = value; }
        }

        public string ScriptFile
        {
            get { return _scriptFile; }
            set { _scriptFile = value; }
        }

        public SoundEffect WavFile
        {
            get { return _wavFile; }
            set { _wavFile = value; }
        }

        public int OffX
        {
            get { return _offX; }
            set { _offX = value; }
        }

        public int OffY
        {
            get { return _offY; }
            set { _offY = value; }
        }

        public bool IsObstacle
        {
            get
            {
                if (Kind == 0 || Kind == 1 || Kind == 5)
                    return true;
                return false;
            }
        }

        public bool IsAutoPlay
        {
            get { return (Kind == 0 || Kind == 6); }
        }

        public bool IsInteractive
        {
            get { return !string.IsNullOrEmpty(ScriptFile); }
        }

        public bool IsTrap
        {
            get { return Kind == 6; }
        }
        #endregion

        public Obj() { }

        public Obj(string filePath)
        {
            Load(filePath);
        }

        public bool Load(string filePath)
        {
            try
            {
                var lines = File.ReadAllLines(filePath, Encoding.GetEncoding(Globals.SimpleChinaeseCode));
                return Load(lines);
            }
            catch (Exception exception)
            {
                Log.LogMessageToFile("Obj load failed [" + filePath + "]." + exception);
                return false;
            }
        }

        public bool Load(string[] lines)
        {
            foreach (var line in lines)
            {
                var nameValue = Utils.GetNameValue(line);
                if (!string.IsNullOrEmpty(nameValue[0]))
                    AssignToValue(nameValue);
            }
            InitializeFigure();
            return true;
        }

        public void InitializeFigure()
        {
            if (ObjFile.ContainsKey((int) ObjState.Common))
            {
                Texture = ObjFile[(int)ObjState.Common].Image;
            }
            CurrentDirection = Dir;
            CurrentFrameIndex = Frame;
        }

        private void AssignToValue(string[] nameValue)
        {
            try
            {
                var info = this.GetType().GetProperty(nameValue[0]);
                switch (nameValue[0])
                {
                    case "ObjName":
                    case "ScriptFile":
                        info.SetValue(this, nameValue[1], null);
                        break;
                    case "WavFile":
                        info.SetValue(this, Utils.GetSoundEffect(nameValue[1]), null);
                        break;
                    case "ObjFile":
                        info.SetValue(this, ResFile.ReadFile(@"ini\objres\" + nameValue[1], ResType.Obj), null);
                        break;
                    default:
                        var integerValue = int.Parse(nameValue[1]);
                        info.SetValue(this, integerValue, null);
                        break;
                }
            }
            catch (Exception)
            {
                //Do nothing
                return;
            }
        }

        public void Update(GameTime gameTime)
        {
            if(Texture.FrameCounts > 1 && IsAutoPlay)
                base.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Draw(spriteBatch, OffX, OffY);
        }
    }
}
