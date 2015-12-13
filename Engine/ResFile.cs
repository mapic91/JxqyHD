using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using IniParser;
using Microsoft.Xna.Framework.Audio;

namespace Engine
{
    //NPC Resource file reader
    static class ResFile
    {
        public static Dictionary<int, ResStateInfo> ReadFile(string path, ResType type)
        {
            var info = new Dictionary<int, ResStateInfo>();
            try
            {
                var data = new FileIniDataParser().ReadFile(path, Globals.LocalEncoding);
                foreach (var section in data.Sections)
                {
                    var state = GetState(section.SectionName, type);
                    if (state != -1)
                    {
                        var keys = section.Keys;
                        var stateInfo = GetStateInfo(keys["Image"], keys["Shade"], keys["Sound"], type);
                        info[state] = stateInfo;
                    }
                }
            }
            catch (Exception exception)
            {
                Log.LogFileLoadError("NpcRes", path, exception);
            }

            return info;
        }

        public static void SetNpcStateImage(Dictionary<int, ResStateInfo> list, CharacterState state, string fileName)
        {
            if(list == null) return;
            if (!list.ContainsKey((int) state))
            {
                list[(int)state] = new ResStateInfo();
            }
            list[(int) state].SetImageFilePath(ImageType.ASF, GetAsfFilePathBase(fileName, ResType.Npc), fileName, null);
        }

        public static string GetAsfFilePathBase(string asfFileName, ResType type)
        {
            string asfPathBase = string.Empty;
            switch (type)
            {
                case ResType.Npc:
                    asfPathBase = @"asf\character\";
                    if (!File.Exists(asfPathBase + asfFileName))
                    {
                        asfPathBase = @"asf\interlude\";
                    }
                    break;
                case ResType.Obj:
                    asfPathBase = @"asf\object\";
                    break;
            }
            return asfPathBase;
        }

        public static string GetMpcFilePathBase(string fileName, ResType type)
        {
            string pathBase = string.Empty;
            switch (type)
            {
                case ResType.Npc:
                    return @"mpc\character\";
                    break;
                case ResType.Obj:
                    return @"mpc\object\";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        private static ResStateInfo GetStateInfo(string image, string shd, string sound, ResType type)
        {
            var info = new ResStateInfo();

            var ext = Path.GetExtension(image);
            ext = string.IsNullOrEmpty(ext) ? "" : ext;
            ext = ext.ToLower();
            switch (ext)
            {
                case ".asf":
                    info.SetImageFilePath(ImageType.ASF, GetAsfFilePathBase(image, type), image, shd);
                    break;
                case ".mpc":
                    info.SetImageFilePath(ImageType.MPC, GetMpcFilePathBase(image, type), image, shd);
                    break;
            }
            info.SetSound(sound);
            return info;
        }

        private static int GetState(string head, ResType type)
        {
            int state = -1;
            switch (type)
            {
                case ResType.Npc:
                    switch (head)
                    {
                        case "Stand":
                            state = (int) CharacterState.Stand;
                            break;
                        case "Stand1":
                            state = (int) CharacterState.Stand1;
                            break;
                        case "Walk":
                            state = (int) CharacterState.Walk;
                            break;
                        case "Run":
                            state = (int) CharacterState.Run;
                            break;
                        case "Jump":
                            state = (int) CharacterState.Jump;
                            break;
                        case "Attack":
                            state = (int) CharacterState.Attack;
                            break;
                        case "Attack1":
                            state = (int) CharacterState.Attack1;
                            break;
                        case "Attack2":
                            state = (int) CharacterState.Attack2;
                            break;
                        case "Magic":
                            state = (int) CharacterState.Magic;
                            break;
                        case "Sit":
                            state = (int) CharacterState.Sit;
                            break;
                        case "Hurt":
                            state = (int) CharacterState.Hurt;
                            break;
                        case "Death":
                            state = (int) CharacterState.Death;
                            break;
                        case "FightStand":
                            state = (int) CharacterState.FightStand;
                            break;
                        case "FightWalk":
                            state = (int) CharacterState.FightWalk;
                            break;
                        case "FightRun":
                            state = (int) CharacterState.FightRun;
                            break;
                        case "FightJump":
                            state = (int) CharacterState.FightJump;
                            break;
                    }
                    break;
                case ResType.Obj:
                    switch (head)
                    {
                        case "Common":
                            state = (int) ObjState.Common;
                            break;
                    }
                    break;
            }
            return state;
        }
    }

    public enum ResType
    {
        Npc,
        Obj
    }

    public enum CharacterState
    {
        Stand,
        Stand1,
        Walk,
        Run,
        Jump,
        Attack,
        Attack1,
        Attack2,
        Magic,
        Sit,
        Hurt,
        Death,
        FightStand,
        FightWalk,
        FightRun,
        FightJump,
    }

    public enum ObjState
    {
        Common
    }

    public enum ImageType
    {
        ASF,
        MPC
    }

    public class ResStateInfo
    {
        private string _basePath;
        private string _fileName;
        private string _shdFileName;
        private TextureBase _image;
        private ImageType _imageType;

        private string _soundFileName;
        private SoundEffect _soundEffect;

        public void SetImageFilePath(ImageType type, string basePath, string fileName, string shdFileName)
        {
            _imageType = type;
            _basePath = basePath;
            _fileName = fileName;
            _shdFileName = shdFileName;
            _image = null;
        }

        public void SetSound(string fileName)
        {
            _soundFileName = fileName;
            _soundEffect = null;
        }

        public TextureBase Image
        {
            get
            {
                if (_image == null)
                {
                    switch (_imageType)
                    {
                        case ImageType.ASF:
                            _image = Utils.GetAsf(_basePath, _fileName);
                            break;
                        case ImageType.MPC:
                            _image = Utils.GetMpc(_basePath, _fileName, _shdFileName);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                return _image;
            }
        }

        public SoundEffect Sound
        {
            get
            {
                if (_soundEffect == null)
                {
                    _soundEffect = Utils.GetSoundEffect(_soundFileName);
                }
                return _soundEffect;
            }
        }
    }
}
