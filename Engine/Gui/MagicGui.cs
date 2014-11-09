using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Gui
{
    public class MagicGui : GuiItem
    {
        private ListView _listView;

        public static void DropHandler(object arg1, DragDropItem.DropEvent arg2)
        {
            var item = (DragDropItem) arg1;
            var sourceItem = arg2.Source;
            var data = item.Data as MagicItemData;
            var sourceData = sourceItem.Data as MagicItemData;
            if (data != null && sourceData != null)
            {
                MagicListManager.ExchangeListItem(data.Index, sourceData.Index);
                item.BaseTexture = MagicListManager.GetTexture(data.Index);
                sourceItem.BaseTexture = MagicListManager.GetTexture(sourceData.Index);
                if(data.Index == MagicListManager.XiuLianIndex || 
                    sourceData.Index == MagicListManager.XiuLianIndex)
                    GuiManager.XiuLianInterface.UpdateItem();
            }
        }

        public static void MouseStayOverHandler(object arg1, GuiItem.MouseEvent arg2)
        {
            var item = (DragDropItem)arg1;
            var data = item.Data as MagicItemData;
            if (data != null)
            {
                var info = MagicListManager.GetItemInfo(data.Index);
                if(info != null)
                    GuiManager.ToolTipInterface.ShowMagic(info.TheMagic);
            }
        }

        public static void MouseLeaveHandler(object arg1, GuiItem.MouseEvent arg2)
        {
            GuiManager.ToolTipInterface.IsShow = false;
        }

        public MagicGui()
        {
            IsShow = false;
            var baseTexture = new Texture(Utils.GetAsf(@"asf\ui\common\", "panel2.asf"));
            var position = new Vector2(
                Globals.WindowWidth / 2f,
                0f);
            _listView = new ListView(null,
                position,
                baseTexture.Width,
                baseTexture.Height,
                baseTexture,
                12);
            _listView.Scrolled += delegate
            {
                UpdateItems();
            };
            _listView.RegisterItemDragHandler((arg1, arg2) =>
            {

            });
            _listView.RegisterItemDropHandler(DropHandler);
            _listView.RegisterItemMouseStayOverHandler(MouseStayOverHandler);
            _listView.RegisterItemMouseLeaveHandler(MouseLeaveHandler);
        }

        public void UpdateItems()
        {
            for (var i = 0; i < 9; i++)
            {
                var index = _listView.ToListIndex(i);
                var magic = MagicListManager.Get(index);
                var image = (magic == null ? null : magic.Image);
                _listView.SetListItem(i, new Texture(image), new MagicItemData(index));
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsShow) return;

            _listView.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsShow) return;

            _listView.Draw(spriteBatch);
        }

        public class MagicItemData
        {
            public int Index { private set; get; }

            public MagicItemData(int index)
            {
                Index = index;
            }
        }
    }
}