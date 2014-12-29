using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Engine.Benchmark
{
    public static class Fps
    {
        private static int _frames;
        private static float _elapsedMilliseconds;
        private static int _updtaeIntervalMilliSecond = 160;
        private static int _fpsValue = 60;

        /// <summary>
        /// Interval to update fps in milliseconds.
        /// </summary>
        public static int UpdateIntervalMilliSecond
        {
            get { return _updtaeIntervalMilliSecond; }
            set { _updtaeIntervalMilliSecond = value > 0 ? value : 1000; }
        }

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
            _elapsedMilliseconds += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            _frames++;
            if (_elapsedMilliseconds >= UpdateIntervalMilliSecond)
            {
                _fpsValue = (int)(_frames/((double) _elapsedMilliseconds/1000));
                _elapsedMilliseconds -= UpdateIntervalMilliSecond;
                _frames = 0;
            }
        }
    }
}
