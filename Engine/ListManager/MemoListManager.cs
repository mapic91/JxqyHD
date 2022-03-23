using System;
using System.Collections.Generic;
using System.IO;
using Engine.Gui;
using IniParser;
using IniParser.Model;

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
                var data = parser.ReadFile(filePath, Globals.LocalEncoding);
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

        public static void SaveList(string filePath)
        {
            try
            {
                var data = new IniData();
                data.Sections.AddSection("Memo");
                var memoSection = data["Memo"];
                var count = MemoList.Count;
                memoSection.AddKey("Count", count.ToString());
                var i = 0;
                foreach (var memo in MemoList)
                {
                    memoSection.AddKey(i.ToString(), memo);
                    i++;
                }
                //Write to file
                File.WriteAllText(filePath, data.ToString(), Globals.LocalEncoding);
            }
            catch (Exception exception)
            {
                Log.LogFileSaveError("Memo file", filePath, exception);
            }
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

        public static void DelMemo(string text)
        {
            text = "●" + text;
            var lines = Utils.SpliteStringInCharCount(text, 10);
            var count = lines.Count;
            for (var line = MemoList.First; line != MemoList.Last; line = line.Next)
            {
                if (line.Value == lines[0])
                {
                    var find = line;
                    line = line.Next;
                    for (var i = 1; i < lines.Count; i++)
                    {
                        if (line == null || line.Value != lines[i])
                        {
                            find = null;
                            break;
                        }
                        line = line.Next;
                    }

                    if (find != null)
                    {
                        for (var i = 0; i < lines.Count; i++)
                        {
                            var remove = find;
                            find = find.Next;
                            MemoList.Remove(remove);
                        }
                        return;
                    }
                }
            }
        }
    }
}