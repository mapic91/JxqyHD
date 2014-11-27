using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Weather
{
    public static class WeatherManager
    {
        public static void ShowSnow(bool isShow)
        {
            Snow.Show(isShow);
        }

        public static void Update(GameTime gameTime)
        {
            Snow.Update(gameTime);
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            Snow.Draw(spriteBatch);
        }
    }
}