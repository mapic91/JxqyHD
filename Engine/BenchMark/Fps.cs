using Microsoft.Xna.Framework;

namespace Engine.Benchmark
{
    public static class Fps
    {
        private static int _fpsValue = 60;

        public static int FpsValue
        {
            get { return _fpsValue; }
        }

        /// <summary>
        /// Update at draw frame.
        /// </summary>
        /// <param name="gameTime"></param>
        public static void Update(GameTime gameTime)
        {
            _fpsValue = (int)(1000.0 / gameTime.ElapsedGameTime.TotalMilliseconds);
        }
    }
}
