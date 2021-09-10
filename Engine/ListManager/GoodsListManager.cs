using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Engine.Gui;
using Engine.Gui.Base;
using Engine.Script;
using IniParser;
using IniParser.Model;
using Microsoft.Xna.Framework;

namespace Engine.ListManager
{
    public static class GoodsListManager
    {
        public static ListType Type = 0;
        public static int MaxGoods = 223;
        public static int ListIndexBegin = 1;
        public static int ListIndexEnd = 223;
        public static int StoreIndexBegin = 1;
        public static int StoreIndexEnd = 198;
        public static int EquipIndexBegin = 201;
        public static int EquipIndexEnd = 207;
        public static int BottomIndexBegin = 221;
        public static int BottomIndexEnd = 223;
        private static readonly GoodsItemInfo[] GoodsList = new GoodsItemInfo[MaxGoods + 1];

        public enum ListType
        {
            TypeByGoodType, //store goods by type
            TypeByGoodItem //store goods by item
        }

        public static void InitIndex(IniData settings)
        {
            var cfg = settings.Sections["GoodsInit"];
            Type = (ListType) int.Parse(cfg["GoodsListType"]);
            StoreIndexBegin = int.Parse(cfg["StoreIndexBegin"]);
            StoreIndexEnd = int.Parse(cfg["StoreIndexEnd"]);
            EquipIndexBegin = int.Parse(cfg["EquipIndexBegin"]);
            EquipIndexEnd = int.Parse(cfg["EquipIndexEnd"]);
            BottomIndexBegin = int.Parse(cfg["BottomIndexBegin"]);
            BottomIndexEnd = int.Parse(cfg["BottomIndexEnd"]);
            MaxGoods = Math.Max(0, StoreIndexEnd);
            MaxGoods = Math.Max(MaxGoods, EquipIndexEnd);
            MaxGoods = Math.Max(MaxGoods, BottomIndexEnd);
            ListIndexEnd = MaxGoods;
        }

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
            return (index >= BottomIndexBegin && index <= BottomIndexEnd);
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

            foreach (var goodsItemInfo in GoodsList)
            {
                if (goodsItemInfo != null && goodsItemInfo.TheGood.Kind == Good.GoodKind.Equipment &&
                    goodsItemInfo.TheGood.NoNeedToEquip > 0)
                {
                    for (var i = 0; i < goodsItemInfo.Count; i++)
                    {
                        player.Equiping(goodsItemInfo.TheGood, null, true);
                    }
                }
            }
        }

        public static void UnEquipAllEquipWithoutTakeOff(Player player)
        {
            if (player == null) return;
            for (var i = EquipIndexBegin; i <= EquipIndexEnd; i++)
            {
                player.UnEquiping(Get(i));
            }
        }

        public static void ClearAllGoods(Player player)
        {
            UnEquipAllEquipWithoutTakeOff(player);
            RenewList();
            GuiManager.UpdateGoodsView();
        }

