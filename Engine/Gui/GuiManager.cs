using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using IniParser;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Gui
{
    public static class GuiManager
    {
        private const int MaxMagic = 49;
        private static readonly Dictionary<int, MagicItemInfo> MagicList = new Dictionary<int, MagicItemInfo>();
        private static MagicGui MagicInterface;
        private static BottomGui BottomInterface;
        public static bool IsMouseStateEated;
        public static DragDropItem DragDropSourceItem;
        public static bool IsDropped;

        public static void Starting()
        {
            MagicInterface = new MagicGui();
            BottomInterface = new BottomGui();
            RenewMagicList();
        }

        public static void Load(string magicListPath)
        {
            LoadMagicList(magicListPath);
        }

        private static void LoadMagicList(string filePath)
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
                        MagicList[head] = new MagicItemInfo(
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
            MagicInterface.UpdateItems();
        }

        private static bool MagicIndexInRange(int index)
        {
            return (index > 0 && index < MaxMagic);
        }

        private static void RenewMagicList()
        {
            for (var i = 1; i < MaxMagic; i++)
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
            }
        }

        public static Magic GetMagic(int index)
        {
            return (MagicIndexInRange(index) && MagicList[index] != null) ?
                MagicList[index].TheMagic : 
                null;
        }

        public static MagicItemInfo GetMagicItemInfo(int index)
        {
            return MagicIndexInRange(index) ? MagicList[index] : null;
        }

        public static void Update(GameTime gameTime)
        {
            MagicInterface.Update(gameTime);
            BottomInterface.Update(gameTime);

            if (IsDropped)
            {
                IsDropped = false;
                DragDropSourceItem = null;
                MagicInterface.UpdateItems();
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            MagicInterface.Draw(spriteBatch);
            BottomInterface.Draw(spriteBatch);
        }
    }

    public class MagicItemInfo
    {
        public Magic TheMagic { private set; get; }
        public int Level { private set; get; }
        public int Exp { private set; get; }

        public MagicItemInfo(string iniFile, int level, int exp)
        {
            TheMagic = Utils.GetMagic(iniFile).GetLevel(level);
            Level = level;
            Exp = exp;
        }
    }
}