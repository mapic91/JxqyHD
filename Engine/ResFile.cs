using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;

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
                var enumerables = File.ReadLines(path, Globals.SimpleChinaeseEncoding);
                var lines = enumerables as string[] ?? enumerables.ToArray();
                var counts = lines.Count();
                for (var i = 0; i < counts;)
                {
                    var state = GetState(lines[i++], type);
                    if (state != -1)
                    {
                        var stateInfo = GetStateInfo(lines[i++], lines[i++], type);
                        if(stateInfo.Image != null || stateInfo.Sound != null)
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
            list[(int) state].Image = Utils.GetAsf(GetAsfFilePathBase(fileName, ResType.Npc), fileName);
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

        private static ResStateInfo GetStateInfo(string image, string sound, ResType type)
        {
            var info = new ResStateInfo();
            var groups = Regex.Match(image, "Image=(.+)").Groups;
            if (groups[0].Success)
            {
                var asfFileName = groups[1].Value;
                var filePathBase = GetAsfFilePathBase(asfFileName, type);
                info.Image = Utils.GetAsf(filePathBase, asfFileName);
            }
            groups = Regex.Match(sound, "Sound=(.+)").Groups;
            if (groups[0].Success)
            {
                info.Sound = Utils.GetSoundEffect(groups[1].Value);
            }
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
                        case "[Stand]":
                            state = (int)CharacterState.Stand;
                            break;
                        case "[Stand1]":
                            state = (int)CharacterState.Stand1;
                            break;
                        case "[Walk]":
                            state = (int)CharacterState.Walk;
                            break;
                        case "[Run]":
                            state = (int)CharacterState.Run;
                            break;
                        case "[Jump]":
                            state = (int)CharacterState.Jump;
                            break;
                        case "[Attack]":
                            state = (int)CharacterState.Attack;
                            break;
                        case "[Attack1]":
                            state = (int)CharacterState.Attack1;
                            break;
                        case "[Attack2]":
                            state = (int)CharacterState.Attack2;
                            break;
                        case "[Magic]":
                            state = (int)CharacterState.Magic;
                            break;
                        case "[Sit]":
                            state = (int)CharacterState.Sit;
                            break;
                        case "[Hurt]":
                            state = (int)CharacterState.Hurt;
                            break;
                        case "[Death]":
                            state = (int)CharacterState.Death;
                            break;
                        case "[FightStand]":
                            state = (int)CharacterState.FightStand;
                            break;
                        case "[FightWalk]":
                            state = (int)CharacterState.FightWalk;
                            break;
                        case "[FightRun]":
                            state = (int)CharacterState.FightRun;
                            break;
                        case "[FightJump]":
                            state = (int)CharacterState.FightJump;
                            break;
                    }
                    break;
                case ResType.Obj:
                    switch (head)
                    {
                        case "[Common]":
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

    public class ResStateInfo
    {
        public Asf Image;
        public SoundEffect Sound;
    }
}
