using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine
{
    public class Sprite : IMoveControlInWorld
    {
        private Vector2 _positionInWorld;
        private int _velocity;
        private int _currentFrameIndex;
        private int _frameBegin;
        private int _frameEnd;
        private int _elapsedMilliSecond;
        private int _currentDirection;
        private Asf _texture;

        public Sprite(Vector2 positionInWorld, int velocity, Asf texture, int direction = 0)
        {
            PositionInWorld = positionInWorld;
            Velocity = velocity;
            Texture = texture;
            CurrentDirection = direction;
        }
        #region Properties
        public Asf Texture
        {
            get { return _texture; }
            set
            {
                _texture = value;
                _elapsedMilliSecond = 0;
                CurrentDirection = CurrentDirection;
                CurrentFrameIndex = CurrentFrameIndex;
            }
        }

        public int CurrentDirection
        {
            get { return _currentDirection; }
            private set
            {
                _currentDirection = value % (_texture.DirectionCounts == 0 ? 1 : _texture.DirectionCounts);
                _frameBegin = _currentDirection*_texture.FrameCountsPerDirection;
                _frameEnd = _frameBegin + _texture.FrameCountsPerDirection - 1;
            }
        }

        public int CurrentFrameIndex
        {
            get { return _currentFrameIndex; }
            set
            {
                _currentFrameIndex = value;
                if (_currentFrameIndex > _frameEnd)
                    _currentFrameIndex = _frameBegin;
                else if (_currentFrameIndex < _frameBegin)
                    _currentFrameIndex = _frameEnd;
            }
        }

        public int Velocity
        {
            get { return _velocity; }
            set { _velocity = value; }
        }

        public Vector2 PositionInWorld
        {
            get { return _positionInWorld; }
            set { _positionInWorld = value; }
        }

        public int Width
        {
            get
            {
                if (_texture != null) return _texture.Width;
                else return 0;
            }
        }
        public int Height
        {
            get
            {
                if (_texture != null) return _texture.Height;
                else return 0;
            }
        }

        public Vector2 Size
        {
            get { return new Vector2(Width, Height); }
        }

        public Rectangle RegionInWorld
        {
            get
            {
                return new Rectangle((int)PositionInWorld.X, (int)PositionInWorld.Y, Width, Height);
            }
        }
        #endregion Properties

        public void MoveTo(Vector2 direction, float elapsedSeconds)
        {
            if (direction != Vector2.Zero)
            {
                SetDirection(direction);
                direction.Normalize();
                _positionInWorld += direction*_velocity*elapsedSeconds;
            }
        }

        public void Update(GameTime gameTime, Vector2 direction)
        {
            MoveTo(direction, (float)gameTime.ElapsedGameTime.TotalSeconds);
            _elapsedMilliSecond += (int)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (_elapsedMilliSecond > _texture.Interval)
            {
                _elapsedMilliSecond -= _texture.Interval;
                CurrentFrameIndex++;
            }
        }

        private void SetDirection(Vector2 direction)
        {
            if (direction == Vector2.Zero) return;
            direction.Normalize();
            var angle = Math.Acos(Vector2.Dot(direction, new Vector2(1, 0)));
            if (angle >= 0 && angle < Math.PI/8f)
                CurrentDirection = 6;
            else if (angle >= Math.PI/8f && angle < Math.PI*3f/8f)
                CurrentDirection = direction.Y <= 0 ? 5 : 7;
            else if (angle >= Math.PI*3f/8f && angle < Math.PI*5f/8f)
                CurrentDirection = direction.Y <= 0 ? 4 : 0;
            else if (angle >= Math.PI*5f/8f && angle < Math.PI*7f/8f)
                CurrentDirection = direction.Y <= 0 ? 3 : 1;
            else if (angle >= Math.PI*7f/8f && angle <= Math.PI)
                CurrentDirection = 2;

        }

        public void Draw(SpriteBatch spriteBatch, Carmera cam)
        {
            var texture = _texture.GetFrame(CurrentFrameIndex);
            if(texture == null) return;

            Rectangle des =
                cam.ToViewRegion(new Rectangle((int)PositionInWorld.X - Texture.Left,
                    (int)PositionInWorld.Y - Texture.Bottom, 
                    texture.Width, 
                    texture.Height));
            spriteBatch.Draw(texture, 
                des, 
                null, 
                Color.White, 
                0, 
                new Vector2(0), 
                SpriteEffects.None, 
                0);
        }
    }
}