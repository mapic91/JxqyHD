using System;
using System.Collections.Generic;
using IniParser;

namespace Engine.Gui
{
    public static class GoodsListManager
    {
        private const int MaxGoods = 223;
        private static readonly Dictionary<int, GoodsItemInfo> GoodsList = new Dictionary<int, GoodsItemInfo>();

        public static void RenewList()
        {
            for (var i = 1; i <= MaxGoods; i++)
            {
                GoodsList[i] = null;
            }
        }

        public static bool IndexInRange(int index)
        {
            return (index > 0 && index <= MaxGoods);
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

        public static void ExchangeListItem(int index1, int index2)
        {
            if (index1 != index2 &&
                IndexInRange(index1) &&
                IndexInRange(index2))
            {
                var temp = GoodsList[index1];
                GoodsList[index1] = GoodsList[index2];
                GoodsList[index2] = temp;
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
                if (index >= 221 && index <= 223)
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

            public GoodsItemInfo(string goodFilePath, int count)
            {
                var good = Utils.GetGood(goodFilePath);
                if(good != null)TheGood = good;
                Count = count;
            }
        }
    }
}