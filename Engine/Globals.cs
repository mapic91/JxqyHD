using System;
using System.IO;
using System.Linq;
using System.Text;
using Engine.Gui;
using Engine.Message;
using IniParser;
using IniParser.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace Engine
{
    //Must load GlobalData when game is initialize
    public static class Globals
    {
        public static readonly Encoding LocalEncoding = Encoding.GetEncoding(936);
        public const int BaseSpeed = 100;
        public const int MinChangeMoveSpeedPercent = -90;
        public const int MagicBasespeed = 100;
        public const float SoundMaxDistance = 1000f;
        public const float Sound3DMaxDistance = 8f;
        public static int RunSpeedFold = 8;

        public const float DistanceOffset = 2f;

        // Out edge color
        public static Color NpcEdgeColor = Color.Yellow*0.6f;
        public static Color FriendEdgeColor = Color.Green*0.6f;
        public static Color EnemyEdgeColor = Color.Red*0.6f;
        public static Color NeturalEdgeColor = Color.Blue * 0.6f;
        public static Color ObjEdgeColor = Color.Yellow*0.6f;

        // NPC OBJ
        public static int DefaultNpcObjTimeScriptInterval = 1000;

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
        public static Color OutEdgeColor;
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
        public static int PlayerIndex;
        public static Player ThePlayer;

        public static Character PlayerKindCharacter
        {
            get
            {
                var charcter = NpcManager.GetPlayerKindCharacter();
                return charcter ?? (ThePlayer.ControledCharacter ?? ThePlayer);
            }
        }

        public static Vector2 PlayerPositionInWorld
        {
            get
            {
                var character = PlayerKindCharacter;
                return character == null ? Vector2.Zero : character.PositionInWorld;
            }
        }

        public static Vector2 PlayerTilePosition
        {
            get
            {
                var character = PlayerKindCharacter;
                return character == null ? Vector2.Zero : character.TilePosition;
            }
        }

        public static bool IsInSuperMagicMode;
        public static MagicSprite SuperModeMagicSprite;

        #region Font
        public static SpriteFont FontSize7;
        public static SpriteFont FontSize10;
        public static SpriteFont FontSize12;
        #endregion

        public const string GameIniFilePath = "Jxqy.ini";
        public const string SettingSectionName = "Setting";
        public static int WindowWidth = 800;
        public static int WindowHeight = 600;
        public static bool IsFullScreen = true;
        public static int SaveLoadSelectionIndex;
        public static bool IsUseThewWhenNormalRun = false;

        private static bool _isInputDisabled;
        private static bool _isInputDisabledDirty;

        public static bool IsInputDisabled
        {
            set
            {
                _isInputDisabled = value;
                _isInputDisabledDirty = true;
            }
            get { return _isInputDisabled; }
        }

        /// <summary>
        /// Is save game disabed.
        /// </summary>
        public static bool IsSaveDisabled { set; get; }

        public static bool IsDropGoodWhenDefeatEnemyDisabled { set; get; }

   
        public static bool IsWaterEffectEnabled;

        public static readonly MessageDelegater TheMessageSender = new MessageDelegater();

        public static void LoadSetting()
        {
            try
            {
                var mode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
                WindowWidth = mode.Width;
                WindowHeight = mode.Height;

                var parser = new FileIniDataParser();
                var data = parser.ReadFile(GameIniFilePath, LocalEncoding);
                var setting = data[SettingSectionName];
                int value;
                if (int.TryParse(setting["FullScreen"], out value))
                    IsFullScreen = (value != 0);
                if (int.TryParse(setting["Width"], out value))
                    WindowWidth = value;
                if (int.TryParse(setting["Height"], out value))
                    WindowHeight = value;
                if (int.TryParse(setting["SaveLoadSelectionIndex"], out value))
                    SaveLoadSelectionIndex = value;
                if (int.TryParse(setting["IsUseThewWhenNormalRun"], out value))
                    IsUseThewWhenNormalRun = value > 0;
                if (int.TryParse(setting["MaxMagicUnit"], out value))
                    MagicManager.MaxMagicUnit = value;
                if (int.TryParse(setting["RunSpeedFold"], out value))
                    RunSpeedFold = value;

                float fv;
                if (float.TryParse(setting["SoundEffectVolume"], out fv))
                    SoundEffect.MasterVolume = fv;
                if(float.TryParse(setting["MusicVolume"], out fv))
                    BackgroundMusic.SetVolume(fv);
            }
            catch (Exception)
            {
                //no setting file, do nothing
            }
        }

        public static void SaveSetting()
        {
            SaveSetting(false);
        }

        public static void SaveAllSetting()
        {
            SaveSetting(true);
        }

        /// <summary>
        /// Save game settings to ini file.
        /// Because some settings was not needed to save when playing game.
        /// Set isAll to flase will only save update require settings when playing game.
        /// Set isAll to thre if is in setting game settings.
        /// </summary>
        /// <param name="isAll">Save all settings to file if true.</param>
        private static void SaveSetting(bool isAll)
        {
            try
            {
                IniData data;
                if (!File.Exists(GameIniFilePath))
                {
                    data = new IniData();
                }
                else
                {
                    data = new FileIniDataParser().ReadFile(GameIniFilePath, LocalEncoding);
                }
                var section = data[SettingSectionName];
                if (section == null)
                {
                    data.Sections.AddSection(SettingSectionName);
                    section = data[SettingSectionName];
                }

                section["FullScreen"] = IsFullScreen ? "1" : "0";
                section["SaveLoadSelectionIndex"] = SaveLoadSelectionIndex.ToString();
                section["IsUseThewWhenNormalRun"] = IsUseThewWhenNormalRun ? "1" : "0";
                if (isAll)
                {
                    section["Width"] = WindowWidth.ToString();
                    section["Height"] = WindowHeight.ToString();
                    section["SoundEffectVolume"] = SoundEffect.MasterVolume.ToString();
                    section["MusicVolume"] = BackgroundMusic.GetVolume().ToString();
                    section["MaxMagicUnit"] = MagicManager.MaxMagicUnit.ToString();
                    section["RunSpeedFold"] = RunSpeedFold.ToString();
                }
                File.WriteAllText(GameIniFilePath, data.ToString(), LocalEncoding);
            }
            catch (Exception exception)
            {
                Log.LogFileSaveError("Game setting file", GameIniFilePath, exception);
            }
        }

        private static bool _lastIsInputDisabled;
        /// <summary>
        /// Temporary enable input.use RestoreInputDisableState() to restore.
        /// </summary>
        public static void EnableInputTemporary()
        {
            _lastIsInputDisabled = _isInputDisabled;
            _isInputDisabled = false;
            _isInputDisabledDirty = false;
        }

        /// <summary>
        /// Restore to last saved input disable state.
        /// </summary>
        public static void RestoreInputDisableState()
        {
            if (!_isInputDisabledDirty)
            {
                //Restor value only when value unchanged.
                _isInputDisabled = _lastIsInputDisabled;
            }
        }
    }
}
