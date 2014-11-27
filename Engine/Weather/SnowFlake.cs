using Microsoft.Xna.Framework;

namespace Engine.Weather
{
    public class SnowFlake : Sprite
    {
        private Vector2 _direction;
        public float MovedYDistance;
        public SnowFlake(Vector2 positionInWorld, Vector2 direction, float velocity, Asf texture)
            :base(positionInWorld, velocity, texture)
        {
            _direction = direction;
            if(_direction != Vector2.Zero) _direction.Normalize();
        }

        public override void Update(GameTime gameTime)
        {
            var elapsedSeconds = (float) gameTime.ElapsedGameTime.TotalSeconds;
            MoveTo(_direction, elapsedSeconds);
            MovedYDistance += Velocity*_direction.Y*elapsedSeconds;
        }
    }
}