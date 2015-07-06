using System;
using System.Collections.Generic;
using System.Linq;
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
        private Vector2 _destnationPixelPosition;
        private LinkedList<Vector2> _paths;
        private bool _isDestroyed;
        private bool _isInDestroy;
        private bool _destroyOnEnd;
        private LinkedList<Sprite> _superModeDestroySprites;
        private List<Character> _leapedCharacters;
        private Character _closedCharecter;
        private float _elapsedMilliSeconds;
        private bool _isOneSecond;

        private int _leftLeapTimes;
        private int _currentEffect;
        private bool _canLeap;

        #region Kind 18
        private double _kind18elapsedMilliSeconds;
        #endregion Kind18


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
            if (Init(belongMagic, belongCharacter, positionInWorld, velocity, moveDirection, destroyOnEnd))
            {
                Begin();
            }
        }


        /// <summary>
        /// Fixed position magic
        /// </summary>
        /// <param name="belongMagic"></param>
        /// <param name="belongCharacter"></param>
        /// <param name="positionInWorld"></param>
        /// <param name="direction"></param>
        /// <param name="destroyOnEnd"></param>
        public MagicSprite(Magic belongMagic, Character belongCharacter, Vector2 positionInWorld, int direction, bool destroyOnEnd)
        {
            if (Init(belongMagic, belongCharacter, positionInWorld, 0, Vector2.Zero, destroyOnEnd))
            {
                SetDirection(direction);
                Begin();
            }
        }
        #endregion Ctor

        private bool Init(Magic belongMagic, Character belongCharacter, Vector2 positionInWorld, float velocity,
            Vector2 moveDirection, bool destroyOnEnd)
        {
            if (belongMagic == null || belongCharacter == null)
            {
                _isDestroyed = true;
                return false;
            }
            _destnationPixelPosition = positionInWorld;
            var texture = belongMagic.FlyingImage;
            switch (belongMagic.MoveKind)
            {
                case 15:
                    texture = belongMagic.SuperModeImage;
                    break;
                case 20:
                    positionInWorld = Map.ToPixelPosition(belongCharacter.TilePosition);
                    break;
                case 21:
                {
                    var player = belongCharacter as Player;
                    if (player != null &&
                        player.ControledCharacter != null)
                    {
                        player.ControledCharacter.ControledMagicSprite = this;
                    }
                    else
                    {
                        throw new Exception("Magic kind 21 internal error.");
                    }
                }
                    break;
            }
            Set(positionInWorld, velocity, texture, 0);
            BelongMagic = belongMagic;
            BelongCharacter = belongCharacter;
            MoveDirection = moveDirection;
            _destroyOnEnd = destroyOnEnd;
            SetDirection(MoveDirection);
            return true;
        }

        private void CharacterHited(Character character)
        {
            if (character == null) return;

            //Apply magic special effect
            switch (BelongMagic.SpecialKind)
            {
                case 1:
                case 7:
                    if (!character.IsFrozened)
                        character.FrozenSeconds = BelongMagic.CurrentLevel + 1;
                    break;
                case 2:
                case 8:
                    if (!character.IsPoisoned)
                        character.PoisonSeconds = BelongMagic.CurrentLevel + 1;
                    break;
                case 3:
                case 9:
                    if (!character.IsPetrified)
                        character.PetrifiedSeconds = BelongMagic.CurrentLevel + 1;
                    break;
            }

            if (BelongMagic.MoveKind == 13)
            {
                //Kind 13 magic only have special effect
                return;
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

                var amount = _canLeap ? _currentEffect : MagicManager.GetEffectAmount(BelongMagic, BelongCharacter);

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
                                //Target character have protecter
                                var manaReduce = MagicManager.GetEffectAmount(magic, character);
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
                if (effect > character.Life)
                {
                    //Effect amount should less than or equal target character current life amount.
                    effect = character.Life;
                }
                character.DecreaseLifeAddHurt(effect);

                //Restore
                if (BelongMagic.RestoreProbability > 0 &&
                Globals.TheRandom.Next(0, 100) < BelongMagic.RestoreProbability)
                {
                    var restoreAmount = (effect * BelongMagic.RestorePercent) / 100;
                    switch ((Magic.RestorePropertyType)BelongMagic.RestoreType)
                    {
                        case Magic.RestorePropertyType.Life:
                            BelongCharacter.AddLife(restoreAmount);
                            break;
                        case Magic.RestorePropertyType.Mana:
                            BelongCharacter.AddMana(restoreAmount);
                            break;
                        case Magic.RestorePropertyType.Thew:
                            BelongCharacter.AddThew(restoreAmount);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
            }

            if (BelongCharacter.IsPlayer)
            {
                var player = BelongCharacter as Player;
                if (player != null)
                {
                    var amount = Utils.GetMagicExp(character.Level);
                    player.AddMagicExp(BelongMagic.ItemInfo, amount);
                    if (player.XiuLianMagic != null &&
                        player.XiuLianMagic != BelongMagic.ItemInfo)
                    {
                        player.AddMagicExp(player.XiuLianMagic, amount);
                    }
                }
            }

            if (_canLeap)
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
            _currentEffect = MagicManager.GetEffectAmount(BelongMagic, BelongCharacter);

            if (_leftLeapTimes > 0)
            {
                //Initilize leap
                _leapedCharacters = new List<Character>();
                _canLeap = true;
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
                List<Character> targets;
                if (BelongCharacter.IsPlayer || BelongCharacter.IsFighterFriend)
                {
                    targets = NpcManager.NpcsInView.Where(npc => npc.IsEnemy).Cast<Character>().ToList();
                }
                else
                {
                    targets = NpcManager.NpcsInView.Where(npc => npc.IsFighterFriend).Cast<Character>().ToList();
                    targets.Add(Globals.ThePlayer);
                }
                foreach (var character in targets)
                {
                    AddDestroySprite(_superModeDestroySprites,
                        character.PositionInWorld,
                        BelongMagic.VanishImage,
                        BelongMagic.VanishSound);
                    CharacterHited(character);
                }
                if (_superModeDestroySprites.Count == 0) _isDestroyed = true;
            }
            else
            {
                switch (BelongMagic.MoveKind)
                {
                    case 20:
                    {
                        BelongCharacter.IsInTransport = false;
                        var tilePosition = Map.ToTilePosition(_destnationPixelPosition);
                        if (!BelongCharacter.HasObstacle(tilePosition) &&
                            !Globals.TheMap.IsObstacleForCharacter(tilePosition))
                        {
                            //Destination has no obstacle, transport magic user.
                            TilePosition = tilePosition;
                            BelongCharacter.SetTilePosition(tilePosition);
                        }
                    }
                        break;
                    case 21:
                    {
                        var player = BelongCharacter as Player;
                        if (player == null)
                        {
                            throw new Exception("Magic kind 21 internal error.");
                        }
                        player.EndControlCharacter();
                    }
                        break;
                }

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

                if (BelongMagic.VibratingScreen > 0)
                {
                    Globals.TheCarmera.VibaratingScreen(BelongMagic.VibratingScreen);
                }

                MoveDirection = Vector2.Zero;
            }
        }

        private void LeapToNextTarget(Character hitedCharacter)
        {
            if (_leftLeapTimes > 0)
            {
                _leftLeapTimes--;
                _currentEffect -= _currentEffect * BelongMagic.EffectReducePercentage / 100;
            }
            else
            {
                EndLeap();
            }

            if (BelongMagic.VanishImage != null)
            {
                AddDestroySprite(MagicManager.EffectSprites, PositionInWorld, BelongMagic.VanishImage, BelongMagic.VanishSound);
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
            _isDestroyed = true;
        }

        private void UpdateTime(GameTime gameTime)
        {
            _isOneSecond = false;
            _elapsedMilliSeconds += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (_elapsedMilliSeconds >= 1000)
            {
                _elapsedMilliSeconds -= 1000;
                _isOneSecond = true;
            }
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
                var interval = Interval == 0 ? 1000 / 60 : Interval;
                framesToPlay = (int)(BelongMagic.LifeFrame * 10f / interval);
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

                UpdateTime(gameTime);
                switch (BelongMagic.SpecialKind)
                {
                    case 4:
                        if (_isOneSecond)
                        {
                            BelongCharacter.AddLife(BelongMagic.SpecialKindValue);
                        }
                        break;
                    case 5:
                        if (_isOneSecond)
                        {
                            BelongCharacter.AddMana(BelongMagic.SpecialKindValue);
                        }
                        break;
                    case 6:
                        if (_isOneSecond)
                        {
                            BelongCharacter.AddThew(BelongMagic.SpecialKindValue);
                        }
                        break;
                    case 7:
                    case 8:
                    case 9:
                        foreach (var target in NpcManager.FindEnemiesInTileDistance(BelongCharacter, BelongMagic.SpecialKindValue))
                        {
                            CharacterHited(target);
                        }
                        break;
                }
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
                switch (BelongMagic.MoveKind)
                {
                    case 13:
                    case 20: //transport
                    case 21: //Controling others
                        break;
                    case 18://Fly magic
                        {
                            _kind18elapsedMilliSeconds += gameTime.ElapsedGameTime.TotalMilliseconds;
                            if (_kind18elapsedMilliSeconds >= BelongMagic.FlyInterval)
                            {
                                _kind18elapsedMilliSeconds -= BelongMagic.FlyInterval;
                                MagicManager.UseMagic(BelongCharacter, BelongMagic.FlyMagic, PositionInWorld, PositionInWorld + _moveDirection);
                            }
                        }
                        break;
                    default:
                        CheckCharacterHited();
                        break;
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

            base.Draw(spriteBatch, color);
        }
    }
}
