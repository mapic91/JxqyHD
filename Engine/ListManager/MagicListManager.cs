using System;
using System.Collections.Generic;
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
        private static MagicItemInfo[] _MagicList = new MagicItemInfo[MaxMagic + 1];
        private static MagicItemInfo[] _MagicListHide = new MagicItemInfo[MaxMagic + 1];
        private static MagicItemInfo[] MagicList
        {
            get
            {
                return IsInReplaceMagicList ? ReplaceMagicList[CurrentReplaceMagicListFilePath] : _MagicList;
            }
        }

        private static MagicItemInfo[] MagicListHide
        {
            get { return IsInReplaceMagicList ? ReplaceMagicListHide[CurrentReplaceMagicListFilePath] : _MagicListHide; }
        }

        private static bool IsInReplaceMagicList;
        private static string CurrentReplaceMagicListFilePath;
        private static Dictionary<string, MagicItemInfo[]> ReplaceMagicList = new Dictionary<string, MagicItemInfo[]>();
        private static Dictionary<string, MagicItemInfo[]> ReplaceMagicListHide = new Dictionary<string, MagicItemInfo[]>();

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
            _MagicList = new MagicItemInfo[MaxMagic + 1];
            _MagicListHide = new MagicItemInfo[MaxMagic + 1];
        }

        private static bool LoadList(string filePath, MagicItemInfo[] list, MagicItemInfo[] hideList)
        {
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
                        var hideCount = section.ContainsKey("HideCount") ? int.Parse(section["HideCount"]) : 0;
                        var lastIndexWhenHide = section.ContainsKey("LastIndexWhenHide") ? int.Parse(section["LastIndexWhenHide"]) : 0;
                        var info = new MagicItemInfo(
                            section["IniFile"],
                            int.Parse(section["Level"]),
                            int.Parse(section["Exp"]),
                                hideCount);
                        info.LastIndexWhenHide = lastIndexWhenHide;
                        if (head >= HideStartIndex)
                        {
                            hideList[head - HideStartIndex] = info;
                        }
                        else
                        {
                            list[head] = info;
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Log.LogFileLoadError("Magic list", filePath, exception);
                return false;
            }

            return true;
        }

        public static void LoadPlayerList(string filePath)
        {
            RenewList();
            LoadList(filePath, _MagicList, _MagicListHide);
            GuiManager.UpdateMagicView();
        }

        private static void SaveList(string filePath, MagicItemInfo[] list, MagicItemInfo[] hideList)
        {
            try
            {
                var data = new IniData();
                data.Sections.AddSection("Head");
                var count = 0;
                for (var i = 1; i <= MaxMagic; i++)
                {
                    var item = list[i];
                    if (item != null && item.TheMagic != null)
                    {
                        count++;
                        data.Sections.AddSection(i.ToString());
                        var section = data[i.ToString()];
                        section.AddKey("IniFile", item.TheMagic.FileName);
                        section.AddKey("Level", item.Level.ToString());
                        section.AddKey("Exp", item.Exp.ToString());
                        section.AddKey("HideCount", item.HideCount.ToString());
                        section.AddKey("LastIndexWhenHide", item.LastIndexWhenHide.ToString());
                    }

                    item = hideList[i];
                    if (item != null && item.TheMagic != null)
                    {
                        var index = HideStartIndex + i;
                        data.Sections.AddSection(index.ToString());
                        var section = data[index.ToString()];
                        section.AddKey("IniFile", item.TheMagic.FileName);
                        section.AddKey("Level", item.Level.ToString());
                        section.AddKey("Exp", item.Exp.ToString());
                        section.AddKey("HideCount", item.HideCount.ToString());
                        section.AddKey("LastIndexWhenHide", item.LastIndexWhenHide.ToString());
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

        public static void SavePlayerList(string filePath)
        {
            SaveList(filePath, _MagicList, _MagicListHide);
        }

        public static void SaveReplaceList()
        {
            foreach (var key in ReplaceMagicList.Keys)
            {
                SaveList(key, ReplaceMagicList[key], ReplaceMagicListHide[key]);
            }
        }

        public static void ClearReplaceList()
        {
            ReplaceMagicList.Clear();
            ReplaceMagicListHide.Clear();
        }

        public static void ReplaceListTo(string filePath, List<string> magicFileNamesList)
        {
            IsInReplaceMagicList = true;
            CurrentReplaceMagicListFilePath = filePath;
            if (ReplaceMagicList.ContainsKey(filePath))
            {
                //do nothing
            }
            else if (File.Exists(filePath))
            {
                ReplaceMagicList[filePath] = new MagicItemInfo[MaxMagic + 1];
                ReplaceMagicListHide[filePath] = new MagicItemInfo[MaxMagic + 1];
                LoadList(filePath, ReplaceMagicList[filePath], ReplaceMagicListHide[filePath]);
            }
            else
            {
                ReplaceMagicList[filePath] = new MagicItemInfo[MaxMagic + 1];
                ReplaceMagicListHide[filePath] = new MagicItemInfo[MaxMagic + 1];
                var listI = 0;
                for (var i = BottomIndexBegin; i <= BottomIndexEnd; i++)
                {
                    if (listI < magicFileNamesList.Count)
                    {
                        ReplaceMagicList[filePath][i] = new MagicItemInfo(magicFileNamesList[listI], 1, 0, 1);
                    }
                    else
                    {
                        break;
                    }
                    listI++;
                }

                for (var i = StoreIndexBegin; i <= StoreIndexEnd; i++)
                {
                    if (listI < magicFileNamesList.Count)
                    {
                        ReplaceMagicList[filePath][i] = new MagicItemInfo(magicFileNamesList[listI], 1, 0, 1);
                    }
                    else
                    {
                        break;
                    }
                    listI++;
                }
            }
            GuiManager.UpdateMagicView();
        }

        public static void StopReplace()
        {
            IsInReplaceMagicList = false;
            GuiManager.UpdateMagicView();
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

        public static int GetNonReplaceIndex(string fileName)
        {
            for (var i = 1; i <= MaxMagic; i++)
            {
                var info = _MagicList[i];
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

        public static int GetNonReplaceFreeIndex()
        {
            var index = -1;
            for (var i = StoreIndexBegin; i <= StoreIndexEnd; i++)
            {
                if (_MagicList[i] == null)
                {
                    index = i;
                    break;
                }
            }

            if (index == -1)
            {
                for (var i = BottomIndexBegin; i <= BottomIndexEnd; i++)
                {
                    if (_MagicList[i] == null)
                    {
                        index = i;
                        break;
                    }
                }
            }
            return index;
        }

        public static MagicItemInfo GetNonReplaceMagic(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return null;

            for (var i = 1; i <= MaxMagic; i++)
            {
                if (_MagicList[i] != null)
                {
                    var magic = _MagicList[i].TheMagic;
                    if (magic != null)
                    {
                        if (Utils.EqualNoCase(magic.FileName, fileName))
                        {
                            return _MagicList[i];
                        }
                    }
                }
            }

            return null;
        }

        public static int GetNonReplaceMagicLevel(string fileName)
        {
            var info = GetNonReplaceMagic(fileName);
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
                if (_MagicList[i] != null)
                {
                    var magic = _MagicList[i].TheMagic;
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

            index = GetNonReplaceFreeIndex();

            if (index != -1)
            {
                _MagicList[index] = new MagicItemInfo(fileName, 1, 0, 1);
                outMagic = _MagicList[index].TheMagic;
                return true;
            }
            return false;
        }

        public static void DelMagic(string fileName, Player player)
        {
            for (var i = 0; i < _MagicList.Length; i++)
            {
                var info = _MagicList[i];
                if (info != null)
                {
                    if (info.TheMagic != null && Utils.EqualNoCase(fileName, info.TheMagic.FileName))
                    {
                        player.OnDeleteMagic(info);
                        _MagicList[i] = null;
                    }
                }
            }
            GuiManager.UpdateMagicView();
        }

        public static void ClearLearnedMagic(Player player)
        {
            for (var i = 0; i < _MagicList.Length; i++)
            {
                var info = _MagicList[i];
                if (info != null)
                {
                    if (info.TheMagic != null)
                    {
                        if (!GoodsListManager.IsMagicInEquipedEquip(info.TheMagic.FileName))
                        {
                            player.OnDeleteMagic(info);
                            _MagicList[i] = null;
                        }
                    }
                    else
                    {
                        _MagicList[i] = null;
                    }
                }
            }
            GuiManager.UpdateMagicView();
        }

        public static bool IsMagicHided(string fileName)
        {
            for (var i = 1; i <= MaxMagic; i++)
            {
                if (_MagicListHide[i] != null)
                {
                    var magic = _MagicListHide[i].TheMagic;
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
                var index = GetNonReplaceIndex(fileName);
                if (index != -1)
                {
                    var info = _MagicList[index];
                    info.HideCount--;
                    if (info.HideCount > 0)
                    {
                        return info;
                    }
                    for (var i = 1; i <= MaxMagic; i++)
                    {
                        if (_MagicListHide[i] == null)
                        {
                            info.LastIndexWhenHide = index;
                            _MagicListHide[i] = info;
                            _MagicList[index] = null;

                            if (!IsInReplaceMagicList && index == XiuLianIndex && Globals.ThePlayer != null)
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
                var index = GetNonReplaceIndex(fileName);
                if (index != -1)
                {
                    var info = _MagicList[index];
                    info.HideCount++;
                    return info;
                }

                for (var i = 1; i <= MaxMagic; i++)
                {
                    if (_MagicListHide[i] != null)
                    {
                        var magic = _MagicListHide[i].TheMagic;
                        if (magic != null)
                        {
                            if (Utils.EqualNoCase(magic.FileName, fileName))
                            {
                                var info = _MagicListHide[i];
                                _MagicListHide[i] = null;
                                info.HideCount = 1;
                                int j;
                                if (_MagicList[info.LastIndexWhenHide] == null)
                                {
                                    j = info.LastIndexWhenHide;
                                }
                                else
                                {
                                    for (j = StoreIndexBegin; j <= StoreIndexEnd; j++)
                                    {
                                        if (_MagicList[j] == null)
                                        {
                                            break;
                                        }
                                    }
                                }
                                _MagicList[j] = info;
                                if (!IsInReplaceMagicList && j == XiuLianIndex && Globals.ThePlayer != null)
                                {
                                    Globals.ThePlayer.XiuLianMagic = info;
                                }
                                return info;
                            }
                        }
                    }
                }
            }

            return null;
        }

        public static void SetNonReplaceMagicLevel(string fileName, int level)
        {
            var info = GetNonReplaceMagic(fileName);
            if (info == null || info.TheMagic == null) return;

            info.Exp = level > 1 ? info.TheMagic.GetLevel(level - 1).LevelupExp : 0;
            info.TheMagic = info.TheMagic.GetLevel(level);
        }

        public class MagicItemInfo
        {
            private float _remainColdMilliseconds;
            public Magic TheMagic { set; get; }
            public int HideCount { set; get; }
            public int LastIndexWhenHide { set; get; }

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

            public MagicItemInfo(string iniFile, int level, int exp, int hideCount)
            {
                var magic = Utils.GetMagic(iniFile, false);
                if (magic != null)
                {
                    TheMagic = magic.GetLevel(level);
                    TheMagic.ItemInfo = this;
                }
                Exp = exp;
                HideCount = hideCount;
            }
        }

        public static void SetMagicEffect(Player player)
        {
            player.LoadMagicEffect(_MagicList);
            player.LoadMagicEffect(_MagicListHide);
        }
    }
}