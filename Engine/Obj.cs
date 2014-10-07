using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Audio;

namespace Engine
{
    using StateMapList = Dictionary<int, ResStateInfo>;
    public class Obj
    {
        private string _objName;
        private int _kind;
        private int _dir;
        private int _damage;
        private int _frame;
        private int _height;
        private int _lum;
        private int _mapX;
        private int _mapY;
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
            set { _dir = value%8; }
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

        public int MapX
        {
            get { return _mapX; }
            set { _mapX = value; }
        }

        public int MapY
        {
            get { return _mapY; }
            set { _mapY = value; }
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

        #endregion

        public Obj(string filePath)
        {
            Load(filePath);
        }

        public bool Load(string filePath)
        {
            try
            {
                var lines = File.ReadAllLines(filePath, Encoding.GetEncoding(936));
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
            return true;
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
    }
}
