using System;
using Engine.Gui;
using Engine.ListManager;
using Engine.Script;
using Engine.Weather;
using IniParser;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace Engine.Storage
{
    public static class Loader
    {
        private static void LoadGameFile()
        {
            try
            {
                var parser = new FileIniDataParser();
                var data = parser.ReadFile(StorageBase.GameIniFilePath);

                //state
                var state = data["State"];
                Globals.TheMap.LoadMap(state["Map"]);
                NpcManager.Load(state["Npc"]);
                ObjManager.Load(state["Obj"]);
                BackgroundMusic.Play(state["Bgm"]);
                Globals.PlayerIndex = int.Parse(state["Chr"]);

                //option
                var option = data["Option"];
                MediaPlayer.Volume = (100 - int.Parse(option["MuiscVolume"]))/100f;
                SoundEffect.MasterVolume = (100 - int.Parse(option["SoundVolume"]))/100f;
                Map.MapTime = int.Parse(option["MapTime"]);
                WeatherManager.ShowSnow(int.Parse(option["SnowShow"]) != 0);
                if (!string.IsNullOrEmpty(option["RainFile"]))
                {
                    WeatherManager.BeginRain(option["RainFile"]);
                }
                Globals.IsWaterEffectEnabled = 
                    int.Parse(option["Water"]) != 0;
                Map.DrawColor = StorageBase.GetColorFromString(option["MpcStyle"]);
                Sprite.DrawColor = StorageBase.GetColorFromString(option["AsfStyle"]);

                //Variables
                ScriptExecuter.LoadVariables(data["Var"]);
            }
            catch (Exception exception)
            {
                Log.LogFileLoadError("Game.ini", StorageBase.GameIniFilePath, exception);
            }
        }

        /// <summary>
        /// Goods list must load first
        /// </summary>
        private static void LoadPlayer()
        {
            var path = StorageBase.PlayerFilePath;
            try
            {
                Globals.ThePlayer = new Player(path);
                Globals.TheCarmera.PlayerToCenter();
                GoodsListManager.ApplyEquipSpecialEffectFromList(Globals.ThePlayer);
            }
            catch (Exception exception)
            {
                Log.LogFileLoadError("Player", path, exception);
            }
            if (Globals.PlayerIndex == 0)
            {
                GuiManager.StateInterface.IsFemale = false;
            }
            else
            {
                GuiManager.StateInterface.IsFemale = true;
            }
        }

        private static void LoadParter()
        {
            NpcManager.LoadPartner(StorageBase.PartnerFilePath);
        }

        /// <summary>
        /// GuiManager must started first
        /// </summary>
        private static void LoadMagicGoodMemoList()
        {
            GuiManager.Load(StorageBase.MagicListFilePath,
                StorageBase.GoodsListFilePath,
                StorageBase.MemoListIniFilePath);
        }

        private static void LoadMagicGoodList()
        {
            MagicListManager.LoadList(StorageBase.MagicListFilePath);
            GoodsListManager.LoadList(StorageBase.GoodsListFilePath);
        }

        private static void LoadTraps()
        {
            Globals.TheMap.LoadTrap(StorageBase.TrapsFilePath);
        }

        /// <summary>
        /// Load game from "save/game" directory
        /// GuiManager must started first
        /// </summary>
        public static void LoadGame()
        {
            LoadGameFile();
            LoadMagicGoodMemoList();
            LoadPlayer();
            LoadParter();
            LoadTraps();
            GameState.State = GameState.StateType.Playing;
        }

        /// <summary>
        /// Load game form 0-7
        /// </summary>
        /// <param name="index">Save index</param>
        public static void LoadGame(int index)
        {
            StorageBase.ClearGameAndCopySaveToGame(index);
            LoadGame();
        }

        public static void NewGame()
        {
            ScriptExecuter.RunScript("NewGame.txt");
        }

        public static void ChangePlayer(int index)
        {
            Globals.PlayerIndex = index;
            GuiManager.StateInterface.IsFemale = (index == 1);
            LoadMagicGoodList();
            LoadPlayer();
        }
    }
}