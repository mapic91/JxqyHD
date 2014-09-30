using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace Engine
{
    //NPC Resource file reader
    static class NpcRes
    {
        public static Dictionary<NpcState, NpcStateInfo> ReadFile(Game game,  string path)
        {
            var info = new Dictionary<NpcState, NpcStateInfo>();
            try
            {
                var enumerables = File.ReadLines(path, Encoding.GetEncoding(936));
                var lines = enumerables as string[] ?? enumerables.ToArray();
                var counts = lines.Count();
                for (var i = 0; i < counts;)
                {
                    var state = GetState(lines[i++]);
                    if (state != NpcState.NpcStateEnd)
                    {
                        var stateInfo = GetStateInfo(lines[i++], lines[i++]);
                        if(stateInfo.Image != null)
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

        private static NpcStateInfo GetStateInfo(string image, string sound)
        {
            var info = new NpcStateInfo();
            var groups = Regex.Match(image, "Image=(.+)").Groups;
            if (groups[0].Success)
            {
                var asfFileName = groups[1].Value;
                var asfPath = GetAsfFilePath(asfFileName);
                var asf = new Asf(asfPath);
                info.Image = asf.IsOk ? asf : null;
            }
            groups = Regex.Match(sound, "Sound=(.+)").Groups;
            if (groups[0].Success)
            {
                var soundPath = @"sound\" + groups[1].Value;
                if (File.Exists(soundPath))
                {
                    using (var stream = File.OpenRead(soundPath))
                    {
                        info.Sound = SoundEffect.FromStream(stream);
                    }
                }
            }
            return info;
        }

        private static string GetAsfFilePath(string asfFileName)
        {
            var asfPath = @"asf\character\" + asfFileName;
            if (!File.Exists(asfPath))
            {
                asfPath = @"asf\interlude\" + asfFileName;
                if(!File.Exists(asfPath)) asfPath = String.Empty;
            }
            return asfPath;
        }

        static public NpcState GetState(string head)
        {
            var state = NpcState.NpcStateEnd;
            switch (head)
            {
                case "[Stand]":
                    state = NpcState.Stand;
                    break;
                case "[Stand1]":
                    state = NpcState.Stand1;
                    break;
                case "[Walk]":
                    state = NpcState.Walk;
                    break;
                case "[Run]":
                    state = NpcState.Run;
                    break;
                case "[Jump]":
                    state = NpcState.Jump;
                    break;
                case "[Attack]":
                    state = NpcState.Attack;
                    break;
                case "[Attack1]":
                    state = NpcState.Attack1;
                    break;
                case "[Attack2]":
                    state = NpcState.Attack2;
                    break;
                case "[Magic]":
                    state = NpcState.Magic;
                    break;
                case "[Sit]":
                    state = NpcState.Sit;
                    break;
                case "[Hurt]":
                    state = NpcState.Hurt;
                    break;
                case "[Death]":
                    state = NpcState.Death;
                    break;
                case "[FightStand]":
                    state = NpcState.FightStand;
                    break;
                case "[FightWalk]":
                    state = NpcState.FightWalk;
                    break;
                case "[FightRun]":
                    state = NpcState.FightRun;
                    break;
                case "[FightJump]":
                    state = NpcState.FightJump;
                    break;
            }
            return state;
        }
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
        NpcStateEnd
    }

    public struct NpcStateInfo
    {
        public Asf Image;
        public SoundEffect Sound;
    }
}
