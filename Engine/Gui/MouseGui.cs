using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Engine.Gui
{
    public class MouseGui : GuiItem
    {
        private GuiItem _drapImage;
        public MouseGui()
        {
            BaseTexture = new Texture(Utils.GetAsf(@"asf\ui\common\mouse.asf"));
            Width = 0;
            Height = 0;
            _drapImage = new GuiItem(this, Vector2.Zero, 0, 0, null);
        }

        public override void Update(GameTime gameTime)
        {
            if(!IsShow) return;
            base.Update(gameTime);
            var mouseState = Mouse.GetState();
            var screenPosition = new Vector2(mouseState.X, mouseState.Y);
            Position = screenPosition;
            var dragTexture = GuiManager.DragDropSourceTexture;
            if (dragTexture == null)
            {
                _drapImage.BaseTexture = null;
            }
            else
            {
                _drapImage.BaseTexture = dragTexture;
                _drapImage.Position = new Vector2(-dragTexture.Width/2f, -dragTexture.Height/2f);
            }
            _drapImage.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if(!IsShow)return;
            _drapImage.Draw(spriteBatch);
            base.Draw(spriteBatch);
        } 
    }
}