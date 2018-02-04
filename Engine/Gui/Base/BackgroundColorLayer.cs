using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Gui.Base
{
    public class BackgroundColorLayer : GuiItem
    {
        public Vector2 Anchor;

        public BackgroundColorLayer(
            GuiItem parent,
            Vector2 position,
            Vector2 anchor,
            int width,
            int height,
            Color color)
        :base(parent, position, width, height, null)
        {
            BaseTexture = new Texture(new Asf(TextureGenerator.GetColorTexture(color, 1, 1)));
            Anchor = anchor;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsShow) return;

            if (BaseTexture != null && Width > 0 && Height > 0)
            {
                var position = ScreenPosition - new Vector2(Anchor.X * Width, Anchor.Y * Height);
                BaseTexture.Draw(spriteBatch, position, Width, Height);
            }
        }
    }
}
