using System;
using System.IO;
using Engine.Gui;
using Engine.Gui.Base;
using Engine.Script;
using IniParser;
using IniParser.Model;

namespace Engine.ListManager
{
    public static class GoodsListManager
    {
        public const int MaxGoods = 223;
        public const int ListIndexBegin = 1;
        public const int ListIndexEnd = 223;
        public const int StoreIndexBegin = 1;
        public const int StoreIndexEnd = 198;
        public const int BottomGoodsIndexBegin = 221;
        public const int BottomGoodsIndexEnd = 223;
        public const int EquipIndexBegin = 201;
        public const int EquipIndexEnd = 207;
        public const int BottomIndexBegin = 221;
        public const int BottomIndexEnd = 223;
        private static readonly GoodsItemInfo[] GoodsList = new GoodsItemInfo[MaxGoods + 1];

        public static void RenewList()
        {
            for (var i = ListIndexBegin; i <= ListIndexEnd; i++)
            {
                GoodsList[i] = null;
            }
        }

        public static bool IndexInRange(int index)
        {
            return (index > 0 && index <= MaxGoods);
        }

        public static bool IsInEquipRange(int index)
        {
            return (index >= EquipIndexBegin && index <= EquipIndexEnd);
        }

        public static bool IsInStoreRange(int index)
        {
            return (index >= StoreIndexBegin && index <= StoreIndexEnd);
        }

        public static bool IsInBottomGoodsRange(int index)
        {
            return (index >= BottomGoodsIndexBegin && index <= BottomGoodsIndexEnd);
        }

