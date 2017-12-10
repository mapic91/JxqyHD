using System;
using System.IO;
using Engine.Gui;
using Engine.Gui.Base;
using IniParser;
using IniParser.Model;

namespace Engine.ListManager
{
    public static class MagicListManager
    {
        public static int MaxMagic = 49;
        public static int MagicListIndexBegin = 1;
        public static int StoreIndexBegin = 1;
        public static int StoreIndexEnd = 36;
        public static int HideStartIndex = 1000;
        private static MagicItemInfo[] MagicList = new MagicItemInfo[MaxMagic + 1];
        private static MagicItemInfo[] MagicListHide = new MagicItemInfo[MaxMagic + 1];

        public static int XiuLianIndex = 49;
        public static int BottomIndexBegin = 40;
        public static int BottomIndexEnd = 44;

        public static void InitIndex(IniData settings)
        {
            var cfg = settings.Sections["MagicInit"];
            StoreIndexBegin = int.Parse(cfg["StoreIndexBegin"]);
            StoreIndexEnd = int.Parse(cfg["StoreIndexEnd"]);
            BottomIndexBegin = int.Parse(cfg["BottomIndexBegin"]);
            BottomIndexEnd = int.Parse(cfg["BottomIndexEnd"]);
            XiuLianIndex = int.Parse(cfg["XiuLianIndex"]);
            HideStartIndex = int.Parse(cfg["HideStartIndex"]);
            MaxMagic = Math.Max(0, StoreIndexEnd);
            MaxMagic = Math.Max(MaxMagic, BottomIndexEnd);
            MaxMagic = Math.Max(MaxMagic, XiuLianIndex);
            MagicList = new MagicItemInfo[MaxMagic + 1];
            MagicListHide = new MagicItemInfo[MaxMagic + 1];
        }
        public static void LoadList(string filePath)
        {
            RenewList();
            GuiManager.UpdateMagicView();// clear
            try
            {
                var parser = new FileIniDataParser();
                var data = parser.ReadFile(filePath, Globals.LocalEncoding);
                foreach (var sectionData in data.Sections)
                {
                    int head;
                    if (int.TryParse(sectionData.SectionName, out head))
                    {
                        var section = data[sectionData.SectionName];
                        var info = new MagicItemInfo(
                            section["IniFile"],
                            int.Parse(section["Level"]),
                            int.Parse(section["Exp"])
                            );
                        if (head >= HideStartIndex)
                        {
                            MagicListHide[head - HideStartIndex] = info;
                        }
                        else
                        {
                            MagicList[head] = info;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                RenewList();
                Log.LogFileLoadError("Magic list", filePath, exception);
            }
            GuiManager.UpdateMagicView();
        }

        public static void SaveList(string filePath)
        {
            try
            {
                var data = new IniData();
                data.Sections.AddSection("Head");
                var count = 0;
                for (var i = 1; i <= MaxMagic; i++)
                {
                    var item = MagicList[i];
                    if (item != null && item.TheMagic != null)
                    {
                        count++;
                        data.Sections.AddSection(i.ToString());
                        var section = data[i.ToString()];
                        section.AddKey("IniFile", item.TheMagic.FileName);
                        section.AddKey("Level", item.Level.ToString());
                        section.AddKey("Exp", item.Exp.ToString());
                    }

                    item = MagicListHide[i];
                    if (item != null && item.TheMagic != null)
                    {
                        var index = HideStartIndex + i;
                        data.Sections.AddSection(index.ToString());
                        var section = data[index.ToString()];
                        section.AddKey("IniFile", item.TheMagic.FileName);
                        section.AddKey("Level", item.Level.ToString());
                        section.AddKey("Exp", item.Exp.ToString());
                    }
                }
                data["Head"].AddKey("Count", count.ToString());
                //Write to file
                File.WriteAllText(filePath, data.ToString(), Globals.LocalEncoding);
            }
            catch (Exception exception)
            {
                Log.LogFileSaveError("Magic list", filePath, exception);
            }
        }

        public static bool IndexInRange(int index)
        {
            return (index >= MagicListIndexBegin && index <= MaxMagic);
        }

        public static bool IndexInBottomRange(int index)
        {
            return (index >= BottomIndexBegin && index <= BottomIndexEnd);
        }

        public static bool IndexInXiuLianIndex(int index)
        {
            return index == XiuLianIndex;
        }

        public static void RenewList()
        {
            for (var i = 1; i <= MaxMagic; i++)
            {
                MagicList[i] = null;
                MagicListHide[i] = null;
            }
        }

        public static void ExchangeListItem(int index1, int index2)
        {
            if (index1 != index2 &&
                IndexInRange(index1) &&
                IndexInRange(index2))
            {
                var temp = MagicList[index1];
                MagicList[index1] = MagicList[index2];
                MagicList[index2] = temp;

                if (Globals.ThePlayer != null)
                {
                    //Current magic in use
                    var info = Globals.ThePlayer.CurrentMagicInUse;
                    if (info != null)
                    {
                        var inbottom1 = IndexInBottomRange(index1);
                        var inbottom2 = IndexInBottomRange(index2);
                        if (inbottom1 != inbottom2)
                        {
                            if (info == MagicList[index1] ||
                                info == MagicList[index2])
                            {
                                //Bottom magic item exchange out, player can't use this magic anymore.
                                Globals.ThePlayer.CurrentMagicInUse = null;
                            }
                            
                        }
                    }

                    //XiuLian magic
                    if (IndexInXiuLianIndex(index1))
                    {
                        Globals.ThePlayer.XiuLianMagic = MagicList[index1];
                    }
                    if (IndexInXiuLianIndex(index2))
                    {
                        Globals.ThePlayer.XiuLianMagic = MagicList[index2];
                    }
                }
            }
        }

        public static Magic Get(int index)
        {
            var itemInfo = GetItemInfo(index);
            return (itemInfo != null) ?
                itemInfo.TheMagic :
                null;
        }

        public static int GetIndex(string fileName)
        {
            for (var i = 1; i <= MaxMagic; i++)
            {
                var info = MagicList[i];
                if (info != null)
                {
                    var magic = info.TheMagic;
                    if (magic != null)
                    {
                        if (Utils.EqualNoCase(magic.FileName, fileName))
                        {
                            return i;
                        }
                    }
                }
            }
            return -1;
        }

        public static Texture GetTexture(int index)
        {
            var magic = Get(index);
            if (magic != null)
            {
                if(index >= BottomIndexBegin && index <= BottomIndexEnd)
                    return new Texture(magic.Icon);
                else
                    return new Texture(magic.Image);
            }
            return null;
        }

        public static Asf GetImage(int index)
        {
            var magic = Get(index);
            if (magic != null)
                return magic.Image;
            return null;
        }

        public static Asf GetIcon(int index)
        {
            var magic = Get(index);
            if (magic != null)
                return magic.Icon;
            return null;
        }

        public static MagicItemInfo GetItemInfo(int index)
        {
            return IndexInRange(index) ? MagicList[index] : null;
        }

        /// <summary>
        /// Get index of info in list.
        /// </summary>
        /// <param name="info">Magic item info to find.</param>
        /// <returns>Item index.Return 0 if not found.</returns>
        public static int GetItemIndex(MagicItemInfo info)
        {
            if (info != null)
            {
                for (int i = MagicListIndexBegin; i <= MaxMagic; i++)
                {
                    if (info == MagicList[i])
                    {
                        return i;
                    }
                }
            }
            return 0;
        }

        public static int GetFreeIndex()
        {
            var index = -1;
            for (var i = StoreIndexBegin; i <= StoreIndexEnd; i++)
            {
                if (MagicList[i] == null)
                {
                    index = i;
                    break;
                }
            }

            if (index == -1)
            {
                for (var i = BottomIndexBegin; i <= BottomIndexEnd; i++)
                {
                    if (MagicList[i] == null)
                    {
                        index = i;
                        break;
                    }
                }
            }
            return index;
        }

        public static MagicItemInfo GetMagic(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return null;

            for (var i = 1; i <= MaxMagic; i++)
            {
                if (MagicList[i] != null)
                {
                    var magic = MagicList[i].TheMagic;
                    if (magic != null)
                    {
                        if (Utils.EqualNoCase(magic.FileName, fileName))
                        {
                            return MagicList[i];
                        }
                    }
                }
            }

            return null;
        }

        public static int GetMagicLevel(string fileName)
        {
            var info = GetMagic(fileName);
            if (info != null && info.TheMagic != null)
            {
                return info.Level;
            }
            return 0;
        }

        public static bool AddMagicToList(string fileName, out int index, out Magic outMagic)
        {
            index = -1;
            outMagic = null;
            if (string.IsNullOrEmpty(fileName)) return false;

            for (var i = 1; i <= MaxMagic; i++)
            {
                if (MagicList[i] != null)
                {
                    var magic = MagicList[i].TheMagic;
                    if (magic != null)
                    {
                        if (Utils.EqualNoCase(magic.FileName, fileName))
                        {
                            index = i;
                            outMagic = magic;
                            return false;
                        }
                    }
                }
            }

            index = GetFreeIndex();

            if (index != -1)
            {
                MagicList[index] = new MagicItemInfo(fileName, 1, 0);
                outMagic = MagicList[index].TheMagic;
                return true;
            }
            return false;
        }

        public static bool IsMagicHided(string fileName)
        {
            for (var i = 1; i <= MaxMagic; i++)
            {
                if (MagicListHide[i] != null)
                {
                    var magic = MagicListHide[i].TheMagic;
                    if (magic != null)
                    {
                        if (Utils.EqualNoCase(magic.FileName, fileName))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public static MagicItemInfo SetMagicHide(string fileName, bool isHide)
        {
            if (isHide)
            {
                var index = GetIndex(fileName);
                if (index != -1)
                {
                    var info = MagicList[index];
                    for (var i = 1; i <= MaxMagic; i++)
                    {
                        if (MagicListHide[i] == null)
                        {
                            MagicListHide[i] = info;
                            MagicList[index] = null;

                            if (index == XiuLianIndex && Globals.ThePlayer != null)
                            {
                                Globals.ThePlayer.XiuLianMagic = null;
                            }

                            return info;
                        }
                    }
                }
            }
            else
            {
                for (var i = 1; i <= MaxMagic; i++)
                {
                    if (MagicListHide[i] != null)
                    {
                        var magic = MagicListHide[i].TheMagic;
                        if (magic != null)
                        {
                            if (Utils.EqualNoCase(magic.FileName, fileName))
                            {
                                var info = MagicListHide[i];
                                for (var j = StoreIndexBegin; j <= StoreIndexEnd; j++)
                                {
                                    if (MagicList[j] == null)
                                    {
                                        MagicList[j] = info;
                                        return info;
                                    }
                                }
                                break;
                            }
                        }
                    }
                }
            }

            return null;
        }

        public static void SetMagicLevel(string fileName, int level)
        {
            var info = GetMagic(fileName);
            if (info == null || info.TheMagic == null) return;

            info.Exp = level > 1 ? info.TheMagic.GetLevel(level - 1).LevelupExp : 0;
            info.TheMagic = info.TheMagic.GetLevel(level);
        }

        public class MagicItemInfo
        {
            private float _remainColdMilliseconds;
            public Magic TheMagic { set; get; }

            public int Level
            {
                get { return TheMagic == null ? 1 : TheMagic.CurrentLevel; }
            }

            public int Exp { set; get; }

            public float RemainColdMilliseconds
            {
                get { return _remainColdMilliseconds; }
                set
                {
                    if (value < 0)
                    {
                        value = 0;
                    }
                    _remainColdMilliseconds = value;
                }
            }

            public MagicItemInfo(string iniFile, int level, int exp)
            {
                var magic = Utils.GetMagic(iniFile, false);
                if (magic != null)
                {
                    TheMagic = magic.GetLevel(level);
                    TheMagic.ItemInfo = this;
                }
                Exp = exp;
            }
        }
    }
}