using System;
using System.Globalization;
using System.IO;
using IniParser;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Storage
{
    public static class StorageBase
    {
        public const string GameIniFilePath = @"save\game\Game.ini";
        public const string GameIniFileName = "Game.ini";
        public const string MemoListIniFilePath = @"save\game\memo.ini";
        public const string TrapsFilePath = @"save\game\Traps.ini";
        public const string TrapIndexIgnoreListFilePath = @"save\game\TrapIndexIgnore.ini";
        public const string SaveGameDirectory = @"save\game";
        public const string SaveRpgDirectory = @"save\rpg";
        public const int SaveIndexBegin = 0;
        public const int SaveIndexEnd = 7;

        public static string PlayerFilePath
        {
            get {return @"save\game\" + "Player" + Globals.PlayerIndex + ".ini";}
        }

        public static string PartnerFilePath
        {
            get { return @"save\game\" + "partner" + Globals.PlayerIndex + ".ini"; }
        }

        public static string PartnerFileName
        {
            get { return "partner" + Globals.PlayerIndex + ".ini"; }
        }

        public static string MagicListFilePath
        {
            get { return @"save\game\" + "Magic" + Globals.PlayerIndex + ".ini"; }
        }

        public static string GoodsListFilePath
        {
            get {return @"save\game\" + "Goods" + Globals.PlayerIndex + ".ini";}
        }

        public static string GetMagicListPath(string fileName)
        {
            return SaveGameDirectory + @"\" + fileName;
        }

        public static bool IsIndexInRange(int index)
        {
            return (index >= SaveIndexBegin && index <= SaveIndexEnd);
        }

        public static Color GetColorFromString(string str)
        {
            var b = str.Substring(0, 2);
            var g = str.Substring(2, 2);
            var r = str.Substring(4, 2);
            return new Color(int.Parse(r, NumberStyles.HexNumber),
                int.Parse(g, NumberStyles.HexNumber),
                int.Parse(b, NumberStyles.HexNumber));
        }

        public static string GetStringFromColor(Color color)
        {
            return string.Format("{0:X2}{1:X2}{2:X2}00", color.B, color.G, color.R);
        }

        public static void DeletAllFiles(string path)
        {
            var pathInfo = new DirectoryInfo(path);
            var files = pathInfo.GetFiles();
            var dirs = pathInfo.GetDirectories();
            foreach (var fileInfo in files)
            {
                fileInfo.Delete();
            }
            foreach (var directoryInfo in dirs)
            {
                directoryInfo.Delete(true);
            }
        }

        public static void CopyAllFilesToDirectory(string directory, string targetDirectory)
        {
            var files = new DirectoryInfo(directory).GetFiles();
            foreach (var file in files)
            {
                var path = Path.Combine(targetDirectory, file.Name);
                file.CopyTo(path, true);
            }
        }

        public static void ClearGameAndCopySaveToGame(int saveIndex)
        {
            DeletAllFiles(SaveGameDirectory);
            CopyAllFilesToDirectory(SaveRpgDirectory + saveIndex, SaveGameDirectory);
        }

        /// <summary>
        /// Copy all files in game dir to save dir of saveIndex.
        /// </summary>
        /// <param name="saveIndex">Save index</param>
        public static void CopyGameToSave(int saveIndex)
        {
            var path = SaveRpgDirectory + saveIndex;
            DeletAllFiles(path);
            CopyAllFilesToDirectory(SaveGameDirectory, path);
        }

        /// <summary>
        /// Delete all user saved data.
        /// </summary>
        public static void DeleteAllSaveData()
        {
            for (var i = 1; i <= 7; i++)
            {
                DeletAllFiles(SaveRpgDirectory + i);
                if (File.Exists(GetSaveSnapShotFilePath(i)))
                {
                    File.Delete(GetSaveSnapShotFilePath(i));
                }
            }
        }

        public static string GetSaveTime(int saveIndex)
        {
            var path = SaveRpgDirectory + saveIndex;
            path = Path.Combine(path, GameIniFileName);
            var time = "";
            if (File.Exists(path))
            {
                try
                {
                    var parser = new FileIniDataParser();
                    var data = parser.ReadFile(path, Globals.LocalEncoding);
                    time = data["State"]["Time"];
                }
                catch (Exception exception)
                {
                    Log.LogFileLoadError("Get save time", path, exception);
                }
            }
            return time;
        }

        public static string GetSaveSnapShotFilePath(int saveIndex)
        {
            return @"save\Shot\" + "rpg" + saveIndex + ".png";
        }

        /// <summary>
        /// Save snapshot to save dir.
        /// </summary>
        /// <param name="saveIndex">Index of save.</param>
        /// <param name="texture2D">Snapshot texture to save.</param>
        public static void SaveSaveSnapShot(int saveIndex, Texture2D texture2D)
        {
            if(texture2D == null) return;
            var path = GetSaveSnapShotFilePath(saveIndex);
            try
            {
                using (var stream = new FileStream(path, FileMode.Create))
                {
                    texture2D.SaveAsPng(stream, texture2D.Width, texture2D.Height);
                }
            }
            catch (Exception exception)
            {
                Log.LogFileSaveError("Snapshot", path, exception);
            }
        }

        /// <summary>
        /// Test whether load game at index is ok.
        /// </summary>
        /// <param name="saveIndex">Index to load</param>
        /// <returns>True is can load.Otherwise false.</returns>
        public static bool CanLoad(int saveIndex)
        {
            if (!IsIndexInRange(saveIndex)) return false;
             var path = SaveRpgDirectory + saveIndex;
            path = Path.Combine(path, GameIniFileName);
            return File.Exists(path);
        }
    }
}