using System;
using System.IO;
using Engine.Gui;
using Engine.Script;
using Engine.Weather;
using IniParser;
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
            option.AddKey("MuiscVolume", ((int)(100 - MediaPlayer.Volume*100)).ToString());
            option.AddKey("SoundVolume", ((int)(100 - SoundEffect.MasterVolume*100)).ToString());
            option.AddKey("MapTime", Map.MapTime.ToString());
            option.AddKey("SnowShow", (WeatherManager.IsSnowing ? 1 : 0).ToString());
            option.AddKey("RainFile", WeatherManager.IsRaining ? WeatherManager.RainFileName : "");
            option.AddKey("Water", Globals.IsWaterEffectEnabled ? "1" : "0");
            option.AddKey("MpcStyle", StorageBase.GetStringFromColor(Map.DrawColor));
            option.AddKey("AsfStyle", StorageBase.GetStringFromColor(Sprite.DrawColor));

            //Variables
            data.Sections.AddSection("Var");
            ScriptExecuter.SaveVariables(data["Var"]);

            //Wirte to file
            File.WriteAllText(StorageBase.GameIniFilePath, data.ToString(), Globals.SimpleChineseEncoding);
        }

        private static void SavePlayer()
        {
            var data = new IniData();
            data.Sections.AddSection("Init");
            Globals.ThePlayer.Save(data["Init"]);
            File.WriteAllText(StorageBase.PlayerFilePath, data.ToString(), Globals.SimpleChineseEncoding);
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

        /// <summary>
        /// Save game to "save/game" directory.
        /// </summary>
        public static void SaveGame()
        {
            StorageBase.DeletAllFiles(StorageBase.SaveGameDirectory);
            SaveGameFile();
            SaveMagicGoodMemoList();
            SavePlayer();
            SavePartner();
            SaveTraps();
        }

        /// <summary>
        /// Save game to 0-7
        /// </summary>
        /// <param name="index">Save index</param>
        public static void SaveGame(int index)
        {
            SaveGame();
            StorageBase.CopyGameToSave(index);
        }
    }
}