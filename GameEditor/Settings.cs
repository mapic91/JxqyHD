using System;
using System.IO;
using IniParser;
using IniParser.Model;

namespace GameEditor
{
    public static class Settings
    {
        private const string IniFilePath = "GameEditor.ini";
        private static IniData _data = new IniData();

        public static void LoadSettings()
        {
            try
            {
                var data = new FileIniDataParser().ReadFile(IniFilePath);
                _data = data;
            }
            catch (Exception)
            {
                _data.Sections.AddSection("Init");
            }
        }

        public static void Save()
        {
            File.WriteAllText(IniFilePath, _data.ToString());
        }

        public static void SaveFormPositionSize(int x, int y, int width, int height, bool maximized)
        {
            var iniSec = _data.Sections["Init"];
            iniSec["Maximized"] = maximized ? "1" : "0";
            iniSec["X"] = x.ToString();
            iniSec["Y"] = y.ToString();
            iniSec["Width"] = width.ToString();
            iniSec["Height"] = height.ToString();
        }

        public static bool IsMaximized()
        {
            return _data.Sections["Init"]["Maximized"] == "1";
        }

        public static int GetInt(string key, int def)
        {
            var iniSec = _data.Sections["Init"];
            if (iniSec.ContainsKey(key))
            {
                int i;
                if (int.TryParse(iniSec[key], out i))
                {
                    return i;
                }
            }
            return def;
        }
    }
}