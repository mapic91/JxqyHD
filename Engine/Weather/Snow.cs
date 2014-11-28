using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Weather
{
    public static class Snow
    {
        private static readonly LinkedList<SnowFlake> SnowFlakes = new LinkedList<SnowFlake>();
        private static bool _isShow;
        private static float _elepsedMilliSeconds;
        private const float IntervalMilliSeconds = 300f;
        private const float Speed = 100;
        private static Asf[] _snowFlakeTexture;

        private static void GenerateSnowFlakes()
        {
            if (_snowFlakeTexture == null)
            {
                _snowFlakeTexture = TextureGenerator.GetSnowFlake();
            }
            var offX = Globals.TheCarmera.ViewBeginX;
            var offY = Globals.TheCarmera.ViewBeginY;
            for (var i = 0; i < Globals.WindowWidth; i += 50)
            {
                var direction = new Vector2(Globals.TheRandom.Next(-10, 11), 10);
                var snowFlake = new SnowFlake(new Vector2(i + offX, offY),
                    direction,
                    Speed * Globals.TheRandom.Next(1, 4),
                    _snowFlakeTexture[Globals.TheRandom.Next(0, 4)]);
                SnowFlakes.AddLast(snowFlake);
            }
        }

        public static void Show(bool isShow)
        {
            SnowFlakes.Clear();
            _isShow = isShow;
        }

        public static void Update(GameTime gameTime)
        {
            if(!_isShow) return;
            _elepsedMilliSeconds += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (_elepsedMilliSeconds >= IntervalMilliSeconds)
            {
                _elepsedMilliSeconds = 0;
                GenerateSnowFlakes();
            }
            var xBound = Globals.WindowWidth;
            var yBound = Globals.WindowHeight;
            for (var node = SnowFlakes.First; node != null;)
            {
                var next = node.Next;
                var snowFlake = node.Value;
                snowFlake.Update(gameTime);
                var position = Globals.TheCarmera.ToViewPosition(snowFlake.PositionInWorld);
                if (snowFlake.MovedYDistance >= yBound)
                {
                    SnowFlakes.Remove(node);
                }
                else
                {
                    if (position.X > xBound)
                    {
                        //Move to left view
                        position.X %= xBound;
                    }
                    else if(position.X < 0)
                    {
                        //Move to right view
                        position.X %= xBound;
                        position.X += xBound;
                    }
                    if (position.Y > yBound)
                    {
                        //Move to top view
                        position.Y %= yBound;
                    }
                    else if (position.Y < 0)
                    {
                        //Move to bottom view
                        position.Y %= yBound;
                        position.Y += yBound;
                    }
                }
                snowFlake.PositionInWorld = Globals.TheCarmera.ToWorldPosition(position);
                node = next;
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            if (!_isShow) return;
            foreach (var snowFlake in SnowFlakes)
            {
                snowFlake.Draw(spriteBatch);
            }
        }
    }
}