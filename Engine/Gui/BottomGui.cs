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

            for (var i = 3; i < 8; i++)
            {
                _items[i].Drag += (arg1, arg2) =>
                {
                    var item = (DragDropItem) arg1;
                    item.BaseTexture = null;
                };
                _items[i].Drop += (object arg1, DragDropItem.DropEvent arg2) =>
                {
                    var data = (MagicGui.MagicItemData) (((DragDropItem) arg1).Data);
                    var sourceData = arg2.Source.Data;
                    if (sourceData is MagicGui.MagicItemData)
                    {
                        var magicItemData = (MagicGui.MagicItemData) sourceData;
                        MagicListManager.ExchangeMagicListItem(data.Index, magicItemData.Index);
                    }
                };
                _items[i].RightClick += (arg1, arg2) =>
                {
                    var data = (MagicGui.MagicItemData) (((DragDropItem) arg1).Data);
                    var info = MagicListManager.GetMagicItemInfo(data.Index);
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
                var magic = MagicListManager.GetMagic(index);
                var image = magic == null ? null : magic.Icon;
                SetItem(i, new Texture(image), new MagicGui.MagicItemData(index));
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