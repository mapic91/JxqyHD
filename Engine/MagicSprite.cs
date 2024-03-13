using System;
using System.Collections.Generic;
using System.Linq;
using Engine.ListManager;
using Engine.Map;
using Engine.Weather;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Engine
{
    public class MagicSprite : Sprite
    {
        private class RoundMoveInfo
        {
            public float CurDegree;
        }

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
        private List<Character> _carrayUser4Characters = new List<Character>();
        private List<Character> _passThroughedCharacters;
        private Character _closedCharecter;
        private float _flyMagicElapsedMilliSeconds;
        private float _summonElapsedMilliseconds;
        private float _rangeElapsedMilliseconds;
        private float _waitMilliSeconds;

        private RoundMoveInfo _roundMoveInfo;

        private int _leftLeapTimes;
        private int _currentEffect;
        private int _currentEffect2;
        private int _currentEffect3;
        private int _currentEffectMana;
        private bool _canLeap;

        private Vector2 _lastTilePosition;

        private Npc _summonedNpc;

        private bool _isInMoveBack;

        private Character _stickedCharacter;

        private Character _parasitiferCharacter;
        private float _parasticTime;
        private int _totalParasticEffect;

        private Vector2 _lastUserWorldPosition;

        private Vector2 _circleMoveDir;

        private int _index;

        public const int MinimalDamage = 5;

        private double _elaspedMillisecond;

        public int Index
        {
            get { return _index; }
            set
            {
                _index = value;

                if (BelongMagic != null && BelongCharacter != null)
                {
                    if (BelongMagic.CarryUser > 0 && BelongMagic.CarryUserSpriteIndex == value)
                    {
                        BelongCharacter.MovedByMagicSprite = this;
                    }

                    if (BelongMagic.RoundMoveColockwise > 0 || BelongMagic.RoundMoveAnticlockwise > 0)
                    {
                        _roundMoveInfo = new RoundMoveInfo();
                        _roundMoveInfo.CurDegree = (BelongMagic.RoundMoveColockwise > 0 ? -1 : 1) *
                                                   (360f * _index / BelongMagic.RoundMoveCount);
                        SetRoundMove();
                    }
                }
            }
        }

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

        /// <summary>
        /// Normalized or Zero
        /// </summary>
        public Vector2 MoveDirection
        {
            get { return _moveDirection; }
            set
            {
                if (value != Vector2.Zero)
                {
                    value.Normalize();
                }
                _moveDirection = value;
            }
        }

        public Vector2 RealMoveDirection
        { get { return _moveDirection + _circleMoveDir; } }

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

        public bool CanDiscard
        {
            get
            {
                return _stickedCharacter == null && _parasitiferCharacter == null && BelongMagic.MoveKind != 13 &&
                       BelongMagic.MoveKind != 15 && BelongMagic.MoveKind != 21 && BelongMagic.MoveKind != 23;
            }
        }

        public bool CanExchangeUser
        {
            get {
                return _stickedCharacter == null && _parasitiferCharacter == null && BelongMagic.MoveKind != 13 &&
                       BelongMagic.MoveKind != 15 && BelongMagic.MoveKind != 20 && BelongMagic.MoveKind != 21 && BelongMagic.MoveKind != 22 && BelongMagic.MoveKind != 23;
            }
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
            if (Init(belongMagic, belongCharacter, positionInWorld, belongMagic.Speed * Globals.MagicBasespeed, Vector2.Zero, destroyOnEnd))
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

            if (belongMagic.NoExplodeWhenLifeFrameEnd > 0)
            {
                destroyOnEnd = false;
            }
            else if (belongMagic.ExplodeWhenLifeFrameEnd > 0)
            {
                destroyOnEnd = true;
            }

            _destnationPixelPosition = positionInWorld;
            var texture = belongMagic.FlyingImage;
            switch (belongMagic.MoveKind)
            {
                case 15:
                    texture = belongMagic.SuperModeImage;
                    break;
                case 20:
                    positionInWorld = MapBase.ToPixelPosition(belongCharacter.TilePosition);
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
                case 22://Summon
                    {
                        var tilePosition = MapBase.ToTilePosition(positionInWorld);
                        var finded = PathFinder.FindNonobstacleNeighborOrItself(belongCharacter, ref tilePosition);
                        if (finded)
                        {
                            positionInWorld = MapBase.ToPixelPosition(tilePosition);
                        }
                        else
                        {
                            _isDestroyed = true;
                            break;
                        }
                        if (belongCharacter.SummonedNpcsCount(belongMagic) >= belongMagic.MaxCount)
                        {
                            //Reach max count
                            belongCharacter.RemoveFirstSummonedNpc(belongMagic);
                        }
                        var npc = NpcManager.AddNpc(belongMagic.NpcFile,
                            (int)tilePosition.X,
                            (int)tilePosition.Y,
                            Utils.GetDirectionIndex(positionInWorld - belongCharacter.PositionInWorld, 8));
                        belongCharacter.AddSummonedNpc(belongMagic, npc);
                        if (belongCharacter.IsPlayer || belongCharacter.IsFighterFriend)
                        {
                            npc.Relation = (int)Character.RelationType.Friend;
                        }
                        else
                        {
                            npc.Kind = (int)Character.CharacterKind.Fighter;
                            npc.Relation = belongCharacter.Relation;
                        }
                        npc.SummonedByMagicSprite = this;
                        _summonedNpc = npc;
                    }
                    break;
            }
            
            Set(positionInWorld, velocity, texture, 0);
            BelongMagic = belongMagic;
            BelongCharacter = belongCharacter;
            MoveDirection = moveDirection;
            _destroyOnEnd = destroyOnEnd;
            SetDirection(MoveDirection);
            _lastUserWorldPosition = belongCharacter.PositionInWorld;
            _rangeElapsedMilliseconds = belongMagic.RangeTimeInerval;

            if (BelongMagic.MeteorMove > 0)
            {
                _waitMilliSeconds = Globals.TheRandom.Next(1000);
                var dir = (BelongMagic.MeteorMoveDir > 7) ? Globals.TheRandom.Next(8) : BelongMagic.MeteorMoveDir;
                var path = new LinkedList<Vector2>();
                path.AddFirst(positionInWorld);
                var tile = MapBase.ToTilePosition(positionInWorld, false);
                for (var i = 0; i <= BelongMagic.MeteorMove; i++)
                {
                    tile = PathFinder.FindNeighborInDirection(tile, dir);
                    path.AddFirst(MapBase.ToPixelPosition(tile, false));
                }
                SetPath(path);
                PositionInWorld = path.First.Value;
                Velocity = belongMagic.Speed * Globals.MagicBasespeed;
            }

            if (!string.IsNullOrEmpty(BelongMagic.MagicWhenNewPos))
            {
                _lastTilePosition = TilePosition;
            }

            return true;
        }

        private bool CharacterHited(Character character)
        {
            if (character == null) return false;

            var destroy = true;

            if (BelongMagic.CarryUser == 4 && BelongCharacter.MovedByMagicSprite == this)
            {
                if (_carrayUser4Characters.Exists(c => c == character))
                {
                    return false;
                }
                _carrayUser4Characters.Add(character);
                character.MovedByMagicSprite = this;
                character.MovedByMagicSpriteOffset = character.PositionInWorld - this.PositionInWorld;

                destroy = false;
            }

            if(BelongMagic.BlindMilliseconds > 0)
            {
                var npc = character as Npc;
                if(npc != null)
                {
                    npc.BlindMilliseconds = BelongMagic.BlindMilliseconds;
                }
            }

            if(BelongMagic.DisableMoveMilliseconds > 0)
            {
                character.DisableMoveMilliseconds = BelongMagic.DisableMoveMilliseconds;
            }

            if(BelongMagic.DisableSkillMilliseconds > 0)
            {
                character.DisableSkillMilliseconds = BelongMagic.DisableSkillMilliseconds;
            }

            if (BelongMagic.Bounce > 0)
            {
                var direction = (RealMoveDirection == Vector2.Zero) ? (character.PositionInWorld - PositionInWorld) : RealMoveDirection;
                if (direction != Vector2.Zero)
                {
                    var velocity = BelongMagic.Bounce;
                    if (character.BouncedVelocity > 0)
                    {
                        direction.Normalize();
                        direction = (character.BouncedDirection * character.BouncedVelocity + direction * BelongMagic.Bounce);
                        velocity = (int)direction.Length();
                    }

                    character.BoundByMagicSprite = this;
                    character.BouncedDirection = direction;
                    character.BouncedVelocity = velocity;
                    character.StandingImmediately();
                }

            }

            if (BelongMagic.BounceFly > 0)
            {
                var direction = (RealMoveDirection == Vector2.Zero) ? (character.PositionInWorld - PositionInWorld) : RealMoveDirection;
                if (direction != Vector2.Zero)
                {
                    var endTile =
                        PathFinder.FindDistanceTileInDirection(character.TilePosition, direction, BelongMagic.BounceFly);
                    character.BezierMoveTo(endTile, BelongMagic.BounceFlySpeed, cha =>
                    {
                        if (BelongMagic.BounceFlyEndMagic != null)
                        {
                            Vector2 pos = BelongCharacter.PositionInWorld;
                            if (BelongMagic.MagicDirectionWhenBounceFlyEnd == 1)
                            {
                                pos = character.PositionInWorld + Utils.GetDirection8(character.CurrentDirection);
                            }
                            else if (BelongMagic.MagicDirectionWhenBounceFlyEnd == 2)
                            {
                                pos = character.PositionInWorld + Utils.GetDirection8(BelongCharacter.CurrentDirection);
                            }
                            MagicManager.UseMagic(BelongCharacter, BelongMagic.BounceFlyEndMagic, character.PositionInWorld, pos);
                        }

                        if (BelongMagic.BounceFlyEndHurt > 0)
                        {
                            character.DecreaseLifeAddHurt(BelongMagic.BounceFlyEndHurt);
                        }

                        if (BelongMagic.BounceFlyTouchHurt > 0)
                        {
                            var neighbors = PathFinder.FindAllNeighbors(character.TilePosition);
                            neighbors.Add(character.TilePosition);
                            foreach (var neighbor in neighbors)
                            {
                                var npc = NpcManager.GetFighter(neighbor);
                                if (npc != null  && npc != character && NpcManager.IsEnemy(npc, BelongCharacter))
                                {
                                    endTile =
                                        PathFinder.FindDistanceTileInDirection(npc.TilePosition, direction, BelongMagic.BounceFly);
                                    npc.BezierMoveTo(endTile, BelongMagic.BounceFlySpeed, null);
                                    character.DecreaseLifeAddHurt(BelongMagic.BounceFlyTouchHurt);
                                    npc.DecreaseLifeAddHurt(BelongMagic.BounceFlyTouchHurt);
                                }
                            }
                        }
                    });
                }
            }

            if (BelongMagic.Ball > 0)
            {
                destroy = false;
                MoveDirection = PathFinder.BouncingAtPoint(RealMoveDirection, PositionInWorld, character.PositionInWorld);
                //Hitted once, Move magic sprite to neighber tile
                TilePosition = PathFinder.FindNeighborInDirection(TilePosition, RealMoveDirection);
                PositionInWorld -= (RealMoveDirection == Vector2.Zero
                    ? Vector2.Zero
                    : Vector2.Normalize(RealMoveDirection));
                AddDestroySprite(MagicManager.EffectSprites, PositionInWorld, BelongMagic.VanishImage, BelongMagic.VanishSound);
            }

            if (BelongMagic.Sticky > 0)
            {
                destroy = false;
                character.StandingImmediately();
                character.MovedByMagicSprite = this;
                _stickedCharacter = character;
                if (BelongMagic.MoveBack > 0 && _isInMoveBack == false)
                {
                    _isInMoveBack = true;
                }
            }

            if (BelongMagic.Parasitic > 0)
            {
                _parasitiferCharacter = character;
                destroy = true;
            }

            if (BelongMagic.ChangeToFriendMilliseconds > 0 && BelongMagic.MaxLevel >= character.Level)
            {
                character.ChangeToOpposite(BelongMagic.ChangeToFriendMilliseconds);
            }

            if (BelongMagic.WeakMilliseconds > 0)
            {
                character.WeakBy(this);
            }

            if (BelongMagic.MorphMilliseconds > 0)
            {
                character.MorphBy(this);
            }

            //Apply magic special effect
            switch (BelongMagic.SpecialKind)
            {
                case 1:
                    character.SetFrozenSeconds(BelongMagic.SpecialKindMilliSeconds >0 ? BelongMagic.SpecialKindMilliSeconds/1000.0f : BelongMagic.EffectLevel + 1, BelongMagic.NoSpecialKindEffect == 0);
                    break;
                case 2:
                    character.SetPoisonSeconds(BelongMagic.SpecialKindMilliSeconds > 0 ? BelongMagic.SpecialKindMilliSeconds / 1000.0f : BelongMagic.EffectLevel + 1, BelongMagic.NoSpecialKindEffect == 0);
                    if (BelongCharacter is Player || BelongCharacter.IsPartner)
                    {
                        character.PoisonByCharacterName = BelongCharacter.Name;
                    }
                    break;
                case 3:
                    character.SetPetrifySeconds(BelongMagic.SpecialKindMilliSeconds > 0 ? BelongMagic.SpecialKindMilliSeconds / 1000.0f : BelongMagic.EffectLevel + 1, BelongMagic.NoSpecialKindEffect == 0);
                    break;
            }

            //Additional attack effect added to magic when player equip special equipment
            switch (BelongMagic.AdditionalEffect)
            {
                case Magic.AddonEffect.Frozen:
                    if (!character.IsFrozened)
                        character.SetFrozenSeconds(BelongCharacter.Level / 10 + 1, BelongMagic.NoSpecialKindEffect == 0);
                    break;
                case Magic.AddonEffect.Poision:
                    if (!character.IsPoisoned)
                    {
                        character.SetPoisonSeconds(BelongCharacter.Level / 10 + 1, BelongMagic.NoSpecialKindEffect == 0);
                        if (BelongCharacter is Player || BelongCharacter.IsPartner)
                        {
                            character.PoisonByCharacterName = BelongCharacter.Name;
                        }
                    }
                    break;
                case Magic.AddonEffect.Petrified:
                    if (!character.IsPetrified)
                        character.SetPetrifySeconds(BelongCharacter.Level / 10 + 1, BelongMagic.NoSpecialKindEffect == 0);
                    break;
            }

            var amount = _canLeap ? _currentEffect : MagicManager.GetEffectAmount(BelongMagic, BelongCharacter);
            var amount2 = _canLeap ? _currentEffect2 : MagicManager.GetEffectAmount2(BelongMagic, BelongCharacter);
            var amount3 = _canLeap ? _currentEffect3 : MagicManager.GetEffectAmount3(BelongMagic, BelongCharacter);
            var amountMana = _canLeap ? _currentEffectMana : BelongMagic.EffectMana;
            CharacterHited(character, amount, amount2, amount3, amountMana);

            if (_canLeap)
            {
                LeapToNextTarget(character);
            }
            else if (BelongMagic.PassThrough > 0)
            {
                if (BelongMagic.PassThroughWithDestroyEffect > 0)
                {
                    AddDestroySprite(MagicManager.EffectSprites, PositionInWorld, BelongMagic.VanishImage, BelongMagic.VanishSound);
                }
                if (Velocity > 0 && RealMoveDirection != Vector2.Zero)
                {
                    //Hit once, move magic sprite to neighber tile
                    TilePosition = PathFinder.FindNeighborInDirection(TilePosition, RealMoveDirection);
                }
            }
            else if (destroy)
            {
                Destroy();
            }

            return true;
        }

        private void CharacterHited(Character character, int damage, int damage2, int damage3, int damageMana, bool addMagicHitedExp = true)
        {
            var isInDeath = character.IsDeathInvoked;
            character.LastAttackerMagicSprite = this;

            character.NotifyFighterAndAllNeighbor(BelongCharacter);

            //Hit ratio
            var targetEvade = character.RealEvade;
            var belongCharacterEvade = BelongCharacter.RealEvade;
            const float maxOffset = 100f;
            const float baseHitRatio = 0.05f;
            const float belowRatio = 0.5f;
            const float upRatio = 0.45f;
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

            if (_parasitiferCharacter != null || Globals.TheRandom.Next(101) <= (int)(hitRatio * 100f))
            {
                var effect3 = damage3 - character.Defend3;
                var effect2 = damage2 - character.Defend2;
                var effect = (damage - character.RealDefend);
                foreach (var magicSprite in character.MagicSpritesInEffect)
                {
                    var magic = magicSprite.BelongMagic;
                    switch (magic.MoveKind)
                    {
                        case 13:
                            if (magic.SpecialKind == 3)
                            {
                                //Target character have protecter
                                var damageReduce = MagicManager.GetEffectAmount(magic, character);
                                var damageReduce2 = MagicManager.GetEffectAmount2(magic, character);
                                var damageReduce3 = MagicManager.GetEffectAmount3(magic, character);
                                effect3 -= damageReduce3;
                                effect2 -= damageReduce2;
                                effect -= damageReduce;
                            }
                            break;
                    }
                }

                if (effect3 > 0) effect += effect3;
                if (effect2 > 0) effect += effect2;

                if (effect > character.Life)
                {
                    //Effect amount should less than or equal target character current life amount.
                    effect = character.Life;
                }
                character.DecreaseLifeAddHurt(effect < MinimalDamage ? MinimalDamage : effect);
                character.DecreaseMana(damageMana);

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

            if (_parasitiferCharacter != null)
            {
                _totalParasticEffect += damage;
            }

            if (character.MagicToUseWhenBeAttacked != null)
            {
                Vector2 destination;
                Character target = null;
                var dirType = (Character.BeAttackedUseMagicDirection) character.MagicDirectionWhenBeAttacked;
                switch (dirType)
                {
                    case Character.BeAttackedUseMagicDirection.Attacker:
                        destination = BelongCharacter.PositionInWorld;
                        target = BelongCharacter;
                        break;
                    case Character.BeAttackedUseMagicDirection.MagicSpriteOppDirection:
                        destination = RealMoveDirection == Vector2.Zero
                            ? (character.PositionInWorld + Utils.GetDirection8(character.CurrentDirection))
                            : (character.PositionInWorld - RealMoveDirection);
                        break;
                    case Character.BeAttackedUseMagicDirection.CurrentNpcDirection:
                        destination = character.PositionInWorld + Utils.GetDirection8(character.CurrentDirection);
                        break;
                    default:
                        destination = BelongCharacter.PositionInWorld;
                        break;
                }
                MagicManager.UseMagic(character, character.MagicToUseWhenBeAttacked, character.PositionInWorld, destination, target);
            }

            foreach(var info in character.MagicToUseWhenAttackedList)
            {
                Vector2 destination;
                Character target = null;
                var dirType = (Character.BeAttackedUseMagicDirection)info.Dir;
                switch (dirType)
                {
                    case Character.BeAttackedUseMagicDirection.Attacker:
                        destination = BelongCharacter.PositionInWorld;
                        target = BelongCharacter;
                        break;
                    case Character.BeAttackedUseMagicDirection.MagicSpriteOppDirection:
                        destination = RealMoveDirection == Vector2.Zero
                            ? (character.PositionInWorld + Utils.GetDirection8(character.CurrentDirection))
                            : (character.PositionInWorld - RealMoveDirection);
                        break;
                    case Character.BeAttackedUseMagicDirection.CurrentNpcDirection:
                        destination = character.PositionInWorld + Utils.GetDirection8(character.CurrentDirection);
                        break;
                    default:
                        destination = BelongCharacter.PositionInWorld;
                        break;
                }
                MagicManager.UseMagic(character, info.Magic, character.PositionInWorld, destination, target);
            }

            {
                Player player = null;
                MagicListManager.MagicItemInfo info = null;
                if (BelongCharacter.IsPlayer)
                {
                    player = BelongCharacter as Player;
                    info = BelongMagic.ItemInfo;

                    if (info == null)
                    {
                        if (player != null) info = player.CurrentMagicInUse;
                    }
                }
                else if (BelongCharacter.ControledMagicSprite != null)
                {
                    player = BelongCharacter.ControledMagicSprite.BelongCharacter as Player;
                    if (player != null)
                    {
                        info = player.CurrentMagicInUse;
                    }
                }
                else if (BelongCharacter.SummonedByMagicSprite != null && BelongCharacter.SummonedByMagicSprite.BelongCharacter.IsPlayer)
                {
                    //Summoned by player, add player's exp
                    player = BelongCharacter.SummonedByMagicSprite.BelongCharacter as Player;
                    if (player != null)
                    {
                        info = player.CurrentMagicInUse;
                    }
                }
                if (addMagicHitedExp && player != null && info != null)
                {
                    var amount = Utils.GetMagicExp(character.Level);
                    player.AddMagicExp(info, amount);
                }
            }

            //Exp
            if (BelongCharacter.IsPlayer || BelongCharacter.IsFighterFriend)
            {
                var isSummonedByPlayerorPartner = (BelongCharacter.SummonedByMagicSprite != null &&
                                                   (BelongCharacter.SummonedByMagicSprite.BelongCharacter.IsPlayer ||
                                                    BelongCharacter.SummonedByMagicSprite.BelongCharacter.IsPartner));
                var isControledByPlayer = (BelongCharacter.ControledMagicSprite != null &&
                                           BelongCharacter.ControledMagicSprite.BelongCharacter.IsPlayer);
                var isKill = !isInDeath && //Alive before hited
                             (character.IsInDeathing || character.IsDeath);
                if (isKill)
                {
                    if ((BelongCharacter.IsPlayer ||
                         BelongCharacter.IsPartner ||
                         isSummonedByPlayerorPartner ||
                         isControledByPlayer))
                    {
                        var player = Globals.ThePlayer;
                        var exp = Utils.GetCharacterDeathExp(Globals.ThePlayer, character);
                        player.AddExp(exp, true);
                        if (BelongCharacter.CanLevelUp > 0 && 
                            ((isSummonedByPlayerorPartner && BelongCharacter.SummonedByMagicSprite.BelongCharacter.IsPartner) || 
                             BelongCharacter.IsPartner))
                        {
                            var npcExp = Utils.GetCharacterDeathExp(BelongCharacter, character);
                            BelongCharacter.AddExp(npcExp);
                        }
                    }
                    
                    if (BelongMagic.MagicToUseWhenKillEnemy != null)
                    {
                        Vector2 pos = BelongCharacter.PositionInWorld;
                        if (BelongMagic.MagicDirectionWhenKillEnemy == 1)
                        {
                            pos = character.PositionInWorld + Utils.GetDirection8(character.CurrentDirection);
                        }
                        else if (BelongMagic.MagicDirectionWhenKillEnemy == 2)
                        {
                            pos = character.PositionInWorld + Utils.GetDirection8(BelongCharacter.CurrentDirection);
                        }
                        MagicManager.UseMagic(BelongCharacter, BelongMagic.MagicToUseWhenKillEnemy, character.PositionInWorld, pos);
                    }
                }
            }
        }

        private void CollisionDetaction()
        {
            if (_stickedCharacter != null && _stickedCharacter.MovedByMagicSprite == this)
            {
                //No character hited checking when sticking character
                return;
            }
            else
            {
                //Clear sticked character
                _stickedCharacter = null;
            }

            if (_parasitiferCharacter != null)
            {
                // Magic sprite finded its parasitifer.
                return;
            }

            if(BelongMagic.CarryUser == 3)
            {
                //Pass through enemy when CarryUser is equal 3
                return;
            }

            if (BelongMagic.CarryUser == 4)
            {
                foreach (var character in _carrayUser4Characters.ToArray())
                {
                    var characters = NpcManager.FindEnemiesInTileDistance(BelongCharacter, character.TilePosition, 1);
                    foreach (var collision in characters)
                    {
                        if (!_carrayUser4Characters.Exists(c => c == collision))
                        {
                            CharacterHited(collision);
                        }
                    }
                }
            }

            bool characterHited = false;
            if (BelongMagic.AttackAll > 0)
            {
                characterHited = CharacterHited(CanCollide(NpcManager.GetFighter(TilePosition)));
            }
            else if (BelongCharacter.IsPlayer || BelongCharacter.IsFighterFriend)
            {
                var target = NpcManager.GetEnemy(TilePosition, true);
                characterHited = CharacterHited(CanCollide(target));
            }
            else if (BelongCharacter.IsEnemy)
            {
                var target = NpcManager.GetPlayerOrFighterFriend(TilePosition, true);
                if(target == null)
                {
                    target = NpcManager.GetOtherGropEnemy(BelongCharacter.Group, TilePosition);
                }
                characterHited = CharacterHited(CanCollide(target));
            }
            else if (BelongCharacter.IsNoneFighter)
            {
                characterHited = CharacterHited(CanCollide(NpcManager.GetNonneutralFighter(TilePosition)));
            }

            if (!characterHited && !CheckMagicDiscard())
            {
                CheckMagicExchangeUser();
            }
        }

        private Character CanCollide(Character character)
        {
            if (character != null)
            {
                if (BelongMagic.PassThrough > 0)
                {
                    if (_passThroughedCharacters.Contains(character))
                    {
                        return null;
                    }
                    _passThroughedCharacters.Add(character);
                }
            }

            return character;
        }

        private bool CheckMagicDiscard()
        {
            if (BelongMagic.DiscardOppositeMagic > 0)
            {
                foreach (var magicSprite in MagicManager.MagicSpritesList)
                {
                    if (magicSprite.TilePosition == TilePosition && 
                        IsOpposite(magicSprite) && 
                        magicSprite.CanDiscard)
                    {
                        magicSprite.SetDestroyed();
                        SetDestroyed();
                        return true;
                    }
                }
            }

            return false;
        }

        private bool CheckMagicExchangeUser()
        {
            if (BelongMagic.ExchangeUser > 0)
            {
                foreach (var magicSprite in MagicManager.MagicSpritesList)
                {
                    if (magicSprite.TilePosition == TilePosition  && 
                        IsOpposite(magicSprite) && 
                        magicSprite.CanExchangeUser)
                    {
                        magicSprite.BelongCharacter = BelongCharacter;
                        var dir = magicSprite.MoveDirection * magicSprite.Velocity +
                                           MoveDirection * Velocity;
                        magicSprite.MoveDirection = dir;
                        magicSprite.Velocity = dir.Length();
                        SetDestroyed();
                    }
                }
            }

            return false;
        }

        public bool IsOpposite(MagicSprite magicSprite)
        {
            return BelongCharacter.IsOpposite(magicSprite.BelongCharacter);
        }

        public bool IsOpposite(Character character)
        {
            return BelongCharacter.IsOpposite(character);
        }

        private void Begin()
        {
            _leftLeapTimes = BelongMagic.LeapTimes;
            _currentEffect = MagicManager.GetEffectAmount(BelongMagic, BelongCharacter);
            _currentEffect2 = MagicManager.GetEffectAmount2(BelongMagic, BelongCharacter);
            _currentEffect3 = MagicManager.GetEffectAmount3(BelongMagic, BelongCharacter);
            _currentEffectMana = BelongMagic.EffectMana;

            if (_leftLeapTimes > 0)
            {
                //Initilize leap
                _leapedCharacters = new List<Character>();
                _canLeap = true;
            }

            if (BelongMagic.PassThrough > 0)
            {
                _passThroughedCharacters = new List<Character>();
            }

            //Start play FlyingImage
            ResetPlay();

            if (Velocity > 0 && MoveDirection != Vector2.Zero)//Move 30
            {
                var second = 30f / Velocity;
                MoveToNoNormalizeDirection(MoveDirection, second);
            }
            else
            {
                // can't put fixed position magic sprite in obstacle
                if (CheckDestroyForObstacleInMap())
                    _isDestroyed = true;
            }
        }

        private void AddDestroySprite(LinkedList<Sprite> list, Vector2 positionInWorld, Asf image, SoundEffect sound)
        {
            if (image != null)
            {
                var sprite = new Sprite(positionInWorld,
                           0f,
                           image);
                sprite.PlayFrames(sprite.FrameCountsPerDirection);
                list.AddLast(sprite);
            }
            SoundManager.Play3DSoundOnece(sound,
                positionInWorld - Globals.ListenerPosition);

            UseMagic(BelongMagic.ExplodeMagicFile);
        }

        private bool CheckDestroyForObstacleInMap()
        {
            var destroy = (BelongMagic.PassThroughWall == 0 && MapBase.Instance.IsObstacleForMagic(TilePosition));
            if (destroy && BelongMagic.Ball > 0)
            {
                MoveDirection = PathFinder.BouncingAtWall(RealMoveDirection, PositionInWorld, TilePosition);
                //Hitted once, Move magic sprite to neighber tile
                TilePosition = PathFinder.FindNeighborInDirection(TilePosition, RealMoveDirection);
                PositionInWorld -= (RealMoveDirection == Vector2.Zero
                    ? Vector2.Zero
                    : Vector2.Normalize(RealMoveDirection));
                return false;
            }
            return destroy;
        }

        public void Destroy()
        {
            if (IsInDestroy) return;
            _isInDestroy = true;

            if (BelongMagic.MoveKind == 15)
            {
                Texture = null;
                _superModeDestroySprites = new LinkedList<Sprite>();
                List<Character> targets;
                if (BelongMagic.AttackAll > 0)
                {
                    targets = NpcManager.NpcsInView.Where(npc => npc.IsFighter).Cast<Character>().ToList();
                    targets.Add(Globals.ThePlayer);
                }
                else if (BelongCharacter.IsPlayer || BelongCharacter.IsFighterFriend)
                {
                    targets = NpcManager.NpcsInView.Where(npc => npc.IsEnemy).Cast<Character>().ToList();
                }
                else if(BelongCharacter.IsEnemy)
                {
                    targets = NpcManager.NpcsInView.Where(npc => npc.IsFighterFriend).Cast<Character>().ToList();
                    targets.Add(Globals.ThePlayer);
                }
                else //None
                {
                    targets = NpcManager.NpcsInView.Where(npc => npc.IsFighter && npc.Relation != (int)Character.RelationType.None).Cast<Character>().ToList();
                    targets.Add(Globals.ThePlayer);
                }
                foreach (var character in targets)
                {
                    AddDestroySprite(_superModeDestroySprites,
                        character.PositionInWorld,
                        BelongMagic.VanishImage,
                        BelongMagic.VanishSound);
                    character.NotifyFighterAndAllNeighbor(BelongCharacter);
                    CharacterHited(character);
                }
                if (_superModeDestroySprites.Count == 0) _isDestroyed = true;
            }
            else if (BelongMagic.MoveKind == 23)
            {
                //Time stop
                if (Globals.TheGame.TimeStoperMagicSprite == this)
                {
                    Globals.TheGame.TimeStoperMagicSprite = null;
                }
            }
            else
            {
                switch (BelongMagic.MoveKind)
                {
                    case 20:
                        {
                            BelongCharacter.IsInTransport = false;
                            var tilePosition = MapBase.ToTilePosition(_destnationPixelPosition);
                            var finded = PathFinder.FindNonobstacleNeighborOrItself(BelongCharacter, ref tilePosition);
                            if (finded)
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
                    case 22:
                        {
                            PositionInWorld = _summonedNpc.PositionInWorld;
                            if (_summonedNpc != null)
                            {
                                _summonedNpc.Death();
                            }
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
                    if (_parasitiferCharacter == null)
                    {
                        _isDestroyed = true;
                    }
                }

                if (_parasitiferCharacter != null)
                {
                    PlayFrames(int.MaxValue);
                }

                SoundManager.Play3DSoundOnece(BelongMagic.VanishSound,
                    PositionInWorld - Globals.ListenerPosition);

                UseMagic(BelongMagic.ExplodeMagicFile);

                if (BelongMagic.VibratingScreen > 0)
                {
                    Globals.TheCarmera.VibaratingScreen(BelongMagic.VibratingScreen);
                }
            }
        }

        public void SetDestroyed()
        {
            _isDestroyed = true;
        }

        private void UseMagic(Magic magic)
        {
            if(magic == null) return;

            MagicManager.UseMagic(BelongCharacter,
                    magic,
                    PositionInWorld,
                    RealMoveDirection == Vector2.Zero
                        ? PositionInWorld + (PositionInWorld - BelongCharacter.PositionInWorld)
                        : PositionInWorld + RealMoveDirection);
        }

        private void LeapToNextTarget(Character hitedCharacter)
        {
            if (_leftLeapTimes > 0)
            {
                _leftLeapTimes--;
                _currentEffect -= _currentEffect * BelongMagic.EffectReducePercentage / 100;
                _currentEffect2 -= _currentEffect2 * BelongMagic.EffectReducePercentage / 100;
                _currentEffect3 -= _currentEffect3 * BelongMagic.EffectReducePercentage / 100;
                _currentEffectMana -= _currentEffectMana * BelongMagic.EffectReducePercentage / 100;
            }
            else
            {
                EndLeap();
            }

            AddDestroySprite(MagicManager.EffectSprites, PositionInWorld, BelongMagic.VanishImage, BelongMagic.VanishSound);

            Character nextTarget;
            if (BelongMagic.AttackAll > 0)
            {
                nextTarget = NpcManager.GetClosestFighter(hitedCharacter.PositionInWorld, _leapedCharacters);
            }
            else
            {
                nextTarget = NpcManager.GetClosestEnemy(BelongCharacter, hitedCharacter.PositionInWorld, true, false, _leapedCharacters);
            }
            if (nextTarget == null)
            {
                EndLeap();
                return;
            }
            Texture = BelongMagic.LeapImage;
            PlayFrames(BelongMagic.LeapFrame);
            MoveDirection = nextTarget.PositionInWorld - PositionInWorld;
            //Move magic sprite to neighber tile
            TilePosition = PathFinder.FindNeighborInDirection(TilePosition, RealMoveDirection);
            //Correct move direction
            MoveDirection = nextTarget.PositionInWorld - PositionInWorld;

            _leapedCharacters.Add(hitedCharacter);
        }

        private void EndLeap()
        {
            _leftLeapTimes = 0;
            _isDestroyed = true;
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

        private void SetRoundMove()
        {
            if (BelongMagic.RoundRadius <= 0)
            {
                return;
            }
            var x = BelongCharacter.PositionInWorld.X +
                    Math.Cos(_roundMoveInfo.CurDegree * Math.PI / 180f) * BelongMagic.RoundRadius;
            var y = BelongCharacter.PositionInWorld.Y +
                    Math.Sin(_roundMoveInfo.CurDegree * Math.PI / 180f) * BelongMagic.RoundRadius;
            PositionInWorld = new Vector2((float)x, (float)y);
            var dir = PositionInWorld - BelongCharacter.PositionInWorld;
            dir.Normalize();
            _moveDirection.X = BelongMagic.RoundMoveColockwise > 0 ? -dir.Y : dir.Y;
            _moveDirection.Y = BelongMagic.RoundMoveColockwise > 0 ? dir.X : -dir.X;
            SetDirection(_moveDirection);
        }

        public override void Update(GameTime gameTime)
        {
            if (IsDestroyed) return;

            _elaspedMillisecond += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (_waitMilliSeconds > 0)
            {
                _waitMilliSeconds -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                return;
            }

            if (BelongMagic.RoundMoveColockwise > 0 || BelongMagic.RoundMoveAnticlockwise > 0)
            {
                _roundMoveInfo.CurDegree += (BelongMagic.RoundMoveColockwise > 0 ? 1 : -1) *
                                            BelongMagic.RoundMoveDegreeSpeed *
                                            (float)gameTime.ElapsedGameTime.TotalSeconds;
                SetRoundMove();
            }

            if (!string.IsNullOrEmpty(BelongMagic.MagicWhenNewPos))
            {
                if (_lastTilePosition != TilePosition)
                {
                    MagicManager.AddFixedPositionMagicSprite(BelongCharacter, Utils.GetMagic(BelongMagic.MagicWhenNewPos, BelongCharacter.IsMagicFromCache), MapBase.ToPixelPosition(_lastTilePosition), true);
                    _lastTilePosition = TilePosition;
                }
            }

            if (_parasitiferCharacter != null)
            {
                PositionInWorld = _parasitiferCharacter.PositionInWorld;

                if (_parasitiferCharacter.IsDeathInvoked)
                {
                    _parasitiferCharacter = null;
                    _isDestroyed = true;
                }
                else
                {
                    _parasticTime += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (_parasticTime >= BelongMagic.ParasiticInterval)
                    {
                        _parasticTime -= BelongMagic.ParasiticInterval;
                        UseMagic(BelongMagic.ParasiticMagic);
                        CharacterHited(_parasitiferCharacter, 
                            MagicManager.GetEffectAmount(BelongMagic, BelongCharacter), 
                            MagicManager.GetEffectAmount2(BelongMagic, BelongCharacter),
                            MagicManager.GetEffectAmount3(BelongMagic, BelongCharacter),
                            BelongMagic.EffectMana, 
                            false);

                        if (BelongMagic.ParasiticMaxEffect > 0 && _totalParasticEffect >= BelongMagic.ParasiticMaxEffect)
                        {
                            _parasitiferCharacter = null;
                            _isDestroyed = true;
                        }
                    }
                }
            }

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
                            if (_destroyOnEnd || BelongMagic.MeteorMove > 0) Destroy();
                            CollisionDetaction();
                        }
                    }
                }
            }
            else
            {
                if (!IsInDestroy)
                {
                    if (BelongMagic.FollowMouse > 0)
                    {
                        var mouseState = Mouse.GetState();
                        var mouseScreenPosition = new Vector2(mouseState.X, mouseState.Y);
                        var mouseWorldPosition = Globals.TheCarmera.ToWorldPosition(mouseScreenPosition);
                        var direction = mouseWorldPosition - PositionInWorld;
                        MoveDirection = direction.Length() > 25 ? direction : Vector2.Zero;
                    }
                    else if (BelongMagic.RandomMoveDegree > 0)
                    {
                        while (MoveDirection == Vector2.Zero)
                        {
                            MoveDirection = new Vector2((float)Globals.TheRandom.Next(-100, 100) / 100.0f, (float)Globals.TheRandom.Next(-100, 100) / 100.0f);
                        }
                        var perpendicular1 = new Vector2(MoveDirection.Y, -MoveDirection.X);
                        var perpendicular2 = new Vector2(-MoveDirection.Y, MoveDirection.X);
                        var random = (Globals.TheRandom.Next(2) == 0 ? perpendicular1 : perpendicular2) *
                                        Globals.TheRandom.Next(BelongMagic.RandomMoveDegree);
                        MoveDirection += random;
                    }

                    if (BelongMagic.MoveImitateUser > 0)
                    {
                        PositionInWorld += (BelongCharacter.PositionInWorld - _lastUserWorldPosition);
                        _lastUserWorldPosition = BelongCharacter.PositionInWorld;
                    }

                    if (BelongMagic.CircleMoveColockwise > 0 || BelongMagic.CircleMoveAnticlockwise > 0)
                    {
                        var dir = PositionInWorld - BelongCharacter.PositionInWorld;
                        if (dir != Vector2.Zero)
                        {
                            dir.Normalize();
                            dir = BelongMagic.CircleMoveColockwise > 0 ? new Vector2(-dir.Y, dir.X) : new Vector2(dir.Y, -dir.X);
                            _circleMoveDir = dir;
                        }
                    }

                    if (_isInMoveBack)
                    {
                        //Move back to magic user.
                        var dir = BelongCharacter.PositionInWorld - PositionInWorld;
                        MoveDirection = dir;
                        if (dir.Length() < 20)
                        {
                            _isInMoveBack = false;
                            _isDestroyed = true;
                        }
                    }
                }

                if (BelongMagic.MoveKind == 16 || BelongMagic.TraceEnemy > 0)
                {
                    if (MovedDistance > 200f || (BelongMagic.TraceEnemy > 0 && _elaspedMillisecond >= BelongMagic.TraceEnemyDelayMilliseconds)) //First move 200, than find target
                    {
                        if (BelongMagic.AttackAll > 0)
                        {
                            _closedCharecter = NpcManager.GetClosestFighter(PositionInWorld);
                        }
                        else
                        {
                            if (BelongCharacter.IsPlayer || BelongCharacter.IsFriend)
                            {
                                if (_closedCharecter == null || _closedCharecter.IsDeath)
                                {
                                    _closedCharecter = NpcManager.GetClosestEnemyTypeCharacter(PositionInWorld, true, false);
                                }
                            }
                            else if(BelongCharacter.IsEnemy)
                            {
                                _closedCharecter = NpcManager.GetLiveClosestPlayerOrFighterFriend(PositionInWorld, true, false);
                            }
                            else if (BelongCharacter.IsNoneFighter)
                            {
                                _closedCharecter = NpcManager.GetLiveClosestNonneturalFighter(PositionInWorld);
                            }
                        }


                        if (_closedCharecter != null)
                        {
                            if (BelongMagic.TraceSpeed > 0)
                            {
                                Velocity = Globals.MagicBasespeed * BelongMagic.TraceSpeed;
                            }
                            
                            MoveDirection = _closedCharecter.PositionInWorld - PositionInWorld;
                        }
                            
                    }
                    MoveToNoNormalizeDirection(RealMoveDirection,
                        (float)gameTime.ElapsedGameTime.TotalSeconds,
                        MagicManager.GetSpeedRatio(RealMoveDirection));
                }
                else if (_isInDestroy)
                {
                    //Stop moving when in destroy.
                }
                else MoveToNoNormalizeDirection(RealMoveDirection, (float)gameTime.ElapsedGameTime.TotalSeconds);
            }

            if (BelongMagic.MoveKind == 13 ||
                BelongMagic.MoveKind == 23)
            {
                PositionInWorld = BelongCharacter.PositionInWorld;
            }

            if (!_isInDestroy)
            {
                if (BelongMagic.RangeEffect > 0 && (_paths == null || _paths.Count <= 2))
                {
                    _rangeElapsedMilliseconds += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (_rangeElapsedMilliseconds >= BelongMagic.RangeTimeInerval)
                    {
                        _rangeElapsedMilliseconds -= BelongMagic.RangeTimeInerval;

                        if (BelongMagic.RangeAddLife > 0 ||
                            BelongMagic.RangeAddMana > 0 ||
                            BelongMagic.RangeAddThew > 0 ||
                            BelongMagic.RangeSpeedUp > 0)
                        {
                            foreach (var target in NpcManager.FindFriendInTileDistance(BelongCharacter, TilePosition, BelongMagic.RangeRadius))
                            {
                                if (BelongMagic.RangeAddLife > 0)
                                {
                                    target.AddLife(BelongMagic.RangeAddLife);
                                }
                                if (BelongMagic.RangeAddMana > 0)
                                {
                                    target.AddMana(BelongMagic.RangeAddMana);
                                }
                                if (BelongMagic.RangeAddThew > 0)
                                {
                                    target.AddThew(BelongMagic.RangeAddThew);
                                }
                                if (BelongMagic.RangeSpeedUp > 0 && target.SppedUpByMagicSprite == null)
                                {
                                    target.SppedUpByMagicSprite = this;
                                }
                            }
                        }

                        if (BelongMagic.RangeFreeze > 0 ||
                            BelongMagic.RangePoison > 0 ||
                            BelongMagic.RangePetrify > 0 ||
                            BelongMagic.RangeDamage > 0)
                        {
                            List<Character> targets;
                            if (BelongMagic.AttackAll > 0)
                            {
                                targets = NpcManager.FindFightersInTileDistance(TilePosition, BelongMagic.RangeRadius);
                            }
                            else
                            {
                                targets = NpcManager.FindEnemiesInTileDistance(BelongCharacter, TilePosition,
                                    BelongMagic.RangeRadius);
                            }
                            foreach (var target in targets)
                            {
                                if (BelongMagic.RangeFreeze > 0)
                                {
                                    target.SetFrozenSeconds(BelongMagic.RangeFreeze/1000.0f, BelongMagic.NoSpecialKindEffect == 0);
                                }
                                if (BelongMagic.RangePoison > 0)
                                {
                                    target.SetPoisonSeconds(BelongMagic.RangePoison/1000.0f, BelongMagic.NoSpecialKindEffect == 0);
                                    if (BelongCharacter is Player || BelongCharacter.IsPartner)
                                    {
                                        target.PoisonByCharacterName = BelongCharacter.Name;
                                    }
                                }
                                if (BelongMagic.RangePetrify > 0)
                                {
                                    target.SetPetrifySeconds(BelongMagic.RangePetrify/1000.0f, BelongMagic.NoSpecialKindEffect == 0);
                                }
                                if (BelongMagic.RangeDamage > 0)
                                {
                                    CharacterHited(target, BelongMagic.RangeDamage, BelongMagic.Effect2, BelongMagic.Effect3, 0);
                                    AddDestroySprite(MagicManager.EffectSprites, target.PositionInWorld, BelongMagic.VanishImage, BelongMagic.VanishSound);
                                }
                            }
                        }
                        
                    }
                }

                if (BelongMagic.FlyMagic != null)
                {
                    _flyMagicElapsedMilliSeconds += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (_flyMagicElapsedMilliSeconds >= BelongMagic.FlyInterval)
                    {
                        _flyMagicElapsedMilliSeconds -= BelongMagic.FlyInterval;
                        var dir = RealMoveDirection == Vector2.Zero
                            ? PositionInWorld - BelongCharacter.PositionInWorld
                            : RealMoveDirection;
                        MagicManager.UseMagic(BelongCharacter, BelongMagic.FlyMagic, PositionInWorld,
                            PositionInWorld + dir);
                    }
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
            else if (_paths != null)
            {
                //do nothing
            }
            else if (BelongMagic.MoveKind == 15)
            {
                if (!IsInPlaying)
                    Destroy();
            }
            else if (BelongMagic.MoveKind == 22) //Summon
            {
                if (_summonElapsedMilliseconds < BelongMagic.KeepMilliseconds)
                {
                    _summonElapsedMilliseconds += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (!IsInPlaying) Texture = Asf.Empty;
                }
                else
                {
                    Destroy();
                }
            }
            else
            {
                var checkHit = true;
                switch (BelongMagic.MoveKind)
                {
                    case 13:
                    case 20: //transport
                    case 21: //Controling others
                    case 22:
                    case 23: //Time stoper
                        checkHit = false;
                        break;
                    default:
                        CollisionDetaction();
                        break;
                }

                if (checkHit && CheckDestroyForObstacleInMap())
                {
                    Destroy();
                }
                else if (!IsInPlaying)
                {
                    if (BelongMagic.MoveBack > 0)
                    {
                        if (Velocity == 0.0f)
                        {
                            Velocity = Globals.MagicBasespeed * BelongMagic.Speed;
                        }
                        _isInMoveBack = true;
                    }
                    else if (_destroyOnEnd)
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
            if (IsDestroyed || _waitMilliSeconds > 0) return;
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
