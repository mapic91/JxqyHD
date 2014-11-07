using System;
using System.Collections.Generic;
using IniParser;

namespace Engine.Gui
{
    public static class GoodsListManager
    {
        private const int MaxGoods = 223;
        private const int ListIndexBegin = 1;
        private const int ListIndexEnd = 223;
        private const int StoreIndexBegin = 1;
        private const int StoreIndexEnd = 198;
        private const int EquipIndexBegin = 201;
        private const int EquipIndexEnd = 207;
        private const int BottomIndexBegin = 221;
        private const int BottomIndexEnd = 223;
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

        public static bool EquipIndexInRange(int index)
        {
            return (index >= EquipIndexBegin && index <= EquipIndexEnd);
        }

        public static bool StoreIndexInRange(int index)
        {
            return (index >= StoreIndexBegin && index <= StoreIndexEnd);
        }

        public static void LoadList(string filePath)
        {
            RenewList();
            GuiManager.UpdateGoodsView(); // clear
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

        public static void ExchangeListItemAndEquiping(int index1, int index2)
        {
            if (index1 != index2 &&
                IndexInRange(index1) &&
                IndexInRange(index2))
            {
                var temp = GoodsList[index1];
                GoodsList[index1] = GoodsList[index2];
                GoodsList[index2] = temp;
                Equiping(index1, index2);
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
            if (EquipIndexInRange(equipItemIndex))
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

        public static bool AddGoodToList(string fileName, out int index)
        {
            for (var i = ListIndexBegin; i <= ListIndexEnd; i++)
            {
                var info = GoodsList[i];
                if (info != null && info.TheGood != null)
                {
                    if (info.TheGood.FileName == fileName)
                    {
                        info.Count += 1;
                        index = i;
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
                    return true;
                }
            }

            index = 0;
            return false;
        }

        public static bool CanEquip(int goodIndex, Good.EquipPosition position)
        {
            return Good.CanEquip(GoodsListManager.Get(goodIndex), position);
        }

        public static void Equiping(int index, int currentEquipIndex)
        {
            Good equip = null;
            Good currentEquip = null;
            if (EquipIndexInRange(index))
            {
                equip = Get(index);
                currentEquip = Get(currentEquipIndex);
            }
            else if(EquipIndexInRange(currentEquipIndex))
            {
                equip = Get(currentEquipIndex);
                currentEquip = Get(index);
            }
            Globals.ThePlayer.Equiping(equip, currentEquip);
        }

        public static bool UnEquiping(int equipIndex, out int newIndex)
        {
            if (EquipIndexInRange(equipIndex))
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

        public static bool UsingGood(int goodIndex)
        {
            var info = GetItemInfo(goodIndex);
            if (info == null) return false;
            var good = info.TheGood;
            if (good != null)
            {
                if (good.Kind == Good.GoodKind.Drug)
                {
                    if (Globals.ThePlayer.UseDrag(good))
                    {
                        
                        if (info.Count == 1)
                            GoodsList[goodIndex] = null;
                        else
                            info.Count -= 1;
                        GuiManager.GoodsInterface.UpdateListItem(goodIndex);
                        return true;
                    }
                }
                else if (good.Kind == Good.GoodKind.Event)
                {
                    return false;
                }
            }
            return false;
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