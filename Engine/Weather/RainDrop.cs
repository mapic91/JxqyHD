using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Weather
{
    public class RainDrop
    {
        private bool _isShow;
        private Vector2 _position;
        private Texture2D _texture;
        public RainDrop(Vector2 positionAtWindow, Texture2D texture)
        {
            _position = positionAtWindow;
            _texture = texture;
        }

        public void Update()
        {
            _isShow = Globals.TheRandom.Next(0, 5) == 0;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (_isShow)
            {
                spriteBatch.Draw(_texture, _position, Color.White);
            }
        }
    }
}