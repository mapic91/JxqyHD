using System;
using Engine.Gui;
using Engine.ListManager;
using Engine.Map;
using Engine.Script;
using Engine.Weather;
using IniParser;
using Microsoft.Xna.Framework;

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
                MapBase.OpenMap(state["Map"]);
                NpcManager.Load(state["Npc"]);
                ObjManager.Load(state["Obj"]);
                BackgroundMusic.Play(state["Bgm"]);
                Globals.PlayerIndex = int.Parse(state["Chr"]);

                //option
                var option = data["Option"];
                MapBase.MapTime = int.Parse(option["MapTime"]);
                WeatherManager.ShowSnow(int.Parse(option["SnowShow"]) != 0);
                if (!string.IsNullOrEmpty(option["RainFile"]))
                {
                    WeatherManager.BeginRain(option["RainFile"]);
                }
                if (string.IsNullOrEmpty(option["Water"]))
                {
                    Globals.IsWaterEffectEnabled = false;
                }
                else
                {
                    Globals.IsWaterEffectEnabled = int.Parse(option["Water"]) != 0;
                }
                if (string.IsNullOrEmpty(option["MpcStyle"]))
                {
                    MapBase.DrawColor = Color.White;
                }
                else
                {
                    MapBase.DrawColor = StorageBase.GetColorFromString(option["MpcStyle"]);
                }
                if (string.IsNullOrEmpty(option["AsfStyle"]))
                {
                    Sprite.DrawColor = Color.White;
                }
                else
                {
                    Sprite.DrawColor = StorageBase.GetColorFromString(option["AsfStyle"]);
                }
                if (string.IsNullOrEmpty(option["SaveDisabled"]))
                {
                    Globals.IsSaveDisabled = false;
                }
                else
                {
                    Globals.IsSaveDisabled = int.Parse(option["SaveDisabled"]) > 0;
                }

                //Timer
                var timer = data["Timer"];
                if (timer != null)
                {
                    var isOn = timer["IsOn"] != "0";
                    if (isOn)
                    {
                        ScriptExecuter.OpenTimeLimit(int.Parse(timer["TotalSecond"]));
                        var isHide = timer["IsTimerWindowShow"] != "1";
                        if (isHide)
                        {
                            ScriptExecuter.HideTimerWnd();
                        }
                        if (timer["IsScriptSet"] != "0")
                        {
                            ScriptExecuter.SetTimeScript(int.Parse(timer["TriggerTime"]),
                                timer["TimerScript"]);
                        }
                    }
                }

                //Variables
                ScriptExecuter.LoadVariables(data["Var"]);
            }
            catch (Exception exception)
            {
                Log.LogFileLoadError("Game.ini", StorageBase.GameIniFilePath, exception);
            }
        }

        /// <summary>
        /// Goods and magic list must load first.
        /// Player using goods list to apply equip special effect.
        /// Player using magic list to load current use magic.
        /// </summary>
        private static void LoadPlayer()
        {
            var path = StorageBase.PlayerFilePath;
            try
            {
                Globals.ThePlayer = new Player(path);
                Globals.TheCarmera.CenterPlayerInCamera();
                GoodsListManager.ApplyEquipSpecialEffectFromList(Globals.ThePlayer);
            }
            catch (Exception exception)
            {
                Log.LogFileLoadError("Player", path, exception);
            }
            GuiManager.StateInterface.Index = GuiManager.EquipInterface.Index = Globals.PlayerIndex;
        }

        private static void LoadPartner()
        {
            NpcManager.LoadPartner(StorageBase.PartnerFilePath);
        }

        /// <summary>
        /// GuiManager must start first
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
            MapBase.LoadTrap(StorageBase.TrapsFilePath);
        }

        private static void LoadTrapIgnoreList()
        {
            MapBase.LoadTrapIndexIgnoreList(StorageBase.TrapIndexIgnoreListFilePath);
        }

        /// <summary>
        /// Load game from "save/game" directory
        /// GuiManager must started first
        /// </summary>
        public static void LoadGame(bool isInitlizeGame)
        {
            if (isInitlizeGame)
            {
                //Clear
                ScriptManager.Clear();
                ScriptExecuter.Init();
                Utils.ClearScriptParserCache();
                MagicManager.Clear();
                NpcManager.ClearAllNpc();
                ObjManager.ClearAllObjAndFileName();
                MapBase.Free();
                GuiManager.CloseTimeLimit();
                GuiManager.EndDialog();
                BackgroundMusic.Stop();
                Globals.IsInputDisabled = false;
                Globals.IsSaveDisabled = false;
            }

            LoadGameFile();
            LoadMagicGoodMemoList();
            LoadPlayer();
            //Apply xiulian magic to player
            Globals.ThePlayer.XiuLianMagic = MagicListManager.GetItemInfo(
                MagicListManager.XiuLianIndex);

            LoadPartner();
            LoadTraps();
            LoadTrapIgnoreList();

            Globals.TheCarmera.CenterPlayerInCamera();

            GameState.State = GameState.StateType.Playing;
            Globals.TheGame.IsGamePlayPaused = false;
            GuiManager.ShowAllPanels(false);
        }

        /// <summary>
        /// Load game form 0-7
        /// </summary>
        /// <param name="index">Load index</param>
        public static void LoadGame(int index)
        {
            if (!StorageBase.IsIndexInRange(index)) return;
            StorageBase.ClearGameAndCopySaveToGame(index);
            LoadGame(index != 0);
        }

        public static void NewGame()
        {
            ScriptExecuter.RunScript("NewGame.txt");
        }

        public static void ChangePlayer(int index)
        {
            Saver.SavePlayer();
            Saver.SaveMagicGoodMemoList();

            Globals.PlayerIndex = index;
            GuiManager.StateInterface.Index = GuiManager.EquipInterface.Index = index;
            LoadMagicGoodList();
            LoadPlayer();
        }
    }
}