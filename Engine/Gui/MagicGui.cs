using Engine.Gui.Base;
using Engine.ListManager;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Texture = Engine.Gui.Base.Texture;

namespace Engine.Gui
{
    public sealed class MagicGui : GuiItem
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

        public static void MouseRightClickdHandler(object arg1, MouseEvent arg2)
        {
            var item = (DragDropItem)arg1;
            var data = item.Data as MagicItemData;
            if (data != null)
            {
                var info = MagicListManager.GetItemInfo(data.Index);
                if (info != null)
                {
                    for (var i = MagicListManager.BottomIndexBegin;
                        i <= MagicListManager.BottomIndexEnd;
                        i++)
                    {
                        var binfo = MagicListManager.GetItemInfo(i);
                        if (binfo == null)
                        {
                            MagicListManager.ExchangeListItem(data.Index, i);
                            GuiManager.UpdateMagicView();
                            break;
                        }
                    }
                }
            }
        }

        public MagicGui()
        {
            var cfg = GuiManager.Setttings.Sections["Magics"];
            var baseTexture = new Texture(Utils.GetAsf(null, cfg["Image"]));
            var position = new Vector2(
                Globals.WindowWidth / 2f + int.Parse(cfg["LeftAdjust"]),
                0f + int.Parse(cfg["TopAdjust"]));
            _listView = new ListView(null,
                position,
                new Vector2(294, 108),
                baseTexture.Width,
                baseTexture.Height,
                baseTexture,
                (MagicListManager.StoreIndexEnd - MagicListManager.StoreIndexBegin + 1 + 2)/3,
                GuiManager.Setttings.Sections["Magics_List_Items"],
                cfg["ScrollBarButton"]);
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
            _listView.RegisterItemMouseRightClickeHandler(MouseRightClickdHandler);

            IsShow = false;
        }

        public void UpdateItems()
        {
            for (var i = 0; i < 9; i++)
            {
                var index = _listView.ToListIndex(i) + MagicListManager.StoreIndexBegin - 1;
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