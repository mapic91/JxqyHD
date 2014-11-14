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
        private int _elapsedFrame;
        private bool _isDestroyed;
        private bool _isInDestroy;
        private bool _destroyOnEnd;
        private LinkedList<Sprite> _superModeDestroySprites;
        private Character _closedCharecter;

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

        public bool IsLive
        {
            get { return (!IsDestroyed && !IsInDestroy); }
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

        #region Ctr
        public MagicSprite() : base() { }

        public MagicSprite(Magic belongMagic, Character belongCharacter, Vector2 positionInWorld, float velocity,
            Vector2 moveDirection, bool destroyOnEnd)
        {
            if (belongMagic == null || belongCharacter == null)
            {
                _isDestroyed = true;
                return;
            }
            var texture = belongMagic.FlyingImage;
            if (belongMagic.MoveKind == 15)
                texture = belongMagic.SuperModeImage;
            Set(positionInWorld, velocity, texture, 0);
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
            var texture = belongMagic.FlyingImage;
            if (belongMagic.MoveKind == 15)
                texture = belongMagic.SuperModeImage;
            Set(positionInWorld, 0, texture, direction);
            BelongMagic = belongMagic;
            BelongCharacter = belongCharacter;
            _destroyOnEnd = destroyOnEnd;
            Begin();
        }
        #endregion Ctr

        private void CharacterHited(Character character)
        {
            if (character == null) return;

            var targetEvade = character.Evade;
            var belongCharacterEvade = BelongCharacter.Evade;
            const float maxOffset = 100f;
            const float baseHitRatio = 0.05f;
            const float belowRatio = 0.3f;
            const float upRatio = 0.65f;
            var hitRatio = baseHitRatio;
            if (targetEvade >= belongCharacterEvade)
            {
                hitRatio += ((float)belongCharacterEvade / (float)targetEvade) * belowRatio;
            }
            else
            {
                var upOffsetRatio = ((float)belongCharacterEvade - targetEvade) / maxOffset;
                if (upOffsetRatio > 1f) upOffsetRatio = 1f;
                hitRatio += belowRatio + upOffsetRatio * upRatio;
            }

            if (Globals.TheRandom.Next(101) <= (int)(hitRatio * 100f))
            {
                const int minimalEffect = 5;
                var effect = minimalEffect;
                var amount = BelongMagic.Effect == 0 ? BelongCharacter.Attack : BelongMagic.Effect;
                var offset = amount - character.Defend;
                if (offset > minimalEffect) effect = offset;
                foreach (var magicSprite in character.MagicSpritesInEffect)
                {
                    var magic = magicSprite.BelongMagic;
                    switch (magic.MoveKind)
                    {
                        case 13:
                            if (magic.SpecialKind == 3)
                            {
                                var manaReduce = magic.Effect;
                                if (effect < manaReduce) manaReduce = effect;
                                manaReduce /= 2;
                                if (character.Mana >= manaReduce)
                                {
                                    character.Mana -= manaReduce;
                                    effect -= magic.Effect;
                                    if (effect < 0) effect = 0;
                                }
                            }
                            break;
                    }
                }
                character.Life -= effect;
                if (character.Life <= 0) character.Death();
                else
                {
                    if (Globals.TheRandom.Next(4) == 0
                        && BelongMagic.SpecialKind != 3)//Can't hurted when going to be petrified 
                        character.Hurting();
                }
            }

            switch (BelongMagic.SpecialKind)
            {
                case 1:
                    if(!character.IsFrozened)
                        character.FrozenSeconds = BelongMagic.CurrentLevel + 1;
                    break;
                case 2:
                    if(!character.IsPoisoned)
                        character.PoisonSeconds = BelongMagic.CurrentLevel + 1;
                    break;
                case 3:
                    if(!character.IsPetrified)
                        character.PetrifiedSeconds = BelongMagic.CurrentLevel + 1;
                    break;
            }

            switch (BelongMagic.AdditionalEffect)
            {
                case Magic.AddonEffect.Frozen:
                    if(!character.IsFrozened)
                        character.FrozenSeconds = BelongCharacter.Level/10 + 1;
                    break;
                case Magic.AddonEffect.Poision:
                    if(!character.IsPoisoned)
                        character.PoisonSeconds = BelongCharacter.Level / 10 + 1;
                    break;
                case Magic.AddonEffect.Petrified:
                    if(!character.IsPetrified)
                        character.PetrifiedSeconds = BelongCharacter.Level / 10 + 1;
                    break;
            }

            Destroy();
        }

        private void CheckCharacterHited()
        {
            if (BelongCharacter.IsPlayer)
            {
                CharacterHited(NpcManager.GetEnemy(TilePosition));
            }
            else
            {
                if (TilePosition == Globals.ThePlayer.TilePosition)
                {
                    CharacterHited(Globals.ThePlayer);
                }
            }
        }

        public void Begin()
        {

            if (BelongMagic.LifeFrame == 0 ||
                BelongMagic.MoveKind == 15)
                PlayCurrentDirOnce();

            if (Velocity != 0)//Move 30
            {
                var second = 30f / Velocity;
                MoveTo(MoveDirection, second);
            }
            else
            {
                // can't put fixed position magic sprite in obstacle
                if (Globals.TheMap.IsObstacleForMagic(MapX, MapY))
                    _isDestroyed = true;
            }
        }

        public void Destroy()
        {
            if (IsInDestroy) return;
            _isInDestroy = true;
            MoveDirection = Vector2.Zero;

            if (BelongMagic.MoveKind == 15)
            {
                Texture = null;
                _superModeDestroySprites = new LinkedList<Sprite>();
                foreach (var npc in NpcManager.NpcsInView)
                {
                    if (npc.IsEnemy)
                    {
                        var sprite = new Sprite(npc.PositionInWorld,
                            0f,
                            BelongMagic.VanishImage,
                            0);
                        sprite.PlayCurrentDirOnce();
                        _superModeDestroySprites.AddLast(sprite);
                        CharacterHited(npc);
                        SoundManager.Play3DSoundOnece(BelongMagic.VanishSound,
                            npc.PositionInWorld - Globals.ListenerPosition);
                    }
                }
                if (_superModeDestroySprites.Count == 0) _isDestroyed = true;
            }
            else
            {
                if (BelongMagic.VanishImage != null)
                {
                    EndPlayCurrentDirOnce();
                    Texture = BelongMagic.VanishImage;
                    PlayCurrentDirOnce();
                }
                SoundManager.Play3DSoundOnece(BelongMagic.VanishSound,
                PositionInWorld - Globals.ListenerPosition);
            }

        }

        public void SetPath(LinkedList<Vector2> paths)
        {
            _paths = paths;
        }

        public void ResetElaspedFrame()
        {
            _elapsedFrame = 0;
        }

        public override void Update(GameTime gameTime)
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
                    if (MovedDistance >= distance)
                    {
                        _paths.RemoveFirst();
                        MovedDistance = 0;
                        PositionInWorld = endPosition;
                        if (_paths.Count < 2)
                        {
                            if (_destroyOnEnd) Destroy();
                            CheckCharacterHited();
                        }
                    }
                }
            }
            else
            {
                if (BelongMagic.MoveKind == 16)
                {
                    if (MovedDistance > 200f)
                    {
                        if (BelongCharacter.IsPlayer)
                        {
                            if (_closedCharecter == null || _closedCharecter.IsDeath)
                            {
                                _closedCharecter = NpcManager.GetClosedEnemy(PositionInWorld);
                            }
                        }
                        else
                        {
                            _closedCharecter = Globals.ThePlayer;
                        }

                        if (_closedCharecter != null &&
                            MoveDirection != Vector2.Zero)//When MoveDirecton equal zero, magic sprite is in destroying, destroyed or can't move
                            MoveDirection = _closedCharecter.PositionInWorld - PositionInWorld;
                    }
                    MoveTo(MoveDirection,
                        (float)gameTime.ElapsedGameTime.TotalSeconds,
                        MagicManager.GetSpeedRatio(MoveDirection));
                }
                else MoveTo(MoveDirection, (float)gameTime.ElapsedGameTime.TotalSeconds);
            }

            if (BelongMagic.MoveKind == 13)
            {
                _elapsedFrame++;
                PositionInWorld = BelongCharacter.PositionInWorld;
            }
            else _elapsedFrame += FrameAdvanceCount;

            if (IsInDestroy)
            {
                if (BelongMagic.MoveKind == 15)
                {
                    var end = false;
                    foreach (var sprite in _superModeDestroySprites)
                    {
                        sprite.Update(gameTime);
                        if (sprite.IsPlayCurrentDirOnceEnd())
                        {
                            end = true;
                            break;
                        }
                    }
                    if (end) _isDestroyed = true;
                }
                else
                {
                    if (IsPlayCurrentDirOnceEnd()) _isDestroyed = true;
                }
            }
            else if (BelongMagic.MoveKind == 15)
            {
                if (IsPlayCurrentDirOnceEnd())
                    Destroy();
            }
            else if (BelongMagic.MoveKind == 17)
            {
                //do nothing
            }
            else
            {
                if (BelongMagic.MoveKind != 13)
                    CheckCharacterHited();

                if (Globals.TheMap.IsObstacleForMagic(MapX, MapY))
                {
                    Destroy();
                }
                else if (
                (BelongMagic.LifeFrame == 0 && IsPlayCurrentDirOnceEnd()) ||
                (BelongMagic.LifeFrame != 0 && _elapsedFrame >= BelongMagic.LifeFrame)
                )
                {
                    if (_destroyOnEnd) Destroy();
                    else _isDestroyed = true;
                }
            }
            base.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (IsDestroyed) return;
            if (BelongMagic.MoveKind == 15 && IsInDestroy)
            {
                foreach (var sprite in _superModeDestroySprites)
                {
                    sprite.Draw(spriteBatch);
                }
            }
            base.Draw(spriteBatch);
        }
    }
}
