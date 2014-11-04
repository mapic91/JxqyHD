using System;
using System.Collections.Generic;
using IniParser;

namespace Engine.Gui
{
    public static class MagicListManager
    {
        private const int MaxMagic = 49;
        private static readonly Dictionary<int, MagicManager.MagicItemInfo> MagicList =
            new Dictionary<int, MagicManager.MagicItemInfo>();
        public static void LoadMagicList(string filePath)
        {
            RenewMagicList();
            try
            {
                var parser = new FileIniDataParser();
                var data = parser.ReadFile(filePath, Globals.SimpleChinaeseEncoding);
                foreach (var sectionData in data.Sections)
                {
                    int head;
                    if (int.TryParse(sectionData.SectionName, out head))
                    {
                        var section = data[sectionData.SectionName];
                        MagicList[head] = new MagicManager.MagicItemInfo(
                            section["IniFile"],
                            int.Parse(section["Level"]),
                            int.Parse(section["Exp"])
                            );
                    }
                }
            }
            catch (Exception exception)
            {
                Log.LogMessageToFile("Magic list file[" + filePath + "] read error: [" + exception);
            }
            GuiManager.UpdateMagicView();
        }

        public static bool MagicIndexInRange(int index)
        {
            return (index > 0 && index <= MaxMagic);
        }

        public static void RenewMagicList()
        {
            for (var i = 1; i <= MaxMagic; i++)
            {
                MagicList[i] = null;
            }
        }

        public static void ExchangeMagicListItem(int index1, int index2)
        {
            if (index1 != index2 &&
                MagicIndexInRange(index1) &&
                MagicIndexInRange(index2))
            {
                var temp = MagicList[index1];
                MagicList[index1] = MagicList[index2];
                MagicList[index2] = temp;
                GuiManager.UpdateMagicView();
            }
        }

        public static Magic GetMagic(int index)
        {
            return (MagicIndexInRange(index) && MagicList[index] != null) ?
                MagicList[index].TheMagic :
                null;
        }

        public static MagicManager.MagicItemInfo GetMagicItemInfo(int index)
        {
            return MagicIndexInRange(index) ? MagicList[index] : null;
        }
    }
}