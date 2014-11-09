using System;
using System.Collections.Generic;
using IniParser;

namespace Engine.Gui
{
    public static class MemoListManager
    {
        private static readonly List<string> MemoList = new List<string>(); 
        public static void LoadList(string filePath)
        {
            RenewList();
            GuiManager.UpdateMemoView();
            try
            {
                var parser = new FileIniDataParser();
                var data = parser.ReadFile(filePath, Globals.SimpleChinaeseEncoding);
                var section = data["Memo"];
                var count = int.Parse(section["Count"]);
                for (var i = 0; i < count; i++)
                {
                    MemoList.Add(section[i.ToString()]);
                }
            }
            catch (Exception exception)
            {
                Log.LogFileLoadError("Memo file", filePath, exception);
            }
            GuiManager.UpdateMemoView();
        }

        public static void RenewList()
        {
            MemoList.Clear();
        }

        public static int GetCount()
        {
            return MemoList.Count;
        }

        public static bool IndexInRange(int index)
        {
            return (index >= 0 && index < GetCount());
        }

        public static string GetString(int index)
        {
            if (IndexInRange(index))
                return MemoList[index];
            return "";
        }
    }
}