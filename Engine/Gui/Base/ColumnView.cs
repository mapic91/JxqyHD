using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Gui.Base
{
    public class ColumnView : GuiItem
    {
        public float Percent { set; get; }

        public ColumnView(GuiItem parent, Vector2 position, int width, int height, Texture baseTexture, float percent = 1f)
            : base(parent, position, width, height, null)
        {
            Percent = percent;
            BaseTexture = baseTexture;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if(!IsShow) return;
            if (BaseTexture != null)
            {
                var texture = BaseTexture.CurrentTexture;
                if (texture != null)
                {
                    var trans = TextureGenerator.MakeTransparentFromTop(texture, Percent);
                    if (trans != null)
                    {
                        spriteBatch.Draw(trans, ScreenPosition, Color.White);
                    }
                }
            }
        }
    }
}