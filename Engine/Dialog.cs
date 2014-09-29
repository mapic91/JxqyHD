using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine
{
    public class Dialog : IScreen
    {
        private Texture2D _texture;
        private Vector2 _position = new Vector2(0f);
        private TextDrawer _textDrawer;
        private int _textOffX, _textOffY;
        private bool _isShown = true;

        public Dialog(Texture2D texture2D, Vector2 position, int textOffX, int textOffY)
        {
            Texture = texture2D;
            Position = position;
            _textDrawer = new TextDrawer();
            TextOffX = textOffX;
            TextOffY = textOffY;
        }

        public int TextOffX
        {
            get { return _textOffX; }
            set { _textOffX = value; }
        }

        public int TextOffY
        {
            get { return _textOffY; }
            set { _textOffY = value; }
        }

        public Rectangle TextRegion
        {
            get { return OffsetRegion(TextOffX, TextOffY); }
        }

        public bool IsShown
        {
            get { return _isShown; }
            set { _isShown = value; }
        }

        public Texture2D Texture
        {
            get { return _texture; }
            set { _texture = value; }
        }

        public Vector2 Position
        {
            get { return _position; }
            set { _position = value; }
        }

        public int Width
        {
            get { return Texture != null ? Texture.Width : 0; }
        }

        public int Height
        {
            get { return Texture != null ? Texture.Height : 0; }
        }

        public Vector2 Size
        {
            get { return new Vector2(Width, Height);}
        }

        public Rectangle Region
        {
            get { return new Rectangle((int)Position.X, (int)Position.Y, Width, Height);}
        }

        public Rectangle OffsetRegion(int offsetX, int offsetY)
        {
            return new Rectangle((int)Position.X + offsetX, (int)Position.Y + offsetY, Width - 2*offsetX, Height - 2*offsetY);
        }

        public Rectangle OffsetRegion(int offset)
        {
            return OffsetRegion(offset, offset);
        }

        public void DisplayText(SpriteFont font, string text)
        {
            DisplayText(font, text, Color.Black);
        }

        public void DisplayText(SpriteFont font, string text, Color color)
        {
            if (_textDrawer != null)
            {
                _textDrawer.DrawString(font, text, TextRegion, color);
            }
        }

        public void Update(GameTime gameTime)
        {
            if (IsShown)
            {
                _textDrawer.Update(gameTime);
            }
        }

        public void Draw(SpriteBatch sprintBatch)
        {
            if (IsShown)
            {
                sprintBatch.Draw(Texture, Region, null, Color.White);
                _textDrawer.Draw(sprintBatch);
            }
            
        }
    }
}