        /// <summary>
        /// Is magic learned from equiping equip?
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static bool IsMagicInEquipedEquip(string fileName)
        {
            for (var i = EquipIndexBegin; i <= EquipIndexEnd; i++)
            {
                var good = Get(i);
                if (good != null && Utils.EqualNoCase(good.MagicIniWhenUse, fileName))
                {
                    return true;
                }
            }
            return false;
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

        public static bool HasFreeItemSpace()
        {
            for (var i = StoreIndexBegin; i <= StoreIndexEnd; i++)
            {
                var info = GoodsList[i];
                if (info == null)
                {
                    return true;
                }
            }

            for (var i = BottomIndexBegin; i <= BottomIndexEnd; i++)
            {
                var info = GoodsList[i];
                if (info == null)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool AddGoodToList(string fileName)
        {
            int i;
            Good g;
            return AddGoodToList(fileName, out i, out g);
        }

        private static void CheckAddNoEquipGood(Good good)
        {
            if (good.Kind == Good.GoodKind.Equipment && good.NoNeedToEquip > 0 && Globals.ThePlayer != null)
            {
                Globals.ThePlayer.Equiping(good, null, false);
            }
        }

        public static bool AddGoodToList(string fileName, out int index, out Good outGood)
        {
            index = -1;
            outGood = null;
            switch (Type)
            {
                case ListType.TypeByGoodType:
                {
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
                                CheckAddNoEquipGood(outGood);
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
                            CheckAddNoEquipGood(outGood);
                            return true;
                        }
                    }
                }
                    break;
                case ListType.TypeByGoodItem:
                {
                    for (var i = StoreIndexBegin; i <= StoreIndexEnd; i++)
                    {
                        var info = GoodsList[i];
                        if (info == null)
                        {
                            GoodsList[i] = new GoodsItemInfo(fileName, 1);
                            index = i;
                            outGood = GoodsList[i].TheGood;
                            CheckAddNoEquipGood(outGood);
                            return true;
                        }
                    }

                    for (var i = BottomIndexBegin; i <= BottomIndexEnd; i++)
                    {
                        var info = GoodsList[i];
                        if (info == null)
                        {
                            GoodsList[i] = new GoodsItemInfo(fileName, 1);
                            index = i;
                            outGood = GoodsList[i].TheGood;
                            CheckAddNoEquipGood(outGood);
                            return true;
                        }
                    }
                }
                    break;
                default:
                    Log.LogMessage(@"GoodsListType 设置错误，请检查Content\ui\UI_Settings.ini文件");
                    return false;
            }

            GuiManager.ShowMessage("物品栏已满");

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
            switch (Type)
            {
                case ListType.TypeByGoodType:
                {
                    var info = GetGoodsItemInfoFromFileName(fileName);
                    if (info != null)
                    {
                        return info.Count;
                    }

                    return 0;
                }
                case ListType.TypeByGoodItem:
                {
                    var count = 0;
                    for (var i = ListIndexBegin; i <= ListIndexEnd; i++)
                    {
                        var info = GoodsList[i];
                        if (info != null && info.TheGood != null)
                        {
                            if (Utils.EqualNoCase(info.TheGood.FileName, fileName))
                            {
                                count++;
                            }
                        }
                    }

                    return count;
                }
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
                else if (good.Kind == Good.GoodKind.Equipment && good.NoNeedToEquip > 0)
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

        public static bool DeleteGood(string fileName, int amount)
        {
            if (amount <= 0) return false;
            switch (Type)
            {
                case ListType.TypeByGoodType:
                {
                    for (var i = ListIndexBegin; i <= ListIndexEnd; i++)
                    {
                        var info = GoodsList[i];
                        if (info != null && info.TheGood != null)
                        {
                            if (Utils.EqualNoCase(info.TheGood.FileName, fileName))
                            {
                                if (info.Count < amount)
                                {
                                    return false;
                                }
                                info.Count -= amount;
                                if (info.Count == 0)
                                {
                                    GoodsList[i] = null;
                                }
                                GuiManager.UpdateGoodItemView(i);
                                return true;
                            }
                        }
                    }
                }
                    break;
                case ListType.TypeByGoodItem:
                {
                    var indexToDelete = new List<int>();
                    for (var i = ListIndexBegin; i <= ListIndexEnd; i++)
                    {
                        var info = GoodsList[i];
                        if (info != null && info.TheGood != null)
                        {
                            if (Utils.EqualNoCase(info.TheGood.FileName, fileName))
                            {
                                indexToDelete.Add(i);
                                amount--;
                                if (amount == 0)
                                {
                                    break;
                                }
                            }
                        }
                    }

                    if (amount == 0)
                    {
                        foreach (var i in indexToDelete)
                        {
                            GoodsList[i] = null;
                            GuiManager.UpdateGoodItemView(i);
                        }
                        return true;
                    }
                }
                    break;
                default:
                    Log.LogMessage(@"GoodsListType 设置错误，请检查Content\ui\UI_Settings.ini文件");
                    return false;
            }
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
            if (good.User != null && good.User.Length > 0)
            {
                if (!good.User.Contains(Globals.ThePlayer.Name))
                {
                    //Current player can't use this good
                    GuiManager.ShowMessage("使用者：" + string.Join("，", good.User));
                    return;
                }
            }
            if (good.MinUserLevel > 0 && Globals.ThePlayer.Level < good.MinUserLevel)
            {
                GuiManager.ShowMessage("需要等级" + good.MinUserLevel);
                return;
            }
            if (good != null)
            {
                switch (good.Kind)
                {
                    case Good.GoodKind.Drug:
                        {
                            if (info.RemainColdMilliseconds > 0)
                            {
                                GuiManager.ShowMessage("该物品尚未冷却");
                                return;
                            }

                            if (info.TheGood.ColdMilliSeconds > 0)
                            {
                                info.RemainColdMilliseconds = info.TheGood.ColdMilliSeconds;
                            }

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
                        if (good.NoNeedToEquip == 0)
                        {
                            GuiManager.EquipInterface.EquipGood(goodIndex);
                        }
                        break;
                    case Good.GoodKind.Event:
                        {
                            ScriptManager.RunScript(Utils.GetScriptParser(
                                good.Script, null, Utils.ScriptCategory.Good), good);
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

        public static void Update(GameTime gameTime)
        {
            if(GoodsList != null)
            {
                var t = (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                foreach (var info in GoodsList)
                {
                    if (info != null && info.RemainColdMilliseconds > 0)
                    {
                        info.RemainColdMilliseconds -= t;
                    }
                }
            }
        }

        public class GoodsItemInfo
        {
            public Good TheGood;
            public int Count;
            private float _remainColdMilliseconds;
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

            public GoodsItemInfo(string fileName, int count)
            {
                var good = Utils.GetGood(fileName);
                if (good != null) TheGood = good;
                Count = count;
            }

            public GoodsItemInfo(Good good, int count)
            {
                TheGood = good;
                Count = count;
            }
        }
    }
}