using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using Engine.Script;
using IniParser.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace Engine
{
    using StateMapList = Dictionary<int, ResStateInfo>;
    public class Obj : Sprite
    {
        private string _objName;
        private int _kind;
        private int _dir;
        private int _damage;
        private int _frame;
        private int _height;
        private int _lum;
        private StateMapList _objFile;
        private string _objFileName;
        private ScriptParser _scriptFile;
        private SoundEffect _wavFile;
        private string _wavFileName;
        private int _offX;
        private int _offY;

        #region Public properties

        public string FileName { protected set; get; }

        public bool IsRemoved { set; get; }

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

        public new int Height
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
            protected set { _objFile = value; }
        }

        public ScriptParser ScriptFile
        {
            get { return _scriptFile; }
            set { _scriptFile = value; }
        }

        public SoundEffect WavFile
        {
            get { return _wavFile; }
            protected set { _wavFile = value; }
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
            get { return ScriptFile != null; }
        }

        public bool IsTrap
        {
            get { return Kind == 6; }
        }

        public bool IsBody
        {
            get { return Kind == 2; }
        }
        #endregion

        public Obj() { }

        public Obj(string filePath)
        {
            try
            {
                FileName = Path.GetFileName(filePath);
            }
            catch (Exception)
            {
                FileName = filePath;
            }
            Load(filePath);
        }

        protected void AddKey(KeyDataCollection keyDataCollection, string key, int value)
        {
            keyDataCollection.AddKey(key, value.ToString());
        }

        protected void AddKey(KeyDataCollection keyDataCollection, string key, string value)
        {
            keyDataCollection.AddKey(key, value);
        }

        public bool Load(string filePath)
        {
            try
            {
                var lines = File.ReadAllLines(filePath, Globals.SimpleChinaeseEncoding);
                return Load(lines);
            }
            catch (Exception exception)
            {
                Log.LogFileLoadError("Obj", filePath, exception);
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

        public void SetObjFile(string fileName)
        {
            _objFileName = fileName;
            _objFile = ResFile.ReadFile(@"ini\objres\" + fileName, ResType.Obj);
        }

        public void SetWaveFile(string fileName)
        {
            _wavFileName = fileName;
            _wavFile = Utils.GetSoundEffect(fileName);
        }

        public void InitializeFigure()
        {
            if (ObjFile.ContainsKey((int)ObjState.Common))
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
                        info.SetValue(this, nameValue[1], null);
                        break;
                    case "ScriptFile":
                        if (!string.IsNullOrEmpty(nameValue[1]))
                            info.SetValue(this,
                                new ScriptParser(Utils.GetScriptFilePath(nameValue[1]), this),
                                null);
                        break;
                    case "WavFile":
                        SetWaveFile(nameValue[1]);
                        break;
                    case "ObjFile":
                        SetObjFile(nameValue[1]);
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

        public void OpenBox()
        {
            PlayCurrentDirOnce();
        }

        public override void Update(GameTime gameTime)
        {
            if ((Texture.FrameCounts > 1 && IsAutoPlay) ||
                IsPlayingCurrentDirOnce)
                base.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Draw(spriteBatch, OffX, OffY);
        }

        public void Save(KeyDataCollection keyDataCollection)
        {
            keyDataCollection.AddKey("ObjName", _objName);
            AddKey(keyDataCollection, "Kind", _kind);
            AddKey(keyDataCollection, "Dir", _dir);
            keyDataCollection.AddKey("MapX", MapX.ToString());
            keyDataCollection.AddKey("MapY", MapY.ToString());
            AddKey(keyDataCollection, "Damage", _damage);
            AddKey(keyDataCollection, "Frame", _frame);
            AddKey(keyDataCollection, "Height", _height);
            AddKey(keyDataCollection, "Lum", _lum);
            AddKey(keyDataCollection, "ObjFile", _objFileName);
            AddKey(keyDataCollection, "OffX", _offX);
            AddKey(keyDataCollection, "OffY", _offY);
            if (_scriptFile != null)
            {
                AddKey(keyDataCollection, "ScriptFile", _scriptFile.FileName);
            }
            if (_wavFile != null)
            {
                AddKey(keyDataCollection, "WavFile", _wavFileName);
            }
        }
    }
}
