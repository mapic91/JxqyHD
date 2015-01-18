using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Gui.Base
{
    public class LineText : GuiItem
    {
        public Color DrawColor = Color.Black;
        public Align TextAlign = Align.Left;
        public string Text;
        public SpriteFont Font;

        public LineText(GuiItem parent, 
            Vector2 position, 
            int width, 
            int height, 
            Align textAlign, 
            string text, 
            Color color, 
            SpriteFont font)
            :base(parent, position, width, height, null)
        {
            TextAlign = textAlign;
            Text = text;
            DrawColor = color;
            Font = font;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if(!IsShow || string.IsNullOrEmpty(Text) || Font == null) return;
            base.Draw(spriteBatch);
            var textSize = Font.MeasureString(Text);
            var drawPosition = ScreenPosition;
            switch (TextAlign)
            {
                case Align.Left:
                    break;
                case Align.Center:
                    drawPosition.X += (Width - textSize.X)/2;
                    break;
                case Align.Right:
                    drawPosition.X += Width - textSize.X;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            spriteBatch.DrawString(Font, Text, drawPosition, DrawColor);
        }

        public enum Align
        {
            Left,
            Center,
            Right
        }
    }
}