using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace Engine
{
    public class MagicSprite : Sprite
    {
        private Character _belongCharacter;
        private Magic _belongMagic;
        private Vector2 _moveDirection;
        private LinkedList<Vector2> _paths;
        private float _elaspedMilliseconds;
        private float _lifeMilliseconds;
        private bool _isDestroyed;
        private bool _isInDestroy;
        private bool _destroyOnEnd;

        #region Public properties
        public Magic BelongMagic
        {
            get { return _belongMagic; }
            set { _belongMagic = value; }
        }

        public Character BelongCharacter
        {
            get { return _belongCharacter; }
            set { _belongCharacter = value; }
        }

        public Vector2 MoveDirection
        {
            get { return _moveDirection; }
            set { _moveDirection = value; }
        }

        public bool IsDestroyed
        {
            get { return _isDestroyed; }
        }

        public bool IsInDestroy
        {
            get { return _isInDestroy; }
        }

        #endregion Public properties

        public MagicSprite() : base() { }

        public MagicSprite(Magic belongMagic, Character belongCharacter, Vector2 positionInWorld, float velocity, 
            Vector2 moveDirection, bool destroyOnEnd)
        {
            if (belongMagic == null || belongCharacter == null)
            {
                _isDestroyed = true;
                return;
            }
            base.Set(positionInWorld, velocity, belongMagic.FlyingImage, 0);
            BelongMagic = belongMagic;
            BelongCharacter = belongCharacter;
            MoveDirection = moveDirection;
            _destroyOnEnd = destroyOnEnd;
            SetDirection(MoveDirection);
            Begin();
        }

        public MagicSprite(Magic belongMagic, Character belongCharacter, Vector2 positionInWorld, int direction, bool destroyOnEnd)
        {
            if (belongMagic == null || belongCharacter == null)
            {
                _isDestroyed = true;
                return;
            }
            base.Set(positionInWorld, 0, belongMagic.FlyingImage,  direction);
            BelongMagic = belongMagic;
            BelongCharacter = belongCharacter;
            _destroyOnEnd = destroyOnEnd;
            Begin();
        }

        public void Begin()
        {
            if (BelongMagic.FlyingImage == null)
                _lifeMilliseconds = 50f*BelongMagic.LifeFrame;
            else
            {
                if (BelongMagic.LifeFrame == 0)
                    PlayCurrentDirOnce();
                else
                    _lifeMilliseconds = BelongMagic.LifeFrame*BelongMagic.FlyingImage.Interval;
            }
            if (Velocity != 0)//Move 30
            {
                var second = 30f/Velocity;
                MoveTo(MoveDirection, second);
            }
        }

        public void Destroy()
        {
            _isInDestroy = true;
            MoveDirection = Vector2.Zero;
            if (BelongMagic.VanishImage != null)
            {
                Texture = BelongMagic.VanishImage;
                PlayCurrentDirOnce();
            }
            SoundManager.Play3DSoundOnece(BelongMagic.VanishSound, 
                PositionInWorld - BelongCharacter.PositionInWorld);
        }

        public void SetPath(LinkedList<Vector2> paths)
        {
            _paths = paths;
        }

        public void Update(GameTime gameTime)
        {
            if (IsDestroyed) return;

            if (_paths != null)
            {
                if (_paths.Count > 1)
                {
                    var beginPosition = _paths.First.Value;
                    var endPosition = _paths.First.Next.Value;
                    var distance = Vector2.Distance(beginPosition, endPosition);
                    MoveTo(endPosition - beginPosition, (float)gameTime.ElapsedGameTime.TotalSeconds);
                    if (MovedDistance + Globals.DistanceOffset > distance)
                    {
                        MovedDistance = 0f;
                        PositionInWorld = endPosition;
                        _paths.RemoveFirst();
                    }
                }
            }
            else
            {
                MoveTo(MoveDirection, (float)gameTime.ElapsedGameTime.TotalSeconds);
            }
            _elaspedMilliseconds += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (IsInDestroy)
            {
                if (IsPlayCurrentDirOnceEnd()) _isDestroyed = true;
            }
            else
            {
                if (Globals.TheMap.IsObstacleForMagic(MapX, MapY))
                {
                    Destroy();
                }
                else if (
                (BelongMagic.LifeFrame == 0 && IsPlayCurrentDirOnceEnd()) ||
                (BelongMagic.LifeFrame != 0 && _lifeMilliseconds < _elaspedMilliseconds)
                )
                {
                    if(_destroyOnEnd)Destroy();
                    else _isDestroyed = true;
                }
            }
            base.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (IsDestroyed) return;
            base.Draw(spriteBatch);
        }
    }
}
