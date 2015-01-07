using System;
using System.Collections.Generic;
using Engine.Gui;
using IniParser;

namespace Engine.ListManager
{
    public static class MemoListManager
    {
        private static readonly LinkedList<string> MemoList = new LinkedList<string>(); 
        public static void LoadList(string filePath)
        {
            RenewList();
            GuiManager.UpdateMemoView();
            try
            {
                var parser = new FileIniDataParser();
                var data = parser.ReadFile(filePath, Globals.SimpleChineseEncoding);
                var section = data["Memo"];
                var count = int.Parse(section["Count"]);
                for (var i = 0; i < count; i++)
                {
                    MemoList.AddLast(section[i.ToString()]);
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
            {
                var node = MemoList.First;
                for (var i = 0; i < index; i++)
                {
                    node = node.Next;
                }
                return node.Value;
            }
            return "";
        }

        public static void AddMemo(string text)
        {
            text = "●" + text;
            var lines = Utils.SpliteStringInCharCount(text, 10);
            var count = lines.Count;
            //Add reversely
            for (var i = count - 1; i >= 0; i--)
            {
                MemoList.AddFirst(lines[i]);
            }
        }
    }
}