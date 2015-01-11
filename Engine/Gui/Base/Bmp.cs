using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Gui.Base
{
    public sealed class Bmp : GuiItem
    {
        private string _bmpFilePath;

        public string BmpFilePath
        {
            get { return _bmpFilePath; }
            set
            {
                _bmpFilePath = value;
                if (string.IsNullOrEmpty(value))
                {
                    BaseTexture = null;
                }
                else
                {
                    BaseTexture = new Texture(new Asf(Utils.LoadTexture2DFromFile(value)));
                }
            }
        }

        public Bmp(GuiItem parent, Vector2 position, string filePath, int drawWidth, int drawHeight)
            :base(parent, position, drawWidth, drawHeight, null)
        {
            IsShow = true;
            BmpFilePath = filePath;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if(!IsShow) return;
            if (BaseTexture != null)
            {
                BaseTexture.Draw(spriteBatch, ScreenPosition, Width, Height);
            }
        }
    }
}