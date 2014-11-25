using Microsoft.Xna.Framework;

namespace Engine
{
    public class Carmera
    {
        private int _worldWidth;
        private int _worldHeight;
        private int _viewBeginX;
        private int _viewBeginY;
        private int _viewWidth;
        private int _viewHeight;
        private IMoveControlInWorld _moveControlInWorld;
        private bool _isFollow;
        private int _moveSpeed;
        private bool _isInMove;
        private Vector2 _moveDestination;
        private float _movedDistance, _maxMoveDistance;
        private void Init(int beginX, int beginY, int viewWidth, int viewHeight, int worldWidth, int worldHeight)
        {
            WorldWidth = worldWidth;
            WorldHeight = worldHeight;
            ViewBeginX = beginX;
            ViewBeginY = beginY;
            ViewWidth = viewWidth;
            ViewHeight = viewHeight;
        }

        public Carmera() { }

        public Carmera(int beginX, int beginY, int viewWidth, int viewHeight, int worldWidth, int worldHeight)
        {
            Init(beginX, beginY, viewWidth, viewHeight, worldWidth, worldHeight);
        }

        public Carmera(Rectangle region, int worldWidth, int worldHeight)
        {
            Init(region.X, region.Y, region.Width, region.Height, worldWidth, worldHeight);
        }

        #region Public method
        public void Update(GameTime gameTime)
        {
            var lastCarmerPos = CarmeraBeginPositionInWorld;
            if (IsInMove)
            {
                Vector2 dir = MoveDestination - CarmeraBeginPositionInWorld;
                if (dir != Vector2.Zero)
                {
                    dir.Normalize();
                    var move = dir*_moveSpeed*(float) gameTime.ElapsedGameTime.TotalSeconds;
                    CarmeraBeginPositionInWorld += move;
                    _movedDistance += move.Length();
                }
                else
                {
                    IsInMove = false;
                }
                if (_movedDistance >= _maxMoveDistance ||
                    (int)lastCarmerPos.X == (int)CarmeraBeginPositionInWorld.X ||
                    (int)lastCarmerPos.Y == (int)CarmeraBeginPositionInWorld.Y)
                {
                    IsInMove = false;
                }
            }
            if (IsFollow && _moveControlInWorld != null && !IsInMove)
            {
                Vector2 pos = _moveControlInWorld.PositionInWorld;
               // pos += new Vector2((float)_moveControlInWorld.Width / 2, (float)_moveControlInWorld.Height / 2);
                pos -= new Vector2((float)ViewWidth / 2, (float)ViewHeight / 2);
                CarmeraBeginPositionInWorld = pos;
            }
        }

        public void Follow(IMoveControlInWorld moveControlInWorld)
        {
            IsFollow = true;
            _moveControlInWorld = moveControlInWorld;
            SmoothMoveTo(moveControlInWorld.PositionInWorld + moveControlInWorld.Size/2 - ViewSize/2);
        }

        public void SmoothMoveTo(Vector2 position, int velocity = 5000)
        {
            IsInMove = true;
            MoveDestination = position;
            _moveSpeed = velocity;
            _movedDistance = 0f;
            _maxMoveDistance = (MoveDestination - CarmeraBeginPositionInWorld).Length();
        }

        public Vector2 ToViewPosition(Vector2 worldPosition)
        {
            return worldPosition - CarmeraBeginPositionInWorld;
        }

        public Vector2 ToWorldPosition(Vector2 viewPosition)
        {
            return CarmeraBeginPositionInWorld + viewPosition;
        }

        public Rectangle ToViewRegion(Rectangle worldRegion)
        {
            Vector2 pos = ToViewPosition(new Vector2(worldRegion.X, worldRegion.Y));
            return new Rectangle((int)pos.X, (int)pos.Y, worldRegion.Width, worldRegion.Height);
        }

        public Rectangle ToWorldRegion(Rectangle viewRegion)
        {
            Vector2 pos = ToWorldPosition(new Vector2(viewRegion.X, viewRegion.Y));
            return new Rectangle((int)pos.X, (int)pos.Y, viewRegion.Width, viewRegion.Height);
        }
        #endregion Public method

        #region Properties
        public bool IsInMove
        {
            get { return _isInMove; }
            set { _isInMove = value; }
        }

        public Vector2 MoveDestination
        {
            get { return _moveDestination; }
            set
            {
                Vector2 tempPos = CarmeraBeginPositionInWorld;
                CarmeraBeginPositionInWorld = value; //Restrict value
                _moveDestination = CarmeraBeginPositionInWorld;
                CarmeraBeginPositionInWorld = tempPos;//Restore
            }
        }

        public bool IsFollow
        {
            get { return _isFollow; }
            set { _isFollow = value; }
        }

        public int ViewHeight
        {
            get { return _viewHeight > WorldHeight ? WorldHeight : _viewHeight; }
            set
            {
                if (WorldHeight > 0) // WorldHeight is setted
                    _viewHeight = (int)MathHelper.Clamp(value, 0, WorldHeight);
                else
                    _viewHeight = value < 0 ? 0 : value;
            }
        }

        public int ViewWidth
        {
            get { return _viewWidth > WorldWidth ? WorldWidth : _viewWidth; }
            set
            {
                if (WorldWidth > 0) //WorldWidth is setted
                    _viewWidth = (int)MathHelper.Clamp(value, 0, WorldWidth);
                else
                    _viewWidth = value < 0 ? 0 : value;
            }
        }

        public Vector2 ViewSize
        {
            get { return new Vector2(ViewWidth,ViewHeight); }
            set
            {
                ViewWidth = (int) value.X;
                ViewHeight = (int) value.Y;
            }
        }

        public int ViewBeginY
        {
            get { return _viewBeginY; }
            set
            {
                if (value <= 0) _viewBeginY = 0;
                else if (value + ViewHeight > WorldHeight)
                    _viewBeginY = WorldHeight - ViewHeight;
                else _viewBeginY = value;
                if (_viewBeginY < 0) _viewBeginY = 0;
            }
        }

        public int ViewBeginX
        {
            get { return _viewBeginX; }
            set
            {
                if (value <= 0) _viewBeginX = 0;
                else if (value + ViewWidth > WorldWidth)
                    _viewBeginX = WorldWidth - ViewWidth;
                else _viewBeginX = value;
                if (_viewBeginX < 0) _viewBeginX = 0;
            }
        }

        public int WorldHeight
        {
            get { return _worldHeight; }
            set
            {
                _worldHeight = value < 0 ? 0 : value;
            }
        }

        public int WorldWidth
        {
            get { return _worldWidth; }
            set
            {
                _worldWidth = value < 0 ? 0 : value;
            }
        }

        public Vector2 CarmeraBeginPositionInWorld
        {
            get { return new Vector2(ViewBeginX, ViewBeginY); }
            set
            {
                ViewBeginX = (int)value.X;
                ViewBeginY = (int)value.Y;
            }
        }

        public Rectangle CarmerRegionInWorld
        {
            get { return new Rectangle(ViewBeginX, ViewBeginY, ViewWidth, ViewHeight); }
            set
            {
                ViewBeginX = value.X;
                ViewBeginY = value.Y;
                ViewWidth = value.Width;
                ViewHeight = value.Height;
            }
        }
        #endregion Properties
    }
}
