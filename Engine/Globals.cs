using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine
{
    //Must load GlobalData when game is initialize
    public static class Globals
    {
        public const int SimpleChinaeseCode = 936;
        public const int BaseSpeed = 100;
        public const int MagicBasespeed = 60;
        public const float SoundMaxDistance = 1000f;
        public const float Sound3DMaxDistance = 8f;

        public const float DistanceOffset = 2f;

        public static Color NpcEdgeColor = Color.Yellow*0.6f;
        public static Color EnemyEdgeColor = Color.Red*0.8f;
        public static Color ObjEdgeColor = Color.Yellow*0.8f;

        public static float MusicVolume = 1f;
        public static float SoundEffectVolume = 1f;

        public static Random TheRandom = new Random();

        #region Sprite out edge
        public static Sprite OutEdgeSprite;
        public static Texture2D OutEdgeTexture;
        public static int OffX;
        public static int OffY;

        public static void ClearGlobalOutEdge()
        {
            Globals.OutEdgeSprite = null;
            Globals.OutEdgeTexture = null;
            Globals.OffX = Globals.OffY = 0;
        }
        #endregion

        public static Game TheGame;
        public static Carmera TheCarmera;
        public static Map TheMap = new Map();
        public static Dictionary<int, Asf> AsfFiles = new Dictionary<int, Asf>();
        public static Dictionary<int, Magic> Magics = new Dictionary<int, Magic>(); 
    }
}
