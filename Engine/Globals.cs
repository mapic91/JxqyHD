using System;
using System.Text;
using Engine.Message;
using IniParser;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine
{
    //Must load GlobalData when game is initialize
    public static class Globals
    {
        public static readonly Encoding SimpleChineseEncoding = Encoding.GetEncoding(936);
        public const int BaseSpeed = 100;
        public const int MagicBasespeed = 100;
        public const float SoundMaxDistance = 1000f;
        public const float Sound3DMaxDistance = 8f;

        public const float DistanceOffset = 2f;

        public static Color NpcEdgeColor = Color.Yellow*0.6f;
        public static Color FriendEdgeColor = Color.Green*0.6f;
        public static Color EnemyEdgeColor = Color.Red*0.6f;
        public static Color ObjEdgeColor = Color.Yellow*0.6f;

        public static Vector2 ListenerPosition
        {
            get { return ThePlayer.PositionInWorld; }
        }

        public static readonly Random TheRandom = new Random();

        #region Sprite out edge
        public static Npc OutEdgeNpc;
        public static Obj OutEdgeObj;
        public static Sprite OutEdgeSprite;
        public static Texture2D OutEdgeTexture;
        public static int OffX;
        public static int OffY;

        public static void ClearGlobalOutEdge()
        {
            OutEdgeNpc = null;
            OutEdgeObj = null;
            Globals.OutEdgeSprite = null;
            Globals.OutEdgeTexture = null;
            Globals.OffX = Globals.OffY = 0;
        }
        #endregion

        public static JxqyGame TheGame;
        public static readonly Carmera TheCarmera = new Carmera();
        public static readonly Map TheMap = new Map();
        public static Player ThePlayer;
        public static int PlayerIndex;

        public static bool IsInSuperMagicMode;
        public static MagicSprite SuperModeMagicSprite;

        #region Font
        public static SpriteFont FontSize7;
        public static SpriteFont FontSize10;
        public static SpriteFont FontSize12;
        #endregion

        public static int WindowWidth = 1366;
        public static int WindowHeight = 768;
        public static bool IsFullScreen = true;
        public static bool IsLogOn;
        public static bool ShowScriptWindow;

        public static bool IsInputDisabled;
        public static bool IsWaterEffectEnabled;

        public static readonly MessageDelegater TheMessageSender = new MessageDelegater();

        public static void Initialize()
        {
            try
            {
                var parser = new FileIniDataParser();
                var data = parser.ReadFile("Jxqy.ini");
                var setting = data["Setting"];
                int value;
                if (int.TryParse(setting["FullScreen"], out value))
                    IsFullScreen = (value == 1);
                if (int.TryParse(setting["Width"], out value))
                    WindowWidth = value;
                if (int.TryParse(setting["Height"], out value))
                    WindowHeight = value;
                if (int.TryParse(setting["Log"], out value))
                    IsLogOn = (value == 1);
                if (int.TryParse(setting["ShowScriptWindow"], out value))
                    ShowScriptWindow = (value == 1);
            }
            catch (Exception)
            {
                //no setting file, do nothing
            }
        }

        private static bool _lastIsInputDisabled;
        /// <summary>
        /// Temporary enable input.use RestoreInputDisableState() to restore.
        /// </summary>
        public static void EnableInputTemporary()
        {
            _lastIsInputDisabled = IsInputDisabled;
            IsInputDisabled = false;
        }

        /// <summary>
        /// Restore to last saved input disable state.
        /// </summary>
        public static void RestoreInputDisableState()
        {
            IsInputDisabled = _lastIsInputDisabled;
        }
    }
}
