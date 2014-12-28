using Engine.Gui.Base;
using Engine.ListManager;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Texture = Engine.Gui.Base.Texture;

namespace Engine.Gui
{
    public class BottomGui : GuiItem
    {
        private DragDropItem[] _items = new DragDropItem[8];

        public BottomGui()
        {
            BaseTexture = new Texture(Utils.GetAsf(@"asf\ui\bottom\", "window.asf"));
            Width = BaseTexture.Width;
            Height = BaseTexture.Height;
            Position = new Vector2(
                (Globals.WindowWidth - BaseTexture.Width)/2f + 102,
                Globals.WindowHeight - BaseTexture.Height);

            InitializeItems();
        }

        private void InitializeItems()
        {
            _items[0] = new DragDropItem(this, new Vector2(7, 20), 30, 40, null, new GoodsGui.GoodItemData(221));
            _items[1] = new DragDropItem(this, new Vector2(44, 20), 30, 40, null, new GoodsGui.GoodItemData(222));
            _items[2] = new DragDropItem(this, new Vector2(82, 20), 30, 40, null, new GoodsGui.GoodItemData(223));
            _items[3] = new DragDropItem(this, new Vector2(199, 20), 30, 40, null, new MagicGui.MagicItemData(40));
            _items[4] = new DragDropItem(this, new Vector2(238, 20), 30, 40, null, new MagicGui.MagicItemData(41));
            _items[5] = new DragDropItem(this, new Vector2(277, 20), 30, 40, null, new MagicGui.MagicItemData(42));
            _items[6] = new DragDropItem(this, new Vector2(316, 20), 30, 40, null, new MagicGui.MagicItemData(43));
            _items[7] = new DragDropItem(this, new Vector2(354, 20), 30, 40, null, new MagicGui.MagicItemData(44));

            for (var i = 0; i < 3; i++)
            {
                _items[i].Drag += (arg1, arg2) =>
                {

                };
                _items[i].Drop += GoodsGui.DropHandler;
                _items[i].RightClick += GoodsGui.RightClickHandler;
                _items[i].MouseStayOver += GoodsGui.MouseStayOverHandler;
                _items[i].MouseLeave += GoodsGui.MouseLeaveHandler;
            }

            for (var i = 3; i < 8; i++)
            {
                _items[i].Drag += (arg1, arg2) =>
                {

                };
                _items[i].Drop += MagicGui.DropHandler;
                _items[i].RightClick += (arg1, arg2) =>
                {
                    var data = (MagicGui.MagicItemData)(((DragDropItem)arg1).Data);
                    var info = MagicListManager.GetItemInfo(data.Index);
                    Globals.ThePlayer.CurrentMagicInUse = info;
                };
                _items[i].MouseStayOver += MagicGui.MouseStayOverHandler;
                _items[i].MouseLeave += MagicGui.MouseLeaveHandler;
            }
        }

        public int ToMagicListIndex(int itemIndex)
        {
            return itemIndex + 37;
        }

        public int ToGoodsListIndex(int itemIndex)
        {
            return itemIndex + 221;
        }

        public void SetItem(int index, Texture texture, string topLeftText = "")
        {
            if (index >= 0 && index < 8)
            {
                _items[index].BaseTexture = texture;
                _items[index].TopLeftText = topLeftText;
            }
        }

        public void UpdateMagicItems()
        {
            for (var i = 3; i < 8; i++)
            {
                UpdateItem(i);
            }
        }

        public void UpdateGoodsItems()
        {
            for (var i = 0; i < 3; i++)
            {
                UpdateItem(i);
            }
        }

        public void UpdateGoodItem(int listIndex)
        {
            UpdateItem(listIndex - GoodsListManager.BottomGoodsIndexBegin);
        }

        public void UpdateItem(int itemIndex)
        {
            if (itemIndex >= 0 && itemIndex < 3)//Goods
            {
                var index = ToGoodsListIndex(itemIndex);
                var info = GoodsListManager.GetItemInfo(index);
                Good good = null;
                var text = "";
                if (info != null)
                {
                    good = info.TheGood;
                    text = info.Count.ToString();
                }
                var icon = good == null ? null : good.Icon;
                SetItem(itemIndex, new Texture(icon), text);
            }

            if (itemIndex >= 3 && itemIndex < 8)//Magic
            {
                var index = ToMagicListIndex(itemIndex);
                var magic = MagicListManager.Get(index);
                var icon = magic == null ? null : magic.Icon;
                SetItem(itemIndex, new Texture(icon));
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsShow) return;

            base.Update(gameTime);
            foreach (var dragDropItem in _items)
            {
                dragDropItem.Update(gameTime);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsShow) return;

            base.Draw(spriteBatch);
            foreach (var dragDropItem in _items)
            {
                dragDropItem.Draw(spriteBatch);
            }
        }
    }
}