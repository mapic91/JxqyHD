using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Engine
{
    //Must load GlobalData when game is initialize
    public static class Globals
    {
        public const int SimpleChinaeseCode = 936;
        public const int Basespeed = 100;
        public const float SoundMaxDistance = 1000f;
        public static float MusicVolume = 1f;
        public static float SoundEffectVolume = 1f;
        public static Game TheGame;
        public static Carmera TheCarmera;
        public static Map TheMap = new Map();
        public static Dictionary<int, Asf> AsfFiles = new Dictionary<int, Asf>();
        public static Dictionary<int, Magic> Magics = new Dictionary<int, Magic>(); 
    }
}
