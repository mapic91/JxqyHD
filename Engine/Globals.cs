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
        public static Game TheGame;
        public static Carmera TheCarmera;
        public static Map TheMap = new Map();
        public static Dictionary<int, Asf> AsfFiles = new Dictionary<int, Asf>();
    }
}
