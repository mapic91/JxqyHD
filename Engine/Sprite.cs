using System;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine
{
    public class Sprite : IMoveControlInWorld
    {
        private Vector2 _positionInWorld;
        private int _mapX;
        private int _mapY;
        private bool _isTilePostionNew;
        private int _velocity;
        private int _currentFrameIndex;
        private int _frameBegin;
        private int _frameEnd;
        private int _elapsedMilliSecond;
        private int _currentDirection;
        private Asf _texture = new Asf();
        private bool _isPlayingCurrentDirOnce;
        private float _movedDistance;

        public Sprite() { }

        public Sprite(Vector2 positionInWorld, int velocity, Asf texture, int direction = 0)
        {
            Set(positionInWorld, velocity, texture, direction);
        }

        public void Set(Vector2 positionInWorld, int velocity, Asf texture, int direction = 0)
        {
            PositionInWorld = positionInWorld;
            Velocity = velocity;
            Texture = texture;
            CurrentDirection = direction;
        }

        #region Public properties
        public Asf Texture
        {
            get { return _texture; }
            set
            {
                if (value == null)
                    _texture = new Asf();
                else _texture = value;
                _elapsedMilliSecond = 0;
                CurrentDirection = CurrentDirection;
                CurrentFrameIndex = _frameBegin;
            }
        }

        public int CurrentDirection
        {
            get { return _currentDirection; }
            private set
            {
                if (_isPlayingCurrentDirOnce) return; //Can't set when playing
                _currentDirection = value % (_texture.DirectionCounts == 0 ? 1 : _texture.DirectionCounts);
                _frameBegin = _currentDirection * _texture.FrameCountsPerDirection;
                _frameEnd = _frameBegin + _texture.FrameCountsPerDirection - 1;
                CurrentFrameIndex = CurrentFrameIndex;
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
            set
            {
                _positionInWorld = value;
                _isTilePostionNew = false;
            }
        }

        public int MapX
        {
            get
            {
                if (IsTilePostionNew) return _mapX;
                IsTilePostionNew = true;
                var position = Map.ToTilePosition(PositionInWorld);
                _mapX = (int)position.X;
                _mapY = (int)position.Y;
                return _mapX;
            }
            set
            {
                _mapX = value;
                PositionInWorld = Map.ToPixelPosition(value, MapY);
            }
        }

        public int MapY
        {
            get
            {
                if (IsTilePostionNew) return _mapY;
                IsTilePostionNew = true;
                var position = Map.ToTilePosition(PositionInWorld);
                _mapX = (int)position.X;
                _mapY = (int)position.Y;
                return _mapY;
            }
            set
            {
                _mapY = value;
                PositionInWorld = Map.ToPixelPosition(MapX, value);
            }
        }

        public bool IsTilePostionNew
        {
            get { return _isTilePostionNew; }
            set { _isTilePostionNew = value; }
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
                return new Rectangle((int)PositionInWorld.X - Texture.Left,
                    (int)PositionInWorld.Y - Texture.Bottom
                    , Width
                    , Height);
            }
        }

        public float MovedDistance
        {
            get { return _movedDistance; }
            set { _movedDistance = value; }
        }

        #endregion Properties

        public void MoveTo(Vector2 direction, float elapsedSeconds)
        {
            if (direction != Vector2.Zero)
            {
                SetDirection(direction);
                direction.Normalize();
                var move = direction * _velocity * elapsedSeconds;
                PositionInWorld += move;
                MovedDistance += move.Length();
            }
        }

        public void PlayCurrentDirOnce()
        {
            if (_isPlayingCurrentDirOnce) return;
            _isPlayingCurrentDirOnce = true;
            CurrentFrameIndex = _frameBegin;//Reset frame
        }

        public bool IsPlayCurrentDirOnceEnd()
        {
            return !_isPlayingCurrentDirOnce;
        }

        public bool IsFrameAtBegin()
        {
            return CurrentFrameIndex == _frameBegin;
        }

        public void Update(GameTime gameTime, Vector2 direction, int speedFold = 1)
        {
            var elapsedTime = new TimeSpan(gameTime.ElapsedGameTime.Ticks * speedFold);
            MoveTo(direction, (float)elapsedTime.TotalSeconds);
            Update(gameTime, speedFold);
        }

        public void Update(GameTime gameTime, int speedFold = 1)
        {
            var elapsedTime = new TimeSpan(gameTime.ElapsedGameTime.Ticks * speedFold);
            _elapsedMilliSecond += (int)elapsedTime.TotalMilliseconds;
            if (_elapsedMilliSecond > Texture.Interval)
            {
                _elapsedMilliSecond -= Texture.Interval;
                CurrentFrameIndex++;
                if (_isPlayingCurrentDirOnce && CurrentFrameIndex == _frameEnd)
                    _isPlayingCurrentDirOnce = false;
            }
        }

        
        public void SetDirection(Vector2 direction)
        {
            if (direction != Vector2.Zero && Texture.DirectionCounts != 0)
            {
                CurrentDirection = Utils.GetDirectionIndex(direction, Texture.DirectionCounts);
            }
        }

        public Texture2D GetCurrentTexture()
        {
            if (Texture == null) return null;
            return Texture.GetFrame(CurrentFrameIndex);
        }

        public void Draw(SpriteBatch spriteBatch, int offX = 0, int offY = 0)
        {
            Draw(spriteBatch, GetCurrentTexture(), offX, offY);
        }

        public void Draw(SpriteBatch spriteBatch, Texture2D texture, int offX = 0, int offY = 0)
        {
            if (texture == null) return;
            Rectangle des =
                 Globals.TheCarmera.ToViewRegion(new Rectangle((int)PositionInWorld.X - Texture.Left + offX,
                    (int)PositionInWorld.Y - Texture.Bottom + offY,
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