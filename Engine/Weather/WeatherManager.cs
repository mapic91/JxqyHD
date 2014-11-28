using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Weather
{
    public static class WeatherManager
    {
        public static bool IsRain { get { return Rain.IsRain; } }

        public static void ShowSnow(bool isShow)
        {
            Snow.Show(isShow);
        }

        public static void BeginRain(string fileName)
        {
            Rain.Raining(true);
        }

        public static void StopRain()
        {
            Rain.Raining(false);
        }

        public static void Update(GameTime gameTime)
        {
            Rain.Update(gameTime);
            Snow.Update(gameTime);
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            Rain.Draw(spriteBatch);
            Snow.Draw(spriteBatch);
        }
    }
}