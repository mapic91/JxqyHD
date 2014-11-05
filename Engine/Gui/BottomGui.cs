using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Gui
{
    public class BottomGui : GuiItem
    {
        private DragDropItem[] _items = new DragDropItem[8];

        public BottomGui()
        {
            BaseTexture = new Texture(Utils.GetAsf(@"asf\ui\bottom\window.asf"));
            Width = BaseTexture.Width;
            Height = BaseTexture.Height;
            Position = new Vector2(
                Globals.WindowWidth - BaseTexture.Width,
                Globals.WindowHeight - BaseTexture.Height);

            InitializeItems();
        }

        private void InitializeItems()
        {
            _items[0] = new DragDropItem(this, new Vector2(7, 20), 30, 40, null);
            _items[1] = new DragDropItem(this, new Vector2(44, 20), 30, 40, null);
            _items[2] = new DragDropItem(this, new Vector2(82, 20), 30, 40, null);
            _items[3] = new DragDropItem(this, new Vector2(199, 20), 30, 40, null);
            _items[4] = new DragDropItem(this, new Vector2(238, 20), 30, 40, null);
            _items[5] = new DragDropItem(this, new Vector2(277, 20), 30, 40, null);
            _items[6] = new DragDropItem(this, new Vector2(316, 20), 30, 40, null);
            _items[7] = new DragDropItem(this, new Vector2(354, 20), 30, 40, null);

            for (var i = 0; i < 3; i++)
            {
                _items[i].Drag += (arg1, arg2) =>
                {

                };
                _items[i].Drop += GoodsGui.DropHandler;
                _items[i].RightClick += (arg1, arg2) =>
                {

                };
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
                    if (info != null)
                    {
                        Globals.ThePlayer.CurrentMagicInUse = info;
                    }
                };
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

        public void SetItem(int index, Texture texture, object data)
        {
            if (index >= 0 && index < 8)
            {
                _items[index].BaseTexture = texture;
                _items[index].Data = data;
            }
        }

        public void UpdateMagicItems()
        {
            for (var i = 3; i < 8; i++)
            {
                var index = ToMagicListIndex(i);
                var magic = MagicListManager.Get(index);
                var icon = magic == null ? null : magic.Icon;
                SetItem(i, new Texture(icon), new MagicGui.MagicItemData(index));
            }
        }

        public void UpdateGoodsItems()
        {
            for (var i = 0; i < 3; i++)
            {
                var index = ToGoodsListIndex(i);
                var good = GoodsListManager.Get(index);
                var icon = good == null ? null : good.Icon;
                SetItem(i, new Texture(icon), new GoodsGui.GoodItemData(index));
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