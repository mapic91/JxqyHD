using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Weather
{
    public static class WeatherManager
    {
        public static bool IsRaining { get { return Rain.IsRaining; } }
        public static bool IsSnowing { get { return Snow.IsSnowing; } }
        public static string RainFileName { get; set; }

        public static void ShowSnow(bool isShow)
        {
            Snow.Show(isShow);
        }

        public static void BeginRain(string fileName)
        {
            RainFileName = fileName;
            Rain.Raining(true);
        }

        public static void StopRain()
        {
            if(IsRaining) Rain.Raining(false);
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