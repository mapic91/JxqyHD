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
        }

        private void SetItem(int index, Texture texture)
        {
            if (index >= 0 && index < 8)
                _items[index].BaseTexture = texture;
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