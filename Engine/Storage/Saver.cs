using System;
using System.IO;
using Engine.Gui;
using Engine.Script;
using Engine.Weather;
using IniParser.Model;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Media;

namespace Engine.Storage
{
    public static class Saver
    {
        private static void SaveGameFile()
        {
            var data = new IniData();

            //State
            data.Sections.AddSection("State");
            var state = data["State"];
            state.AddKey("Map", Globals.TheMap.MapFileNameWithoutExtension + ".map");
            state.AddKey("Npc", NpcManager.FileName);
            state.AddKey("Obj", ObjManager.FileName);
            state.AddKey("Bgm", BackgroundMusic.FileName);
            state.AddKey("Chr", Globals.PlayerIndex.ToString());
            state.AddKey("Time", string.Format(
                "{0:yyyy} 年{0:MM} 月{0:dd} 日 {0:HH} 时{0:mm} 分{0:ss} 秒", 
                DateTime.Now));

            //Save npc obj
            NpcManager.SaveNpc();
            ObjManager.Save();

            //Option
            data.Sections.AddSection("Option");
            var option = data["Option"];
            option.AddKey("MapTime", Map.MapTime.ToString());
            option.AddKey("SnowShow", (WeatherManager.IsSnowing ? 1 : 0).ToString());
            option.AddKey("RainFile", WeatherManager.IsRaining ? WeatherManager.RainFileName : "");
            option.AddKey("Water", Globals.IsWaterEffectEnabled ? "1" : "0");
            option.AddKey("MpcStyle", StorageBase.GetStringFromColor(Map.DrawColor));
            option.AddKey("AsfStyle", StorageBase.GetStringFromColor(Sprite.DrawColor));

            //Timer
            data.Sections.AddSection("Timer");
            var timer = data["Timer"];
            timer.AddKey("IsOn", GuiManager.IsTimerStarted() ? "1" : "0");
            if (GuiManager.IsTimerStarted())
            {
                timer.AddKey("TotalSecond", GuiManager.GetTimerCurrentSeconds().ToString());
                timer.AddKey("IsTimerWindowShow", GuiManager.IsTimerWindowHided() ? "0" : "1");
                timer.AddKey("IsScriptSet", ScriptExecuter.IsTimeScriptSet ? "1" : "0");
                timer.AddKey("TimerScript", ScriptExecuter.TimeScriptFileName);
                timer.AddKey("TriggerTime", ScriptExecuter.TimeScriptSeconds.ToString());
            }

            //Variables
            data.Sections.AddSection("Var");
            ScriptExecuter.SaveVariables(data["Var"]);

            //Wirte to file
            File.WriteAllText(StorageBase.GameIniFilePath, data.ToString(), Globals.LocalEncoding);
        }

        public static void SavePlayer()
        {
            var data = new IniData();
            data.Sections.AddSection("Init");
            Globals.ThePlayer.Save(data["Init"]);
            File.WriteAllText(StorageBase.PlayerFilePath, data.ToString(), Globals.LocalEncoding);
        }

        private static void SavePartner()
        {
            NpcManager.SavePartner(StorageBase.PartnerFileName);
        }

        private static void SaveMagicGoodMemoList()
        {
            GuiManager.Save(StorageBase.MagicListFilePath,
                StorageBase.GoodsListFilePath,
                StorageBase.MemoListIniFilePath);
        }

        private static void SaveTraps()
        {
            Globals.TheMap.SaveTrap(StorageBase.TrapsFilePath);
        }

        private static void SaveTrapIgnoreList()
        {
            Globals.TheMap.SaveTrapIndexIgnoreList(StorageBase.TrapIndexIgnoreListFilePath);
        }

        /// <summary>
        /// Save game to "save/game" directory.
        /// </summary>
        public static void SaveGame()
        {
            SaveGameFile();
            SaveMagicGoodMemoList();
            SavePlayer();
            SavePartner();
            SaveTraps();
            SaveTrapIgnoreList();
        }

        /// <summary>
        /// Save game to 0-7
        /// </summary>
        /// <param name="index">Save index</param>
        public static void SaveGame(int index)
        {
            if(!StorageBase.IsIndexInRange(index)) return;
            SaveGame();
            StorageBase.CopyGameToSave(index);
        }
    }
}