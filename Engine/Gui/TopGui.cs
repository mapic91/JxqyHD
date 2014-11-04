using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Gui
{
    public class TopGui : GuiItem
    {
        private GuiItem[] _buttons = new GuiItem[7];
        public TopGui()
        {
            BaseTexture = new Texture(Utils.GetAsf(@"asf\ui\top\window.asf"));
            Width = BaseTexture.Width;
            Height = BaseTexture.Height;
            Position = new Vector2((Globals.WindowWidth - BaseTexture.Width) / 2f, 0f);
            InitializeItems();
        }

        private void InitializeItems()
        {
            string[] paths =
            {
                @"asf\ui\top\BtnState.asf",
                @"asf\ui\top\BtnEquip.asf",
                @"asf\ui\top\BtnXiuLian.asf",
                @"asf\ui\top\BtnGoods.asf",
                @"asf\ui\top\BtnMagic.asf",
                @"asf\ui\top\BtnNotes.asf",
                @"asf\ui\top\BtnOption.asf"
            };
            Vector2[] position =
            {
                new Vector2(52, 0),
                new Vector2(80, 0),
                new Vector2(107, 0),
                new Vector2(135, 0),
                new Vector2(162, 0),
                new Vector2(189, 0),
                new Vector2(216, 0)
            };
            var clickedSound = Utils.GetSoundEffect("界-大按钮.wav");
            for (var i = 0; i < 7; i++)
            {
                var asf = Utils.GetAsf(paths[i]);
                var baseTexture = new Texture(asf, 0, 1);
                var clickedTexture = new Texture(asf, 1, 1);
                _buttons[i] = new GuiItem(this, 
                position[i],
                baseTexture.Width,
                baseTexture.Height,
                baseTexture,
                null,
                clickedTexture,
                null,
                clickedSound);
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            foreach (var button in _buttons)
            {
                button.Update(gameTime);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            foreach (var button in _buttons)
            {
                button.Draw(spriteBatch);
            }
        }
    }
}