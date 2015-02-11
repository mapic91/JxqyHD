using System.Collections.Generic;
using Engine.Weather;
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
        private bool _isDestroyed;
        private bool _isInDestroy;
        private bool _destroyOnEnd;
        private LinkedList<Sprite> _superModeDestroySprites;
        private LinkedList<Sprite> _destoryedLeapSprites;
        private List<Character> _leapedCharacters; 
        private Character _closedCharecter;

        private int _leftLeapTimes;
        private int _currentEffect;


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

        #region Ctor
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
        #endregion Ctor

        private static int GetEffectAmount(Magic magic, Character belongCharacter)
        {
            //If magic effect not set(equal 0) use belong character attack value as amount.
            //Because npc just can use FlyIni FlyIni2, 
            //so if belong character is a npc not a player use character attack value as amount also.
            if (magic == null || belongCharacter == null) return 0;
            return (magic.Effect == 0 || !belongCharacter.IsPlayer) ?
                    belongCharacter.Attack :
                    magic.Effect;
        }

        private void CharacterHited(Character character)
        {
            if (character == null) return;

            //Apply magic special effect
            switch (BelongMagic.SpecialKind)
            {
                case 1:
                    if (!character.IsFrozened)
                        character.FrozenSeconds = BelongMagic.CurrentLevel + 1;
                    break;
                case 2:
                    if (!character.IsPoisoned)
                        character.PoisonSeconds = BelongMagic.CurrentLevel + 1;
                    break;
                case 3:
                    if (!character.IsPetrified)
                        character.PetrifiedSeconds = BelongMagic.CurrentLevel + 1;
                    break;
            }

            //Additional attack effect added to magic when player equip special equipment
            switch (BelongMagic.AdditionalEffect)
            {
                case Magic.AddonEffect.Frozen:
                    if (!character.IsFrozened)
                        character.FrozenSeconds = BelongCharacter.Level / 10 + 1;
                    break;
                case Magic.AddonEffect.Poision:
                    if (!character.IsPoisoned)
                        character.PoisonSeconds = BelongCharacter.Level / 10 + 1;
                    break;
                case Magic.AddonEffect.Petrified:
                    if (!character.IsPetrified)
                        character.PetrifiedSeconds = BelongCharacter.Level / 10 + 1;
                    break;
            }

            //Hit ratio
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
                //Character hurted by magic
                const int minimalEffect = 5;
                var effect = minimalEffect;

                var amount = _currentEffect > 0 ? _currentEffect : GetEffectAmount(BelongMagic, BelongCharacter);

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
                character.DecreaseLifeAddHurt(effect);
            }

            if (BelongCharacter.IsPlayer)
            {
                var player = BelongCharacter as Player;
                if (player != null)
                {
                    player.AddMagicExp(BelongMagic.ItemInfo, 
                        Utils.GetMagicExp(character.Level));
                }
            }

            if (_leftLeapTimes > 0)
            {
                LeapToNextTarget(character);
            }
            else
            {
                Destroy();
            }
        }

        private void CheckCharacterHited()
        {
            if (BelongCharacter.IsPlayer || BelongCharacter.IsFriend)
            {
                var target = NpcManager.GetEnemy(TilePosition);
                var isInDeath = target != null && target.IsDeathInvoked;
                CharacterHited(target);
                if (target != null)
                {
                    target.NotifyEnemyAndAllNeighbor(BelongCharacter);
                    //Hited character death
                    if (!isInDeath && //Alive before hited
                        target.IsInDeathing && //Death after hited
                        (BelongCharacter.IsPlayer ||
                        BelongCharacter.IsPartner))
                    {
                        Globals.ThePlayer.AddExp(
                            Utils.GetCharacterDeathExp(Globals.ThePlayer, target));
                    }
                }
            }
            else
            {
                CharacterHited(NpcManager.GetPlayerOrFighterFriend(TilePosition));
            }
        }

        private void Begin()
        {
            _leftLeapTimes = BelongMagic.LeapTimes;
            _currentEffect = GetEffectAmount(BelongMagic, BelongCharacter);

            if (_leftLeapTimes > 0)
            {
                //Initilize leap
                _destoryedLeapSprites = new LinkedList<Sprite>();
                _leapedCharacters = new List<Character>();
            }

            //Start play FlyingImage
            ResetPlay();

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

        private static void AddDestroySprite(LinkedList<Sprite> list, Vector2 positionInWorld, Asf image, SoundEffect sound)
        {
            var sprite = new Sprite(positionInWorld,
                            0f,
                            image);
            sprite.PlayFrames(sprite.FrameCountsPerDirection);
            list.AddLast(sprite);
            SoundManager.Play3DSoundOnece(sound,
                positionInWorld - Globals.ListenerPosition);
        }

        private void Destroy()
        {
            if (IsInDestroy) return;
            _isInDestroy = true;

            if (BelongMagic.MoveKind == 15)
            {
                Texture = null;
                _superModeDestroySprites = new LinkedList<Sprite>();
                foreach (var npc in NpcManager.NpcsInView)
                {
                    if (npc.IsEnemy)
                    {
                        AddDestroySprite(_superModeDestroySprites, 
                            npc.PositionInWorld,
                            BelongMagic.VanishImage,
                            BelongMagic.VanishSound);
                        CharacterHited(npc);
                    }
                }
                if (_superModeDestroySprites.Count == 0) _isDestroyed = true;
            }
            else
            {
                if (BelongMagic.VanishImage != null)
                {
                    Texture = BelongMagic.VanishImage;
                    PlayFrames(FrameCountsPerDirection);
                }
                else
                {
                    _isDestroyed = true;
                }
                SoundManager.Play3DSoundOnece(BelongMagic.VanishSound,
                    PositionInWorld - Globals.ListenerPosition);

                if (BelongMagic.ExplodeMagicFile != null)
                {
                    MagicManager.UseMagic(BelongCharacter,
                        BelongMagic.ExplodeMagicFile,
                        PositionInWorld,
                        MoveDirection == Vector2.Zero
                            ? PositionInWorld + (PositionInWorld - BelongCharacter.PositionInWorld)
                            : PositionInWorld + MoveDirection);
                }

                MoveDirection = Vector2.Zero;
            }
        }

        private void LeapToNextTarget(Character hitedCharacter)
        {
            if (_leftLeapTimes > 0)
            {
                _leftLeapTimes--;
                _currentEffect -= _currentEffect*BelongMagic.EffectReducePercentage/100;
            }
            else
            {
                EndLeap();
            }

            if (BelongMagic.VanishImage != null)
            {
                AddDestroySprite(_destoryedLeapSprites, PositionInWorld, BelongMagic.VanishImage, BelongMagic.VanishSound);
            }

            var closedEnemy = NpcManager.GetClosedEnemy(BelongCharacter, hitedCharacter.PositionInWorld, _leapedCharacters);
            if (closedEnemy == null)
            {
                EndLeap();
                return;
            }
            Texture = BelongMagic.LeapImage;
            PlayFrames(BelongMagic.LeapFrame);
            MoveDirection = closedEnemy.PositionInWorld - PositionInWorld;
            //Move magic sprite to neighber tile
            TilePosition = PathFinder.FindNeighborInDirection(TilePosition, MoveDirection);
            //Correct move direction
            MoveDirection = closedEnemy.PositionInWorld - PositionInWorld;

            _leapedCharacters.Add(hitedCharacter);
        }

        private void EndLeap()
        {
            _leftLeapTimes = 0;
            if (_destroyOnEnd) Destroy();
        }

        public void SetPath(LinkedList<Vector2> paths)
        {
            _paths = paths;
        }

        /// <summary>
        /// Stop play than play from begin.
        /// </summary>
        public void ResetPlay()
        {
            var framesToPlay = BelongMagic.LifeFrame;
            if (BelongMagic.LifeFrame == 0 ||
                BelongMagic.MoveKind == 15)
            {
                framesToPlay = FrameCountsPerDirection;
            }
            else if (BelongMagic.MoveKind == 13)
            {
                var interval = Interval == 0 ? 1000/60 : Interval;
                framesToPlay = (int)(BelongMagic.LifeFrame * 10f/ interval);
            }

            PlayFrames(framesToPlay);
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
                    if (MovedDistance > 200f) //First move 200, than find target
                    {
                        if (BelongCharacter.IsPlayer || BelongCharacter.IsFriend)
                        {
                            if (_closedCharecter == null || _closedCharecter.IsDeath)
                            {
                                _closedCharecter = NpcManager.GetClosedEnemyTypeCharacter(PositionInWorld);
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
                PositionInWorld = BelongCharacter.PositionInWorld;
            }

            if (IsInDestroy)
            {
                if (BelongMagic.MoveKind == 15)
                {
                    var end = false;
                    foreach (var sprite in _superModeDestroySprites)
                    {
                        sprite.Update(gameTime);
                        if (!sprite.IsInPlaying)
                        {
                            end = true;
                            break;
                        }
                    }
                    if (end) _isDestroyed = true;
                }
                else
                {
                    if (!IsInPlaying) _isDestroyed = true;
                }
            }
            else if (BelongMagic.MoveKind == 15)
            {
                if (!IsInPlaying)
                    Destroy();
            }
            else if (BelongMagic.MoveKind == 17)
            {
                //do nothing
            }
            else
            {
                if (BelongMagic.MoveKind != 13)
                {
                    CheckCharacterHited();
                }

                if (Globals.TheMap.IsObstacleForMagic(MapX, MapY))
                {
                    Destroy();
                }
                else if (!IsInPlaying)
                {
                    if (_destroyOnEnd)
                    {
                        Destroy();
                    }
                    else _isDestroyed = true;
                }
            }

            if (_destoryedLeapSprites != null)
            {
                for (var node = _destoryedLeapSprites.First; node != null;)
                {
                    var next = node.Next;
                    var value = node.Value;

                    value.Update(gameTime);
                    if (!value.IsInPlaying)
                    {
                        _destoryedLeapSprites.Remove(node);
                    }

                    node = next;
                }
            }

            base.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if (IsDestroyed) return;
            var color = DrawColor;

            //When rain make magic sprite has normal light
            if (WeatherManager.IsRaining) color = Color.White;

            if (BelongMagic.MoveKind == 15 && IsInDestroy)
            {
                foreach (var sprite in _superModeDestroySprites)
                {
                    sprite.Draw(spriteBatch, color);
                }
            }

            if (_destoryedLeapSprites != null)
            {
                foreach (var sprite in _destoryedLeapSprites)
                {
                    sprite.Draw(spriteBatch, color);
                }
            }

            base.Draw(spriteBatch, color);
        }
    }
}
