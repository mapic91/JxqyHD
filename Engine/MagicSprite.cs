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
        private List<Vector2> _paths;
        private int _elaspedFrameSum;
        private bool _isDestroyed;
        private bool _isInDestroy;

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

        public MagicSprite(Vector2 positionInWorld, int velocity, Magic belongMagic, Character belongCharacter,
            Vector2 moveDirection)
            : base(positionInWorld, velocity, belongMagic.FlyingImage, 0)
        {
            BelongMagic = belongMagic;
            BelongCharacter = belongCharacter;
            MoveDirection = moveDirection;
            Begin();
        }

        public void Begin()
        {
            Texture = BelongMagic.FlyingImage;
            SetDirection(MoveDirection);
            if (BelongMagic.FlyingSound != null)
                BelongMagic.FlyingSound.Play(Globals.SoundEffectVolume, 0f, 0f);
            if (BelongMagic.LifeFrame == 0)
                PlayCurrentDirOnce();
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
            else _isDestroyed = true;
            if (BelongMagic.VanishSound != null)
            {
                var distance = Vector2.Distance(BelongCharacter.PositionInWorld, PositionInWorld);
                if (distance < Globals.SoundMaxDistance)
                {
                    var volume = 1f - distance/Globals.SoundMaxDistance;
                    BelongMagic.VanishSound.Play(volume * Globals.SoundEffectVolume, 0f, 0f);
                }
            }
        }

        public void SetPath(List<Vector2> paths)
        {
            _paths = paths;
        }

        public void Update(GameTime gameTime)
        {
            if (IsDestroyed) return;

            var lastPosition = PositionInWorld;
            MoveTo(MoveDirection, (float)gameTime.ElapsedGameTime.TotalSeconds);
            _elaspedFrameSum++;
            if (IsInDestroy)
            {
                if (IsPlayCurrentDirOnceEnd()) _isDestroyed = true;
            }
            else
            {
                if (
                (BelongMagic.LifeFrame == 0 && IsPlayCurrentDirOnceEnd()) ||
                (BelongMagic.LifeFrame != 0 && BelongMagic.LifeFrame < _elaspedFrameSum)
                )
                {
                    Destroy();
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