        public static void LoadList(string filePath)
        {
            RenewList();
            GuiManager.UpdateGoodsView(); // clear
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
                        GoodsList[head] = new GoodsItemInfo(
                            section["IniFile"],
                            int.Parse(section["Number"]));
                    }
                }
            }
            catch (Exception exception)
            {
                RenewList();
                Log.LogFileLoadError("Goods list", filePath, exception);
            }
            GuiManager.UpdateGoodsView();
        }

        public static void SaveList(string filePath)
        {
            try
            {
                var data = new IniData();
                data.Sections.AddSection("Head");
                var count = 0;
                for (var i = 1; i <= MaxGoods; i++)
                {
                    var item = GoodsList[i];
                    if (item != null && item.TheGood != null)
                    {
                        count++;
                        data.Sections.AddSection(i.ToString());
                        var section = data[i.ToString()];
                        section.AddKey("IniFile", item.TheGood.FileName);
                        section.AddKey("Number", item.Count.ToString());
                    }
                }
                data["Head"].AddKey("Count", count.ToString());
                //Write to file
                File.WriteAllText(filePath, data.ToString(), Globals.LocalEncoding);
            }
            catch (Exception exception)
            {
                Log.LogFileSaveError("Goods list", filePath, exception);
            }
        }

        public static void ApplyEquipSpecialEffectFromList(Player player)
        {
            if (player == null) return;
            for (var i = EquipIndexBegin; i <= EquipIndexEnd; i++)
            {
                player.Equiping(Get(i), null, true);
            }
        }

        public static void ExchangeListItemAndEquiping(int index1, int index2)
        {
            if (index1 != index2 &&
                IndexInRange(index1) &&
                IndexInRange(index2))
            {
                var temp = GoodsList[index1];
                GoodsList[index1] = GoodsList[index2];
                GoodsList[index2] = temp;
                ChangePlayerEquiping(index1, index2);
            }
        }

        /// <summary>
        /// If return true, newIndex is the new index
        /// </summary>
        /// <param name="equipItemIndex"></param>
        /// <param name="newIndex"></param>
        /// <returns></returns>
        public static bool MoveEquipItemToList(int equipItemIndex, out int newIndex)
        {
            if (IsInEquipRange(equipItemIndex))
            {
                var info = GoodsList[equipItemIndex];
                if (info != null)
                {
                    GoodsList[equipItemIndex] = null;
                    for (var i = StoreIndexBegin; i <= StoreIndexEnd; i++)
                    {
                        if (GoodsList[i] == null)
                        {
                            GoodsList[i] = info;
                            newIndex = i;
                            return true;
                        }
                    }
                }
            }
            newIndex = 0;
            return false;
        }

        public static void AddGoodToList(string fileName)
        {
            int i;
            Good g;
            AddGoodToList(fileName, out i, out g);
        }

        public static bool AddGoodToList(string fileName, out int index, out Good outGood)
        {
            index = -1;
            outGood = null;
            for (var i = ListIndexBegin; i <= ListIndexEnd; i++)
            {
                var info = GoodsList[i];
                if (info != null && info.TheGood != null)
                {
                    if (Utils.EqualNoCase(info.TheGood.FileName, fileName))
                    {
                        info.Count += 1;
                        index = i;
                        outGood = info.TheGood;
                        return true;
                    }
                }
            }

            for (var i = StoreIndexBegin; i <= StoreIndexEnd; i++)
            {
                var info = GoodsList[i];
                if (info == null)
                {
                    GoodsList[i] = new GoodsItemInfo(fileName, 1);
                    index = i;
                    outGood = GoodsList[i].TheGood;
                    return true;
                }
            }

            return false;
        }

        public static GoodsItemInfo GetGoodsItemInfoFromFileName(string fileName)
        {
            for (var i = ListIndexBegin; i <= ListIndexEnd; i++)
            {
                var info = GoodsList[i];
                if (info != null && info.TheGood != null)
                {
                    if (Utils.EqualNoCase(info.TheGood.FileName, fileName))
                    {
                        return info;
                    }
                }
            }
            return null;
        }

        public static int GetGoodsNum(string fileName)
        {
            var info = GetGoodsItemInfoFromFileName(fileName);
            if (info != null)
            {
                return info.Count;
            }
            return 0;
        }

        public static void DeleteGood(string fileName)
        {
            var i = ListIndexBegin;
            for (; i <= ListIndexEnd; i++)
            {
                if (GoodsList[i] != null &&
                    GoodsList[i].TheGood != null &&
                    Utils.EqualNoCase(GoodsList[i].TheGood.FileName, fileName))
                    break;
            }
            if (i <= ListIndexEnd)
            {
                var info = GoodsList[i];
                var good = info.TheGood;
                if (info.Count == 1)
                    GoodsList[i] = null;
                else
                    info.Count -= 1;

                //if goods is unequiped
                if (i >= EquipIndexBegin && i <= EquipIndexEnd && GoodsList[i] == null)
                {
                    if (Globals.ThePlayer != null)
                        Globals.ThePlayer.UnEquiping(good);
                }
            }
        }

        public static bool CanEquip(int goodIndex, Good.EquipPosition position)
        {
            return (!IsInEquipRange(goodIndex) &&
                Good.CanEquip(Get(goodIndex), position));
        }

        public static void ChangePlayerEquiping(int index1, int index2)
        {
            Good equip = null;
            Good currentEquip = null;
            if (IsInEquipRange(index1))
            {
                equip = Get(index1);
                currentEquip = Get(index2);
            }
            else if (IsInEquipRange(index2))
            {
                equip = Get(index2);
                currentEquip = Get(index1);
            }
            Globals.ThePlayer.Equiping(equip, currentEquip);
        }

        public static bool PlayerUnEquiping(int equipIndex, out int newIndex)
        {
            if (IsInEquipRange(equipIndex))
            {
                if (MoveEquipItemToList(equipIndex, out newIndex))
                {
                    Globals.ThePlayer.UnEquiping(Get(newIndex));
                    return true;
                }
            }
            newIndex = 0;
            return false;
        }

        public static void UsingGood(int goodIndex)
        {
            if (IsInEquipRange(goodIndex))
            {
                //Can't use equiped good.
                return;
            }

            var info = GetItemInfo(goodIndex);
            if (info == null) return;
            var good = info.TheGood;
            if (good != null)
            {
                switch (good.Kind)
                {
                    case Good.GoodKind.Drug:
                        {
                            if (Globals.ThePlayer.UseDrug(good))
                            {

                                if (info.Count == 1)
                                    GoodsList[goodIndex] = null;
                                else
                                    info.Count -= 1;
                            }
                            var sound = Utils.GetSoundEffect("界-使用物品.wav");
                            if (sound != null)
                            {
                                sound.Play();
                            }
                        }
                        break;
                    case Good.GoodKind.Equipment:
                        GuiManager.EquipInterface.EquipGood(goodIndex);
                        break;
                    case Good.GoodKind.Event:
                        {
                            ScriptManager.RunScript(Utils.GetScriptParser(
                                good.FileName, good, null, Utils.ScriptCategory.Good));
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                GuiManager.UpdateGoodItemView(goodIndex);
            }
        }

        public static Good Get(int index)
        {
            var itemInfo = GetItemInfo(index);
            return (itemInfo != null) ?
                itemInfo.TheGood :
                null;
        }

        public static Texture GetTexture(int index)
        {
            var good = Get(index);
            if (good != null)
            {
                if (index >= BottomIndexBegin && index <= BottomIndexEnd)
                    return new Texture(good.Icon);
                else
                    return new Texture(good.Image);
            }
            return null;
        }

        public static Asf GetImage(int index)
        {
            var good = Get(index);
            if (good != null)
                return good.Image;
            return null;
        }

        public static Asf GetIcon(int index)
        {
            var good = Get(index);
            if (good != null)
                return good.Icon;
            return null;
        }

        public static GoodsItemInfo GetItemInfo(int index)
        {
            return IndexInRange(index) ? GoodsList[index] : null;
        }

        public class GoodsItemInfo
        {
            public Good TheGood;
            public int Count;

            public GoodsItemInfo(string fileName, int count)
            {
                var good = Utils.GetGood(fileName);
                if (good != null) TheGood = good;
                Count = count;
            }
        }
    }
}