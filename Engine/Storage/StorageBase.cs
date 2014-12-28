using System.Globalization;
using System.IO;
using Microsoft.Xna.Framework;

namespace Engine.Storage
{
    public static class StorageBase
    {
        public const string GameIniFilePath = @"save\game\Game.ini";
        public const string MemoListIniFilePath = @"save\game\memo.ini";
        public const string TrapsFilePath = @"save\game\Traps.ini";
        public const string SaveGameDirectory = @"save\game";
        public const string SaveRpgDirectory = @"save\rpg";
        public const string SaveShotDirectory = @"save\Shot";

        public static string PlayerFilePath
        {
            get {return @"save\game\" + "Player" + Globals.PlayerIndex + ".ini";}
        }

        public static string PartnerFilePath
        {
            get { return @"save\game\" + "partner" + Globals.PlayerIndex + ".ini"; }
        }

        public static string MagicListFilePath
        {
            get { return @"save\game\" + "Magic" + Globals.PlayerIndex + ".ini"; }
        }

        public static string GoodsListFilePath
        {
            get {return @"save\game\" + "Goods" + Globals.PlayerIndex + ".ini";}
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

        public static void MoveAllFilesToDirectory(string directory, string targetDirectory)
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
            MoveAllFilesToDirectory(SaveRpgDirectory + saveIndex, SaveGameDirectory);
        }
    }
}