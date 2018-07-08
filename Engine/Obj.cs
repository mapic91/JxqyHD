using System;
using System.Collections.Generic;
using System.IO;
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
        private string _scriptFile;
        private int _canInteractDirectly;
        private string _timerScriptFile;
        private int _timerScriptInterval = Globals.DefaultNpcObjTimeScriptInterval;
        private float _timerScriptIntervlElapsed;
        private SoundEffect _wavFileSoundEffect;
        private SoundEffectInstance _soundInstance;
        private string _wavFileName;
        private int _offX;
        private int _offY;

        #region Public properties

        public string FileName { protected set; get; }

        public bool IsRemoved { set; get; }

        public override Rectangle RegionInWorld
        {
            get
            {
                var region = base.RegionInWorld;
                return new Rectangle(region.X + OffX,
                    region.Y + OffY,
                    region.Width,
                    region.Height);
            }
        }

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

        public string ScriptFile
        {
            get { return _scriptFile; }
            set { _scriptFile = value; }
        }

        public int CanInteractDirectly
        {
            get { return _canInteractDirectly; }
            set { _canInteractDirectly = value; }
        }

        public string TimerScriptFile
        {
            get { return _timerScriptFile; }
            set { _timerScriptFile = value; }
        }

        public int TimerScriptInterval
        {
            get { return _timerScriptInterval; }
            set { _timerScriptInterval = value; }
        }

        public string WavFile
        {
            get { return _wavFileName; }
            protected set
            {
                _wavFileName = value;
                _wavFileSoundEffect = Utils.GetSoundEffect(value);
                if (_wavFileSoundEffect != null)
                {
                    _soundInstance = _wavFileSoundEffect.CreateInstance();
                }
                else
                {
                    _soundInstance = null;
                }
            }
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

        public override Vector2 ReginInWorldBeginPosition
        {
            get
            {
                return base.ReginInWorldBeginPosition + new Vector2(_offX, _offY);
            }
        }

        public bool IsObstacle
        {
            get
            {
                if (Kind == (int) ObjKind.Dynamic || 
                    Kind == (int) ObjKind.Static || 
                    Kind == (int) ObjKind.Door)
                {
                    return true;
                }
                return false;
            }
        }

        public bool IsDrop
        {
            get
            {
                return Kind == (int) ObjKind.Drop;
            }
        }

        public bool IsAutoPlay
        {
            get 
            { 
                return (Kind == (int)ObjKind.Dynamic || 
                        Kind == (int)ObjKind.Trap ||
                        Kind == (int)ObjKind.Drop); 
            }
        }

        public bool IsInteractive
        {
            get { return HasInteractScript; }
        }

        public bool HasInteractScript
        {
            get { return !string.IsNullOrEmpty(ScriptFile); }
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

        #region Ctor
        public Obj() { }

        public Obj(string filePath)
        {
            Load(filePath);
        }
        #endregion Ctor

        /// <summary>
        /// Apply 3D effect to suound
        /// </summary>
        private void UpdateSound()
        {
            if (_soundInstance != null)
            {
                SoundManager.Apply3D(_soundInstance,
                    PositionInWorld - Globals.ListenerPosition);
            }
        }

        private void PlaySound()
        {
            if (_soundInstance != null)
            {
                _soundInstance.Play();
            }
        }

        private void PlayRandSound()
        {
            if (_soundInstance == null) return;

            const int maxRandValue = 200;
            if (Globals.TheRandom.Next(0, maxRandValue) == 0)
            {
                PlaySound();
            }
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
                FileName = Path.GetFileName(filePath);
            }
            catch (Exception)
            {
                FileName = filePath;
            }

            try
            {
                var lines = File.ReadAllLines(filePath, Globals.LocalEncoding);
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
            WavFile = fileName;
        }

        public void InitializeFigure()
        {
            if (ObjFile != null && ObjFile.ContainsKey((int)ObjState.Common))
            {
                Texture = ObjFile[(int)ObjState.Common].Image;
            }
            CurrentDirection = Dir;
            CurrentFrameIndex += Frame;
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
                    case "WavFile":
                    case "TimerScriptFile":
                        info.SetValue(this, nameValue[1], null);
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
            PlayFrames(FrameEnd - CurrentFrameIndex);
        }

        public void CloseBox()
        {
            PlayFrames(CurrentFrameIndex - FrameBegin, true);
        }

        /// <summary>
        /// Set offset
        /// </summary>
        /// <param name="offSet">Offset value</param>
        public void SetOffSet(Vector2 offSet)
        {
            OffX = (int)offSet.X;
            OffY = (int)offSet.Y;
        }

        public void Save(KeyDataCollection keyDataCollection)
        {
            keyDataCollection.AddKey("ObjName", _objName);
            AddKey(keyDataCollection, "Kind", _kind);
            AddKey(keyDataCollection, "Dir", _dir);
            keyDataCollection.AddKey("MapX", MapX.ToString());
            keyDataCollection.AddKey("MapY", MapY.ToString());
            AddKey(keyDataCollection, "Damage", _damage);
            AddKey(keyDataCollection, "Frame", CurrentFrameIndex - FrameBegin);
            AddKey(keyDataCollection, "Height", _height);
            AddKey(keyDataCollection, "Lum", _lum);
            AddKey(keyDataCollection, "ObjFile", _objFileName);
            AddKey(keyDataCollection, "OffX", _offX);
            AddKey(keyDataCollection, "OffY", _offY);
            if (_scriptFile != null)
            {
                AddKey(keyDataCollection, "ScriptFile", _scriptFile);
            }
            if (_timerScriptFile != null)
            {
                AddKey(keyDataCollection, "TimerScriptFile", _timerScriptFile);
            }
            if (_wavFileSoundEffect != null)
            {
                AddKey(keyDataCollection, "WavFile", _wavFileName);
            }
            if (_timerScriptInterval != Globals.DefaultNpcObjTimeScriptInterval)
            {
                AddKey(keyDataCollection, "TimerScriptInterval", _timerScriptInterval);
            }
        }

        public void StartInteract()
        {
            if (!IsRemoved)
            {
                ScriptManager.RunScript(Utils.GetScriptParser(ScriptFile), this);
            }
        }

        private ScriptParser _timeScriptParserCache;
        public override void Update(GameTime gameTime)
        {
            if (!string.IsNullOrEmpty(_timerScriptFile))
            {
                _timerScriptIntervlElapsed += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                if (_timerScriptIntervlElapsed >= _timerScriptInterval)
                {
                    _timerScriptIntervlElapsed -= _timerScriptInterval;
                    if (_timeScriptParserCache == null ||
                            Globals.TheGame.IsInEditMode // Turn off cache script in edit mode
                       )
                    {
                        _timeScriptParserCache = Utils.GetScriptParser(_timerScriptFile);
                    }
                    ScriptManager.RunScript(_timeScriptParserCache, this);
                }
            }

            if ((Texture.FrameCounts > 1 && IsAutoPlay) ||
                IsInPlaying)
                base.Update(gameTime);

            switch ((ObjKind)Kind)
            {
                case ObjKind.LoopingSound:
                    UpdateSound();
                    PlaySound();
                    break;
                case ObjKind.RandSound:
                    UpdateSound();
                    PlayRandSound();
                    break;
                case ObjKind.Trap:
                    if (Damage > 0 && 
                        CurrentFrameIndex == FrameBegin)//Hurting fighter character at frame begin
                    {
                        //Npcs
                        var npcs = NpcManager.NpcList;
                        foreach (var npc in npcs)
                        {
                            if (npc.IsFighter && npc.MapX == MapX && npc.MapY == MapY)
                            {
                                npc.DecreaseLifeAddHurt(Damage);
                            }
                        }
                        //Player
                        if (Globals.ThePlayer.MapX == MapX && Globals.ThePlayer.MapY == MapY)
                        {
                            Globals.ThePlayer.DecreaseLifeAddHurt(Damage);
                        }
                    }
                    break;
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Draw(spriteBatch, OffX, OffY);
        }

        #region Enum type
        public enum ObjKind
        {
            Dynamic,
            Static,
            Body,
            LoopingSound,
            RandSound,
            Door,
            Trap,
            Drop
        }
        #endregion Enum type
    }
}
