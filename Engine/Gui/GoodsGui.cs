using System;
using Engine.ListManager;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Gui
{
    public class GoodsGui : GuiItem
    {
        private ListView _listView;
        private TextGui _money;

        public static void DropHandler(object arg1, DragDropItem.DropEvent arg2)
        {
            var item = (DragDropItem)arg1;
            var sourceItem = arg2.Source;
            var data = item.Data as GoodItemData;
            var sourceData = sourceItem.Data as GoodItemData;
            if (data != null && sourceData != null)
            {
                if (GoodsListManager.IsInEquipRange(sourceData.Index))
                {
                    var info = GoodsListManager.GetItemInfo(data.Index);
                    var sourceGood = GoodsListManager.Get(sourceData.Index);
                    if (sourceGood == null ||
                        (info != null && info.TheGood == null) ||
                        (info != null && info.TheGood.Part != sourceGood.Part))
                    {
                        return;
                    }
                }
            }

            int index, sourceIndex;
            ExchangeItem(arg1, arg2, out index, out sourceIndex);
        }

        public static void MouseStayOverHandler(object arg1, GuiItem.MouseEvent arg2)
        {
            var item = (DragDropItem)arg1;
            var data = item.Data as GoodItemData;
            if (data != null)
            {
                var info = GoodsListManager.GetItemInfo(data.Index);
                if (info != null)
                    GuiManager.ToolTipInterface.ShowGood(info.TheGood);
            }
        }

        public static void MouseLeaveHandler(object arg1, GuiItem.MouseEvent arg2)
        {
            GuiManager.ToolTipInterface.IsShow = false;
        }

        public static void RightClickHandler(object arg1, GuiItem.MouseRightClickEvent arg2)
        {
            var theItem = (DragDropItem)arg1;
            var data = theItem.Data as GoodItemData;
            if (data != null)
            {
                var good = GoodsListManager.Get(data.Index);
                if (good != null)
                {
                    switch (good.Kind)
                    {
                        case Good.GoodKind.Drug:
                        case Good.GoodKind.Event:
                            GoodsListManager.UsingGood(data.Index);
                            break;
                        case Good.GoodKind.Equipment:
                            GuiManager.EquipInterface.EquipGood(data.Index);
                            break;
                    }
                }
            }
        }

        public static bool ExchangeItem(object arg1, DragDropItem.DropEvent arg2, 
            out int index, out int sourceIndex)
        {
            var item = (DragDropItem)arg1;
            var sourceItem = arg2.Source;
            var data = item.Data as GoodItemData;
            var sourceData = sourceItem.Data as GoodItemData;
            if (data != null && sourceData != null)
            {
                GoodsListManager.ExchangeListItemAndEquiping(data.Index, sourceData.Index);
                item.BaseTexture = GoodsListManager.GetTexture(data.Index);
                sourceItem.BaseTexture = GoodsListManager.GetTexture(sourceData.Index);
                index = data.Index;
                sourceIndex = sourceData.Index;
                var info = GoodsListManager.GetItemInfo(index);
                var sourceInfo = GoodsListManager.GetItemInfo(sourceIndex);
                item.TopLeftText = info != null ? info.Count.ToString() : "";
                sourceItem.TopLeftText = sourceInfo != null ? sourceInfo.Count.ToString() : "";
                return true;
            }
            index = 0;
            sourceIndex = 0;
            return false;
        }

        public GoodsGui()
        {
            IsShow = false;
            var baseTexture = new Texture(Utils.GetAsf(@"asf\ui\common\", "panel3.asf"));
            var position = new Vector2(
                Globals.WindowWidth / 2f,
                0f);
            _listView = new ListView(null,
                position,
                new Vector2(308, 110),
                baseTexture.Width,
                baseTexture.Height,
                baseTexture,
                66);
            _listView.Scrolled += delegate
            {
                UpdateItems();
            };
            _listView.RegisterItemDragHandler((arg1, arg2) =>
            {
                
            });
            _listView.RegisterItemDropHandler(DropHandler);
            _listView.RegisterItemMouseRightClickeHandler(RightClickHandler);
            _listView.RegisterItemMouseStayOverHandler(MouseStayOverHandler);
            _listView.RegisterItemMouseLeaveHandler(MouseLeaveHandler);
            _money = new TextGui(_listView,
                new Vector2(137, 363),
                100,
                12,
                Globals.FontSize7,
                0,
                0,
                "",
                Color.White*0.8f);
        }

        public bool IsItemShow(int listIndex, out int index)
        {
            return _listView.IsItemShow(listIndex, out index);
        }

        public void UpdateItems()
        {
            for (var i = 0; i < 9; i++)
            {
                var index = _listView.ToListIndex(i);
                var info = GoodsListManager.GetItemInfo(index);
                Good good = null;
                if (info != null) good = info.TheGood;
                var image = (good == null ? null : good.Image);
                _listView.SetListItem(i, new Texture(image), new GoodItemData(index));
                _listView.SetItemTopLeftText(i, info != null ? info.Count.ToString() : "");
            }
        }

        public void UpdateListItem(int listIndex)
        {
            int itemIndex;
            if (IsItemShow(listIndex, out itemIndex))
            {
                _listView.SetListItemTexture(itemIndex,
                    GoodsListManager.GetTexture(listIndex));
                var info = GoodsListManager.GetItemInfo(listIndex);
                _listView.SetItemTopLeftText(itemIndex, info != null ? info.Count.ToString() : "");
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsShow) return;

            _listView.Update(gameTime);
            if (Globals.ThePlayer != null)
            {
                _money.Text = Globals.ThePlayer.Money.ToString();
            }
            else
            {
                _money.Text = "";
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsShow) return;

            _listView.Draw(spriteBatch);
            _money.Draw(spriteBatch);
        }

        public class GoodItemData
        {
            public int Index { private set; get; }
            public GoodItemData(int index)
            {
                Index = index;
            }
        }
    }
}