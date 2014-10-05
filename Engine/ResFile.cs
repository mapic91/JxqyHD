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
                var enumerables = File.ReadLines(path, Encoding.GetEncoding(936));
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
                Log.LogMessageToFile("NpcRes file [" + path + "] read failed" + exception);
            }

            return info;
        }

        private static ResStateInfo GetStateInfo(string image, string sound, ResType type)
        {
            var info = new ResStateInfo();
            var groups = Regex.Match(image, "Image=(.+)").Groups;
            if (groups[0].Success)
            {
                var asfFileName = groups[1].Value;
                var asfPath = GetAsfFilePath(asfFileName, type);
                info.Image = Utils.GetAsf(asfPath);
            }
            groups = Regex.Match(sound, "Sound=(.+).wav").Groups;
            if (groups[0].Success)
            {
                var assertName = @"sound\" + groups[1].Value;
                info.Sound = Utils.GetSoundEffect(assertName);
            }
            return info;
        }

        private static string GetAsfFilePath(string asfFileName, ResType type)
        {
            string asfPath = string.Empty;
            switch (type)
            {
                case ResType.Npc:
                    asfPath = @"asf\character\" + asfFileName;
                    if (!File.Exists(asfPath))
                    {
                        asfPath = @"asf\interlude\" + asfFileName;
                        if (!File.Exists(asfPath)) asfPath = String.Empty;
                    }
                    break;
                case ResType.Obj:
                    asfPath = @"asf\object\" + asfFileName;
                    if (!File.Exists(asfPath)) asfPath = String.Empty;
                    break;
            }
            return asfPath;
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
                            state = (int)NpcState.Stand;
                            break;
                        case "[Stand1]":
                            state = (int)NpcState.Stand1;
                            break;
                        case "[Walk]":
                            state = (int)NpcState.Walk;
                            break;
                        case "[Run]":
                            state = (int)NpcState.Run;
                            break;
                        case "[Jump]":
                            state = (int)NpcState.Jump;
                            break;
                        case "[Attack]":
                            state = (int)NpcState.Attack;
                            break;
                        case "[Attack1]":
                            state = (int)NpcState.Attack1;
                            break;
                        case "[Attack2]":
                            state = (int)NpcState.Attack2;
                            break;
                        case "[Magic]":
                            state = (int)NpcState.Magic;
                            break;
                        case "[Sit]":
                            state = (int)NpcState.Sit;
                            break;
                        case "[Hurt]":
                            state = (int)NpcState.Hurt;
                            break;
                        case "[Death]":
                            state = (int)NpcState.Death;
                            break;
                        case "[FightStand]":
                            state = (int)NpcState.FightStand;
                            break;
                        case "[FightWalk]":
                            state = (int)NpcState.FightWalk;
                            break;
                        case "[FightRun]":
                            state = (int)NpcState.FightRun;
                            break;
                        case "[FightJump]":
                            state = (int)NpcState.FightJump;
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

    public enum NpcState
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

    public struct ResStateInfo
    {
        public Asf Image;
        public SoundEffect Sound;
    }
}
