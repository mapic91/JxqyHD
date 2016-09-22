using System;
using Engine.Weather;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine
{
    public class Sprite : IMoveControlInWorld
    {
        private Vector2 _positionInWorld;
        private int _mapX;
        private int _mapY;
        private float _velocity;
        private int _currentFrameIndex;
        private int _frameBegin;
        private int _frameEnd;
        private int _elapsedMilliSecond;
        private int _currentDirection;
        private TextureBase _texture = Asf.Empty;
        private bool _isPlayReverse;
        private int _leftFrameToPlay;
        private float _movedDistance;
        private static Color _drawColor = Color.White;
        private static Color _rainDrawColor = Color.White;
        public int FrameAdvanceCount { protected set; get; }

        public static Color DrawColor
        {
            get
            {
                if (WeatherManager.IsRaining)
                {
                    return _rainDrawColor;
                }
                else
                {
                    return _drawColor;
                }

            }
            set
            {
                if (WeatherManager.IsRaining)
                {
                    _rainDrawColor = value;
                }
                else
                {
                    _drawColor = value;
                }
            }
        }

        public Sprite() { }

        public Sprite(Vector2 positionInWorld, float velocity, Asf texture, int direction = 0)
        {
            Set(positionInWorld, velocity, texture, direction);
        }

        public void Set(Vector2 positionInWorld, float velocity, TextureBase texture, int direction = 0)
        {
            PositionInWorld = positionInWorld;
            Velocity = velocity;
            Texture = texture;
            CurrentDirection = direction;
        }

        #region Public properties

        public bool IsInPlaying
        {
            get { return (_leftFrameToPlay > 0); }
        }

        public TextureBase Texture
        {
            get { return _texture; }
            set
            {
                if (value == null)
                    _texture = Asf.Empty;
                else _texture = value;
                _elapsedMilliSecond = 0;
                CurrentDirection = CurrentDirection;
                CurrentFrameIndex = _frameBegin;
            }
        }

        public int FrameBegin
        {
            get { return _frameBegin; }
        }

        public int FrameEnd
        {
            get { return _frameEnd; }
        }

        public int Interval
        {
            get
            {
                if (_texture == null) return 0;
                return _texture.Interval;
            }
        }

        public int CurrentDirection
        {
            get { return _currentDirection; }
            set
            {
                var last = _currentDirection;
                var directionCount = _texture.DirectionCounts == 0 ? 1 : _texture.DirectionCounts;
                _currentDirection = value % directionCount;
                if(_currentDirection < 0)
                {
                    _currentDirection = (_currentDirection + directionCount)%directionCount;
                }
                _frameBegin = _currentDirection * _texture.FrameCountsPerDirection;
                _frameEnd = _frameBegin + _texture.FrameCountsPerDirection - 1;
                //If direction change,also change frame index
                if (last != _currentDirection)
                {
                    CurrentFrameIndex = _frameBegin;
                }
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

        public int FrameCountsPerDirection
        {
            get
            {
                if (_texture == null) return 1;
                return _texture.FrameCountsPerDirection;
            }
        }

        public float Velocity
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
                var tilePosition = Map.ToTilePosition(value);
                _mapX = (int) tilePosition.X;
                _mapY = (int) tilePosition.Y;
            }
        }

        public int MapX
        {
            get
            {
                return _mapX;
            }
            set
            {
                _mapX = value;
                _positionInWorld = Map.ToPixelPosition(value, MapY);
            }
        }

        public int MapY
        {
            get
            {
                return _mapY;
            }
            set
            {
                _mapY = value;
                _positionInWorld = Map.ToPixelPosition(MapX, value);
            }
        }

        public Vector2 TilePosition
        {
            get { return new Vector2(MapX, MapY); }
            set
            {
                _positionInWorld = Map.ToPixelPosition(value);
                _mapX = (int)value.X;
                _mapY = (int)value.Y;
            }
        }

        public int Width
        {
            get { return _texture.Width; }
        }
        public int Height
        {
            get{ return _texture.Height; }
        }

        public Vector2 Size
        {
            get { return new Vector2(Width, Height); }
        }

        public virtual Rectangle RegionInWorld
        {
            get
            {
                var reginInWorldPosition = ReginInWorldBeginPosition;
                return new Rectangle((int)reginInWorldPosition.X
                    , (int)reginInWorldPosition.Y
                    , Width
                    , Height);
            }
        }

        public virtual Vector2 ReginInWorldBeginPosition
        {
            get
            {
                return new Vector2((int)PositionInWorld.X - Texture.Left,
                    (int)PositionInWorld.Y - Texture.Bottom);
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
            MoveTo(direction, elapsedSeconds, 1f);
        }

        public void MoveTo(Vector2 direction, float elapsedSeconds, float speedRatio)
        {
            if (direction != Vector2.Zero)
            {
                direction.Normalize();
                MoveToNoNormalizeDirection(direction, elapsedSeconds, speedRatio);
            }
        }

        public void MoveToNoNormalizeDirection(Vector2 direction, float elapsedSeconds, float speedRatio = 1.0f)
        {
            SetDirection(direction);
            var move = direction * _velocity * elapsedSeconds * speedRatio;
            PositionInWorld += move;
            MovedDistance += move.Length();
        }

        /// <summary>
        /// Play frames count than stop
        /// </summary>
        /// <param name="count">Frame count</param>
        /// <param name="reverse">Play from current index to front.Otherwise to back.</param>
        public void PlayFrames(int count, bool reverse = false)
        {
            _leftFrameToPlay = count;
            _isPlayReverse = reverse;
        }

        public void PlayCurrentDirOnce()
        {
            if (IsInPlaying) return;
            PlayFrames(_frameEnd - CurrentFrameIndex + 1);
        }

        public void PlayCurrentDirOnceReverse()
        {
            if (IsInPlaying) return;
            PlayFrames(_currentFrameIndex - _frameBegin + 1, true);
        }

        public void EndPlayCurrentDirOnce()
        {
            _leftFrameToPlay = 0;
        }

        public bool IsPlayCurrentDirOnceEnd()
        {
            return !IsInPlaying;
        }

        public bool IsFrameAtBegin()
        {
            return CurrentFrameIndex == _frameBegin;
        }

        public bool IsFrameAtEnd()
        {
            return CurrentFrameIndex == _frameEnd;
        }

        /// <summary>
        /// Set direction(not correct input direction value)
        /// </summary>
        /// <param name="direcion">direction to set</param>
        public void SetDirectionValue(int direcion)
        {
            if (_texture.DirectionCounts > direcion)
            {
                //Direction in current texture direction count range
                SetDirection(direcion);
            }
            else
            {
                //Direction not in range
                _currentDirection = direcion;
            }
        }

        public void Update(GameTime gameTime, Vector2 direction, int speedFold = 1)
        {
            var elapsedTime = new TimeSpan(gameTime.ElapsedGameTime.Ticks * speedFold);
            MoveTo(direction, (float)elapsedTime.TotalSeconds);
            Update(gameTime, speedFold);
        }

        public void Update(GameTime gameTime, int speedFold)
        {
            var elapsedTime = new TimeSpan(gameTime.ElapsedGameTime.Ticks * speedFold);
            Update((int)elapsedTime.TotalMilliseconds);
        }

        public virtual void Update(GameTime gameTime)
        {
            Update((int)gameTime.ElapsedGameTime.TotalMilliseconds);
        }

        private void Update(int elapsedMilliSecond)
        {
            _elapsedMilliSecond += elapsedMilliSecond;
            FrameAdvanceCount = 0;
            if (_elapsedMilliSecond > Texture.Interval)
            {
                _elapsedMilliSecond -= Texture.Interval;
                if (IsInPlaying && _isPlayReverse)
                {
                    CurrentFrameIndex--;
                }
                else
                {
                    CurrentFrameIndex++;
                }
                FrameAdvanceCount = 1;

                if (_leftFrameToPlay > 0)
                {
                    _leftFrameToPlay--;
                }
            }
        }


        public void SetDirection(Vector2 direction)
        {
            if (direction != Vector2.Zero && Texture.DirectionCounts != 0)
            {
                CurrentDirection = Utils.GetDirectionIndex(direction, Texture.DirectionCounts);
            }
        }

        public void SetDirection(int direction)
        {
            CurrentDirection = direction;
        }

        public Texture2D GetCurrentTexture()
        {
            if (Texture == null) return null;
            return Texture.GetFrame(CurrentFrameIndex);
        }

        public virtual void Draw(SpriteBatch spriteBatch, int offX = 0, int offY = 0)
        {
            Draw(spriteBatch, GetCurrentTexture(), offX, offY);
        }

        public virtual void Draw(SpriteBatch spriteBatch, Color color)
        {
            Draw(spriteBatch, GetCurrentTexture(), color);
        }

        public virtual void Draw(SpriteBatch spriteBatch, Texture2D texture, int offX = 0, int offY = 0)
        {
            Draw(spriteBatch, texture, DrawColor, offX, offY);
        }

        public virtual void Draw(SpriteBatch spriteBatch, Texture2D texture, Color color, int offX = 0, int offY = 0)
        {
            if (texture == null) return;
            Rectangle des =
                 Globals.TheCarmera.ToViewRegion(new Rectangle((int)PositionInWorld.X - Texture.Left + offX,
                    (int)PositionInWorld.Y - Texture.Bottom + offY,
                    texture.Width,
                    texture.Height));

            if (color == Color.Black)
            {
                spriteBatch.End();
                JxqyGame.BeginSpriteBatch(spriteBatch, Globals.TheGame.GrayScaleEffect);


                spriteBatch.Draw(texture,
                    des,
                    null,
                    Color.White,
                    0,
                    new Vector2(0),
                    SpriteEffects.None,
                    0);

                spriteBatch.End();
                JxqyGame.BeginSpriteBatch(spriteBatch);
            }
            else
            {
                spriteBatch.Draw(texture,
                des,
                null,
                color,
                0,
                new Vector2(0),
                SpriteEffects.None,
                0);
            }
        }
    }
}