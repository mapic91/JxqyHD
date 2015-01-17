using System;
using System.Collections.Generic;
using System.Globalization;
using Engine.Script;
using IniParser;
using IniParser.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace Engine
{
    using StateMapList = Dictionary<int, ResStateInfo>;
    public abstract class Character : Sprite
    {
        #region Field
        private SoundEffectInstance _sound;
        private int _dir;
        private string _name;
        private int _kind;
        private int _relation;
        private int _pathFinder;
        private int _state;
        private int _visionRadius;
        private int _dialogRadius;
        private int _attackRadius;
        private int _lum;
        private int _action;
        private int _walkSpeed = 1;
        private int _evade;
        private int _attack;
        private int _attackLevel;
        private int _defend;
        private int _exp;
        private int _levelUpExp;
        private int _level;
        private int _life;
        private int _lifeMax;
        private int _thew;
        private int _thewMax;
        private int _mana;
        private int _manaMax;
        private StateMapList _npcIni = new StateMapList();
        private string _npcIniFileName;
        private Obj _bodyIni;
        private Magic _flyIni;
        private Magic _flyIni2;
        private string _scriptFile;
        private string _deathScript;
        private ScriptParser _currentRunInteractScript;
        private ScriptParser _currentRunDeathScript;
        private int _expBonus;
        private string _fixedPos;
        private int _idle;
        private Vector2 _magicDestination;
        private Vector2 _attackDestination;
        private bool _isInFighting;
        private float _totalNonFightingSeconds;
        private const float MaxNonFightSeconds = 7f;
        private Vector2 _destinationMovePositionInWorld = Vector2.Zero;
        private Vector2 _destinationMoveTilePosition = Vector2.Zero;
        private Vector2 _destinationAttackTilePosition = Vector2.Zero;
        private Vector2 _destinationAttackPositionInWorld = Vector2.Zero;
        private LinkedList<Vector2> _path;
        private object _interactiveTarget;
        private bool _isInInteract;
        private bool _isRunToTarget;
        private bool _isDeath;
        private float _poisonedMilliSeconds;
        private float _frozenSeconds;
        private float _poisonSeconds;
        private float _petrifiedSeconds;
        private bool _notAddBody;
        private bool _isNextStepStand;
        private Dictionary<int, Utils.LevelDetail> _levelIni;
        private bool _isInStepMove;
        private int _stepMoveDirection;
        private int _leftStepToMove;
        private int _directionBeforInteract;
        private int _specialActionLastDirection; //Direction before play special action
        private float _fixedPathDistanceToMove;
        private Vector2 _fixedPathMoveDestinationPixelPostion = Vector2.Zero;

        /// <summary>
        /// List of the fixed path tile position.
        /// When load <see cref="FixedPos"/>, <see cref="FixedPos"/> is converted to list and stored on this value.
        /// </summary>
        protected List<Vector2> FixedPathTilePositionList;
        protected int _currentFixedPosIndex;
        protected Magic MagicUse;

        #endregion Field

        #region Protected properties
        protected virtual bool MagicFromCache { get { return true; } }

        protected string LevelIniFile { set; get; }
        #endregion Protected properties

        #region Public properties

        public LinkedList<MagicSprite> MagicSpritesInEffect = new LinkedList<MagicSprite>();

        public Dictionary<int, Utils.LevelDetail> LevelIni
        {
            get { return _levelIni; }
            set { _levelIni = value; }
        }

        public bool IsFightDisabled { protected set; get; }
        public bool IsJumpDisabled { protected set; get; }
        public bool IsRunDisabled { protected set; get; }
        public Character FollowTarget { protected set; get; }
        public bool IsFollowTargetFound { protected set; get; }
        public bool IsInSpecialAction { protected set; get; }

        public bool IsDeathScriptEnd
        {
            get
            {
                if (_currentRunDeathScript == null)
                {
                    return true;
                }
                return _currentRunDeathScript.IsEnd;
            }
        }

        public bool IsNodAddBody
        {
            get { return _notAddBody; }
        }

        public bool IsHide { get; set; }

        public bool IsInStepMove
        {
            get { return _isInStepMove; }
        }

        public float FrozenSeconds
        {
            get { return _frozenSeconds; }
            set
            {
                if (IsInDeathing) return;
                if (value < 0) value = 0;
                _frozenSeconds = value;
                _poisonSeconds = 0;
                _petrifiedSeconds = 0;
            }
        }

        public float PoisonSeconds
        {
            get { return _poisonSeconds; }
            set
            {
                if (IsInDeathing) return;
                if (value < 0) value = 0;
                _frozenSeconds = 0;
                _poisonSeconds = value;
                _petrifiedSeconds = 0;
            }
        }

        public float PetrifiedSeconds
        {
            get { return _petrifiedSeconds; }
            set
            {
                if (IsInDeathing) return;
                if (value < 0) value = 0;
                _frozenSeconds = 0;
                _poisonSeconds = 0;
                _petrifiedSeconds = value;
            }
        }

        public bool IsFrozened { get { return FrozenSeconds > 0; } }
        public bool IsPoisoned { get { return PoisonSeconds > 0; } }
        public bool IsPetrified { get { return PetrifiedSeconds > 0; } }

        public bool BodyFunctionWell
        {
            get
            {
                return (FrozenSeconds <= 0 &&
                        PoisonSeconds <= 0 &&
                        PetrifiedSeconds <= 0);
            }
        }

        public int Dir
        {
            get { return _dir; }
            set { _dir = value % 8; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public int Kind
        {
            get { return _kind; }
            set { _kind = value; }
        }

        public int Relation
        {
            get { return _relation; }
            set { _relation = value; }
        }

        public int State
        {
            get { return _state; }
            set { _state = value; }
        }

        public StateMapList NpcIni
        {
            get { return _npcIni; }
            protected set { _npcIni = value; }
        }

        public virtual int PathFinder
        {
            get { return _pathFinder; }
            set { _pathFinder = value; }
        }

        public abstract Engine.PathFinder.PathType PathType { get; }

        public int VisionRadius
        {
            get { return _visionRadius == 0 ? 1 : _visionRadius; }
            set { _visionRadius = value; }
        }

        public int DialogRadius
        {
            get { return _dialogRadius == 0 ? 1 : _dialogRadius; }
            set { _dialogRadius = value; }
        }

        public int AttackRadius
        {
            get { return _attackRadius == 0 ? 1 : _attackRadius; }
            set { _attackRadius = value; }
        }

        public int Lum
        {
            get { return _lum; }
            set { _lum = value; }
        }

        public int Action
        {
            get { return _action; }
            set { _action = value; }
        }

        public int WalkSpeed
        {
            get { return _walkSpeed; }
            set { _walkSpeed = value < 1 ? 1 : value; }
        }

        public int Evade
        {
            get { return _evade; }
            set { _evade = value; }
        }

        public int Attack
        {
            get { return _attack; }
            set { _attack = value; }
        }

        public int AttackLevel
        {
            get { return _attackLevel; }
            set
            {
                _attackLevel = value;
                if (FlyIni != null)
                {
                    FlyIni = FlyIni.GetLevel(value);
                }
                if (FlyIni2 != null)
                {
                    FlyIni2 = FlyIni2.GetLevel(value);
                }
            }
        }

        public int Defend
        {
            get { return _defend; }
            set { _defend = value; }
        }

        public int Exp
        {
            get { return _exp; }
            set { _exp = value; }
        }

        public int LevelUpExp
        {
            get { return _levelUpExp; }
            set { _levelUpExp = value; }
        }

        public int Life
        {
            get { return _life; }
            set
            {
                _life = value < 0 ? 0 : value;
                if (_life > _lifeMax) _life = _lifeMax;
            }
        }

        public int Level
        {
            get { return _level; }
            set { _level = value; }
        }

        public int LifeMax
        {
            get { return _lifeMax; }
            set
            {
                _lifeMax = value;
                if (_life > value) _life = value;
            }
        }

        public int Thew
        {
            get { return _thew; }
            set
            {
                _thew = value < 0 ? 0 : value;
                if (_thew > _thewMax) _thew = _thewMax;
            }
        }

        public int ThewMax
        {
            get { return _thewMax; }
            set
            {
                _thewMax = value;
                if (_thew > value) _thew = value;
            }
        }

        public int Mana
        {
            get { return _mana; }
            set
            {
                _mana = value < 0 ? 0 : value;
                if (_mana > _manaMax) _mana = _manaMax;
            }
        }

        public int ManaMax
        {
            get { return _manaMax; }
            set
            {
                _manaMax = value;
                if (_mana > value) _mana = value;
            }
        }

        public Obj BodyIni
        {
            get { return _bodyIni; }
            set { _bodyIni = value; }
        }

        public Magic FlyIni
        {
            get { return _flyIni; }
            set { _flyIni = value; }
        }

        public Magic FlyIni2
        {
            get { return _flyIni2; }
            set { _flyIni2 = value; }
        }

        public string ScriptFile
        {
            get { return _scriptFile; }
            set { _scriptFile = value; }
        }

        public bool HasInteractScript
        {
            get { return !string.IsNullOrEmpty(ScriptFile); }
        }

        public string DeathScript
        {
            get { return _deathScript; }
            set { _deathScript = value; }
        }

        public int ExpBonus
        {
            get { return _expBonus; }
            set { _expBonus = value; }
        }

        public string FixedPos
        {
            get { return _fixedPos; }
            set
            {
                _fixedPos = value;
                FixedPathTilePositionList = ToFixedPosTilePositionList(value);
            }
        }

        public int CurrentFixedPosIndex
        {
            get { return _currentFixedPosIndex; }
            set { _currentFixedPosIndex = value; }
        }

        public int Idle
        {
            get { return _idle; }
            set { _idle = value; }
        }

        public bool IsObstacle
        {
            get { return (Kind != 7); }
        }

        public bool IsPlayer
        {
            get { return (Kind == 2 || Kind == 3); }
        }

        public Vector2 DestinationMovePositionInWorld
        {
            get { return _destinationMovePositionInWorld; }
            set
            {
                _destinationMovePositionInWorld = value;
                _destinationMoveTilePosition = Map.ToTilePosition(value);
            }
        }

        public Vector2 DestinationMoveTilePosition
        {
            get
            { return _destinationMoveTilePosition; }
            set
            {
                _destinationMoveTilePosition = value;
                _destinationMovePositionInWorld = Map.ToPixelPosition(value);
            }
        }

        public Vector2 DestinationAttackTilePosition
        {
            get { return _destinationAttackTilePosition; }
            set
            {
                _destinationAttackTilePosition = value;
                _destinationAttackPositionInWorld = Map.ToPixelPosition(value);
            }
        }

        public Vector2 DestinationAttackPositionInWorld
        {
            get { return _destinationAttackPositionInWorld; }
            set
            {
                _destinationAttackPositionInWorld = value;
                _destinationAttackTilePosition = Map.ToTilePosition(value);
            }
        }

        public LinkedList<Vector2> Path
        {
            get { return _path; }
            protected set
            {
                _path = value;
                MovedDistance = 0;
            }
        }

        public bool IsDeath
        {
            get { return _isDeath; }
            protected set { _isDeath = value; }
        }

        /// <summary>
        /// Death() method invoked
        /// </summary>
        public bool IsDeathInvoked { protected set; get; }

        public bool IsInDeathing { get { return State == (int)CharacterState.Death; } }

        #endregion Public properties

        #region Character Type and Relation
        public bool IsEnemy
        {
            get { return Kind == 1 && Relation == 1; }
        }

        public bool IsFriend
        {
            get { return Relation == (int)RelationType.Friend; }
        }

        public bool IsFighterFriend
        {
            get { return ((Kind == 1 || Kind == 3) && Relation == 0); }
        }

        public bool IsFighter
        {
            get { return Kind == (int)CharacterKind.Fighter; }
        }

        public bool IsPartner
        {
            get { return Kind == 3; }
        }

        public bool IsInteractive
        {
            get { return (HasInteractScript || IsEnemy || IsFighterFriend); }
        }

        #endregion Character Type and Relation

        #region Ctor
        public Character() { }

        public Character(string filePath)
        {
            Load(filePath);
        }

        public Character(KeyDataCollection keyDataCollection)
        {
            Load(keyDataCollection);
        }
        #endregion Ctor

        #region Private method
        private void Initlize()
        {
            if (NpcIni.ContainsKey((int)CharacterState.Stand))
            {
                Set(Map.ToPixelPosition(MapX, MapY),
                    Globals.BaseSpeed,
                    NpcIni[(int)CharacterState.Stand].Image, Dir);
            }
            if (FlyIni != null)
            {
                FlyIni = FlyIni.GetLevel(AttackLevel);
            }
            if (FlyIni2 != null)
            {
                FlyIni2 = FlyIni2.GetLevel(AttackLevel);
            }
        }        

        /// <summary>
        /// Convert FixedPos string to path list
        /// FixedPos string pattern xx000000yy000000xx000000yy000000
        /// xx,yy is Hex string.
        /// xx - tile postion x, yy - tile postion y
        /// exemple: "0B0000001C0000000E000000170000000000000000000000"
        /// </summary>
        /// <param name="fixPos">FixedPos string</param>
        /// <returns>Converted path</returns>
        private List<Vector2> ToFixedPosTilePositionList(string fixPos)
        {
            var steps = Utils.SpliteStringInCharCount(fixPos, 8);
            var count = steps.Count;
            if (count < 4) return null; //Step less than 2
            if (count % 2 != 0) count--;//Not even, decrease one
            try
            {
                var path = new List<Vector2>();
                for (var i = 0; i < count; i += 2)
                {
                    var x = int.Parse(steps[i].Substring(0, 2), NumberStyles.HexNumber);
                    var y = int.Parse(steps[i + 1].Substring(0, 2), NumberStyles.HexNumber);
                    if (x == 0 && y == 0) break;
                    path.Add(new Vector2(x, y));
                }
                return path;
            }
            catch (Exception)
            {
                //FixedPos format error
                return null;
            }
        }

        private void EndInteract()
        {
            _isInInteract = false;
            if (IsStanding())
            {
                //Character may moved when in interacting.
                //Only set back direction when standing.
                SetDirection(_directionBeforInteract);
            }
        }

        private void CheckStepMove()
        {
            if (!_isInStepMove) return;
            if (_leftStepToMove == 0)
            {
                StandingImmediately();
                _isInStepMove = false;
                return;
            }
            var destinationTilePosition = Engine.PathFinder.FindNeighborInDirection(
                TilePosition,
                _stepMoveDirection);
            WalkTo(destinationTilePosition);
            if (Path == null ||
                HasObstacle(destinationTilePosition))//Unable to move
            {
                _isInStepMove = false;
                return;
            }
            _leftStepToMove--;
        }

        private void MoveAlongPath(float elapsedSeconds, int speedFold)
        {
            if (Path == null || Path.Count < 2)
            {
                StandingImmediately();
                return;
            }

            var from = Path.First.Value;
            var to = Path.First.Next.Value;
            var tileFrom = Map.ToTilePosition(from);
            var tileTo = Map.ToTilePosition(to);
            var direction = to - from;
            if (direction != Vector2.Zero)
            {
                direction.Normalize();
            }
            var distance = Vector2.Distance(from, to);
            //Sotre value in case of cleared in below code
            var interactTarget = _interactiveTarget;

            if (TilePosition == tileFrom)
            {
                if (HasObstacle(tileTo)) //Obstacle in the way
                {
                    //PositionInWorld = from;
                    var path = Engine.PathFinder.FindPath(this, TilePosition, DestinationMoveTilePosition, PathType);
                    if (tileTo == DestinationMoveTilePosition || //Just one step, standing
                        path == null //Can't find path
                        )
                    {
                        Path = null;
                        StandingImmediately();
                        return;
                    }
                    if (PositionInWorld != path.First.Value)
                        path.AddFirst(PositionInWorld);//Move back
                    Path = path;
                }
            }
            MoveTo(direction, elapsedSeconds * speedFold);
            if (TilePosition != tileFrom &&
                TilePosition != tileTo) // neither in from nor in to, correcting path
            {
                var correctDistance = distance / 2 + 1f; // half plus one
                PositionInWorld = from + direction * correctDistance;
                MovedDistance = correctDistance;
                var pos = TilePosition;
            }

            if (MovedDistance >= distance)
            {
                if (_isNextStepStand)
                {
                    _isNextStepStand = false;
                    PositionInWorld = to;
                    StandingImmediately();
                    MovedDistance = 0;
                }
                else if (DestinationMovePositionInWorld != Path.Last.Value) // new destination
                {
                    var destination = DestinationMovePositionInWorld;
                    PositionInWorld = to;
                    Path = Engine.PathFinder.FindPath(this, TilePosition, Map.ToTilePosition(destination), PathType);
                    if (Path == null) StandingImmediately();
                }
                else
                {
                    if (Path.Count > 2)
                    {
                        Path.RemoveFirst();
                        //Apply offset to next move
                        var offset = MovedDistance - distance;
                        var newFrom = Path.First.Value;
                        var newTo = Path.First.Next.Value;
                        var newDirection = newTo - newFrom;
                        newDirection.Normalize();
                        PositionInWorld = newFrom + newDirection * offset;
                        MovedDistance = offset;
                    }
                    else
                    {
                        PositionInWorld = to;
                        //If in step move don't stand
                        if (!IsInStepMove)
                        {
                            StandingImmediately();
                        }
                        MovedDistance = 0;
                    }
                }
                CheckMapTrap();
                CheckStepMove();
                if (AttackingIsOk()) PerformeAttack();
                _interactiveTarget = interactTarget;
                if (InteractIsOk()) PerformeInteract();
                if (IsRuning())
                {
                    if (!CanRunning())
                    {
                        WalkTo(DestinationMoveTilePosition);
                    }
                }
            }
        }

        private void JumpAlongPath(float elapsedSeconds)
        {
            if (Path == null)
            {
                StandingImmediately();
                return;
            }
            if (Path.Count == 2)
            {
                var from = Path.First.Value;
                var to = Path.First.Next.Value;
                var distance = Vector2.Distance(from, to);
                bool isOver = false;
                var nextTile = Engine.PathFinder.FindNeighborInDirection(
                    TilePosition, to - from);
                if (Globals.TheMap.IsObstacleForCharacterJump(nextTile) ||
                    (nextTile == to && HasObstacle(nextTile)))
                {
                    TilePosition = TilePosition;//Correcting position
                    isOver = true;
                }
                else MoveTo(to - from, elapsedSeconds * 8);
                if (MovedDistance >= distance - Globals.DistanceOffset && !isOver)
                {
                    MovedDistance = 0;
                    PositionInWorld = to;
                    isOver = true;
                }
                if (isOver)
                {
                    Path.RemoveFirst();
                }
            }
            if (IsPlayCurrentDirOnceEnd())
            {
                StandingImmediately();
                CheckMapTrap();
            }
        }

        protected void RandWalk(List<Vector2> tilePositionList, int randMaxValue, bool isFlyer)
        {
            if (tilePositionList == null ||
                tilePositionList.Count < 2 ||
                !IsStanding() ||
                _isInInteract) return;
            if (Globals.TheRandom.Next(0, randMaxValue) == 0)
            {
                var tilePosition = tilePositionList[Globals.TheRandom.Next(0, tilePositionList.Count)];
                MoveTo(tilePosition, isFlyer);
            }
        }

        protected void LoopWalk(List<Vector2> tilePositionList, int randMaxValue, ref int currentPathIndex, bool isFlyer)
        {
            if (tilePositionList == null ||
                tilePositionList.Count < 2 ||
                _isInInteract) return;

            if (IsStanding() &&
                Globals.TheRandom.Next(0, randMaxValue) == 0)
            {
                currentPathIndex++;
                if (currentPathIndex > tilePositionList.Count - 1)
                {
                    currentPathIndex = 0;
                }
                MoveTo(tilePositionList[currentPathIndex], isFlyer);
            }
        }

        protected void KeepMinTileDistance(Vector2 targetTilePosition, int minTileDistance)
        {
            if (_isInInteract) // In interacting can't moving
            {
                return;
            }

            var tileDistance = Engine.PathFinder.GetTileDistance(TilePosition, targetTilePosition);

            if (tileDistance < minTileDistance)
            {
                MoveAwayTarget(Map.ToPixelPosition(targetTilePosition), false);
            }
        }

        protected void MoveTo(Vector2 tilePosition, bool isFlyer)
        {
            if (isFlyer)
            {
                //Flyer can move in straight line use fixed path move style.
                FixedPathMoveToDestination(tilePosition);
            }
            else
            {
                //Find path and walk to destionation.
                WalkTo(tilePosition);
            }
        }

        /// <summary>
        /// Get rand path, path first step is current character tile position.
        /// </summary>
        /// <param name="count">Path step count.</param>
        /// <param name="checkObstacle">If true, tile position is obstacle for character is no added to path.</param>
        /// <returns>The rand path.</returns>
        protected List<Vector2> GetRandTilePath(int count, bool checkObstacle)
        {
            var path = new List<Vector2>() { TilePosition };

            int maxTry = count * 3;//For performace, otherwise method may run forever.
            const int maxOffset = 15;

            for (var i = 1; i < count; i++)
            {
                Vector2 tilePosition;
                do
                {
                    if (--maxTry < 0) return path;

                    tilePosition = Globals.TheMap.GetRandPositon(TilePosition, maxOffset);
                } while (tilePosition == Vector2.Zero ||
                    (checkObstacle && Globals.TheMap.IsObstacleForCharacter(tilePosition)));
                path.Add(tilePosition);
            }

            return path;
        }

        /// <summary>
        /// Move to destination in fixed path move style.
        /// </summary>
        /// <param name="destinationTilePosition">Move destination tile position.</param>
        protected void FixedPathMoveToDestination(Vector2 destinationTilePosition)
        {
            _fixedPathMoveDestinationPixelPostion = Map.ToPixelPosition(destinationTilePosition);
            _fixedPathDistanceToMove = Vector2.Distance(PositionInWorld, _fixedPathMoveDestinationPixelPostion);
            MovedDistance = 0f;
            SetState(CharacterState.Walk);
        }
        #endregion Private method

        #region Protected method

        protected virtual void AssignToValue(KeyData keyData)
        {
            try
            {
                var info = this.GetType().GetProperty(keyData.KeyName);
                switch (keyData.KeyName)
                {
                    case "FixedPos":
                    case "Name":
                    case "ScriptFile":
                    case "DeathScript":
                        info.SetValue(this, keyData.Value, null);
                        break;
                    case "NpcIni":
                        SetNpcIni(keyData.Value);
                        break;
                    case "BodyIni":
                        info.SetValue(this, new Obj(@"ini\obj\" + keyData.Value), null);
                        break;
                    case "Defence":
                        Defend = int.Parse(keyData.Value);
                        break;
                    case "LevelIni":
                        LevelIniFile = keyData.Value;
                        info.SetValue(this, Utils.GetLevelLists(@"ini\level\" + keyData.Value), null);
                        break;
                    case "FlyIni":
                    case "FlyIni2":
                        info.SetValue(this, Utils.GetMagic(keyData.Value, MagicFromCache), null);
                        break;
                    case "Life":
                        _life = int.Parse(keyData.Value);
                        break;
                    case "Thew":
                        _thew = int.Parse(keyData.Value);
                        break;
                    case "Mana":
                        _mana = int.Parse(keyData.Value);
                        break;
                    default:
                        {
                            var integerValue = int.Parse(keyData.Value);
                            info.SetValue(this, integerValue, null);
                            break;
                        }
                }
            }
            catch (Exception)
            {
                //Do nothing
                return;
            }
        }

        /// <summary>
        /// Check wheather follow target is findable.Is no follow target do nothing.
        /// Dirived class can override <see cref="FollowTargetFound"/> <see cref="FollowTargetLost"/> method to change character behaviour.
        /// </summary>
        protected void PerformeFollow()
        {
            if (FollowTarget == null) return;
            var targetTilePosition = FollowTarget.TilePosition;

            var tileDistance = Engine.PathFinder.GetTileDistance(TilePosition, targetTilePosition);

            var canSeeTarget = false;

            if (tileDistance <= VisionRadius) //Target in view range
            {
                canSeeTarget = Engine.PathFinder.CanViewTarget(TilePosition, targetTilePosition, VisionRadius);

                IsFollowTargetFound = (IsFollowTargetFound || //Target already found and within vision radius
                                       canSeeTarget); //Character can see target
            }
            else
            {
                IsFollowTargetFound = false;
            }

            if (IsFollowTargetFound)
            {
                FollowTargetFound(canSeeTarget);
            }
            else
            {
                FollowTargetLost();
            }
        }

        protected virtual void FollowTargetFound(bool attackCanReach)
        {
            WalkTo(FollowTarget.TilePosition);
        }

        protected virtual void FollowTargetLost()
        {
            //Do nothing
        }

        protected virtual void CheckMapTrap() { }

        protected abstract void PlaySoundEffect(SoundEffect soundEffect);

        protected virtual bool CanUseMagic()
        {
            return true;
        }

        protected virtual bool CanRunning()
        {
            return !IsRunDisabled;
        }

        protected virtual bool CanJump()
        {
            return !IsJumpDisabled;
        }

        protected void AddKey(KeyDataCollection keyDataCollection, string key, int value)
        {
            if (value != 0)
            {
                keyDataCollection.AddKey(key, value.ToString());
            }
        }

        protected void AddKey(KeyDataCollection keyDataCollection, string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                keyDataCollection.AddKey(key, value);
            }
        }

        protected void AddKey(KeyDataCollection keyDataCollection, string key, bool value)
        {
            if (value)
            {
                keyDataCollection.AddKey(key, "1");
            }
        }

        /// <summary>
        /// Set state to current state
        /// </summary>
        protected void ResetState()
        {
            SetState((CharacterState)State, true);
        }

        /// <summary>
        /// Override this to do something when attacking(use magic FlyIni FlyIni2).
        /// </summary>
        /// <param name="attackDestinationPixelPosition">Attacking destination</param>
        protected virtual void OnAttacking(Vector2 attackDestinationPixelPosition)
        {
            //Do nothing here
        }
        #endregion Protected method

        #region Public method
        public abstract bool HasObstacle(Vector2 tilePosition);
        #endregion Public method

        #region Save load method
        public bool Load(string filePath)
        {
            try
            {
                var data = new FileIniDataParser().ReadFile(filePath, Globals.SimpleChineseEncoding);
                return Load(Utils.GetFirstSection(data));
            }
            catch (Exception exception)
            {
                Log.LogFileLoadError("Character", filePath, exception);
                return false;
            }
        }

        public bool Load(KeyDataCollection keyDataCollection)
        {
            foreach (var keyData in keyDataCollection)
            {
                AssignToValue(keyData);
            }
            Initlize();
            return true;
        }

        /// <summary>
        /// Save to ini
        /// </summary>
        /// <param name="keyDataCollection">Ini key value collection</param>
        public virtual void Save(KeyDataCollection keyDataCollection)
        {
            var c = keyDataCollection;
            keyDataCollection.AddKey("Name", _name);
            AddKey(keyDataCollection, "Kind", _kind);
            AddKey(keyDataCollection, "Relation", _relation);
            AddKey(keyDataCollection, "PathFinder", _pathFinder);
            AddKey(keyDataCollection, "State", _state);
            AddKey(keyDataCollection, "VisionRadius", _visionRadius);
            AddKey(keyDataCollection, "DialogRadius", _dialogRadius);
            AddKey(keyDataCollection, "AttackRadius", _attackRadius);
            AddKey(keyDataCollection, "Dir", _dir);
            keyDataCollection.AddKey("MapX", MapX.ToString());
            keyDataCollection.AddKey("MapY", MapY.ToString());
            AddKey(keyDataCollection, "Lum", _lum);
            AddKey(keyDataCollection, "Action", _action);
            AddKey(keyDataCollection, "WalkSpeed", _walkSpeed);
            AddKey(keyDataCollection, "Evade", _evade);
            AddKey(keyDataCollection, "Attack", _attack);
            AddKey(keyDataCollection, "AttackLevel", _attackLevel);
            AddKey(keyDataCollection, "Defend", _defend);
            AddKey(keyDataCollection, "Exp", _exp);
            AddKey(keyDataCollection, "LevelUpExp", _levelUpExp);
            AddKey(keyDataCollection, "Level", _level);
            AddKey(keyDataCollection, "Life", _life);
            AddKey(keyDataCollection, "LifeMax", _lifeMax);
            AddKey(keyDataCollection, "Thew", _thew);
            AddKey(keyDataCollection, "ThewMax", _thewMax);
            AddKey(keyDataCollection, "Mana", _mana);
            AddKey(keyDataCollection, "ManaMax", _manaMax);
            AddKey(keyDataCollection, "ExpBonus", _expBonus);
            AddKey(keyDataCollection, "FixedPos", _fixedPos);
            AddKey(keyDataCollection, "CurrentFixedPosIndex", CurrentFixedPosIndex);
            AddKey(keyDataCollection, "Idle", _idle);
            AddKey(keyDataCollection, "NpcIni", _npcIniFileName);
            if (_bodyIni != null)
            {
                AddKey(keyDataCollection, "BodyIni", _bodyIni.FileName);
            }
            if (_flyIni != null)
            {
                AddKey(keyDataCollection, "FlyIni", _flyIni.FileName);
            }
            if (_flyIni2 != null)
            {
                AddKey(keyDataCollection, "FlyIni2", _flyIni2.FileName);
            }
            if (_scriptFile != null)
            {
                AddKey(keyDataCollection, "ScriptFile", _scriptFile);
            }
            if (_deathScript != null)
            {
                AddKey(keyDataCollection, "DeathScript", _deathScript);
            }
        }

        #endregion Save load method

        #region Perform action
        public void StandingImmediately()
        {
            StateInitialize();
            if (_isInFighting && NpcIni.ContainsKey((int)CharacterState.FightStand)) SetState(CharacterState.FightStand);
            else
            {
                if (NpcIni.ContainsKey((int)CharacterState.Stand1) &&
                    Globals.TheRandom.Next(4) == 1 &&
                    State != (int)CharacterState.Stand1) SetState(CharacterState.Stand1);
                else SetState(CharacterState.Stand);
                PlayCurrentDirOnce();
            }
        }

        /// <summary>
        /// If is in walk or run, when reach next step(tile), standing.
        /// Don't like StandingImmediately() staning now,  this is a soft standing
        /// </summary>
        public void NextStepStaning()
        {
            if (IsWalking() || IsRuning())
            {
                _isNextStepStand = true;
            }
        }

        public bool PerformActionOk()
        {
            if (State == (int)CharacterState.Jump ||
                State == (int)CharacterState.Attack ||
                State == (int)CharacterState.Attack1 ||
                State == (int)CharacterState.Attack2 ||
                State == (int)CharacterState.Magic ||
                State == (int)CharacterState.Hurt ||
                State == (int)CharacterState.Death ||
                State == (int)CharacterState.FightJump ||
                IsPetrified ||
                _isInInteract) return false;
            return true;
        }

        public void StateInitialize()
        {
            EndPlayCurrentDirOnce();
            DestinationMoveTilePosition = Vector2.Zero;
            Path = null;
            CancleAttackTarget();
            if (_isInInteract)
            {
                //End interact in case of action to perform direction not correct
                EndInteract();
            }
        }

        public bool IsStanding()
        {
            return (State == (int)CharacterState.Stand ||
                    State == (int)CharacterState.Stand1 ||
                    State == (int)CharacterState.FightStand);
        }

        public bool IsWalking()
        {
            return (State == (int)CharacterState.Walk ||
                    State == (int)CharacterState.FightWalk);
        }

        public bool IsRuning()
        {
            return (State == (int)CharacterState.Run ||
                    State == (int)CharacterState.FightRun);
        }

        public bool IsSitting()
        {
            return State == (int)CharacterState.Sit;
        }

        /// <summary>
        /// Walk steps in direction.
        /// </summary>
        /// <param name="direction">Direction 0-7</param>
        /// <param name="moveStep">Steps to move</param>
        public void WalkToDirection(int direction, int moveStep)
        {
            _isInStepMove = true;
            _leftStepToMove = moveStep;
            _stepMoveDirection = direction;
            CheckStepMove();
        }

        /// <summary>
        /// Walk to destination tile position.
        /// </summary>
        /// <param name="destinationTilePosition">Destination tile positon</param>
        public virtual void WalkTo(Vector2 destinationTilePosition)
        {
            if (PerformActionOk() &&
                destinationTilePosition != TilePosition)
            {
                //If in step move, alway find new path
                if (IsWalking() && !IsInStepMove)
                {
                    DestinationMoveTilePosition = destinationTilePosition;
                    CancleAttackTarget();
                }
                else
                {
                    StateInitialize();
                    Path = Engine.PathFinder.FindPath(this, TilePosition, destinationTilePosition, PathType);
                    if (Path == null) StandingImmediately();
                    else
                    {
                        DestinationMoveTilePosition = destinationTilePosition;
                        if (_isInFighting && NpcIni.ContainsKey((int)CharacterState.FightWalk)) SetState(CharacterState.FightWalk);
                        else SetState(CharacterState.Walk);
                    }
                }
            }

        }

        public virtual void RunTo(Vector2 destinationTilePosition)
        {
            if (PerformActionOk() &&
                destinationTilePosition != TilePosition)
            {
                if (Thew > 0)
                {
                    if (IsRuning())
                        DestinationMoveTilePosition = destinationTilePosition;
                    else
                    {
                        StateInitialize();
                        Path = Engine.PathFinder.FindPath(this, TilePosition, destinationTilePosition, PathType);
                        if (Path == null) StandingImmediately();
                        else
                        {
                            DestinationMoveTilePosition = destinationTilePosition;
                            if (_isInFighting && NpcIni.ContainsKey((int)CharacterState.FightRun)) SetState(CharacterState.FightRun);
                            else SetState(CharacterState.Run);
                        }
                    }
                }
                else//Thew not enough
                {
                    WalkTo(destinationTilePosition);
                }
            }
        }

        public void JumpTo(Vector2 destinationTilePosition)
        {
            if (PerformActionOk() &&
                destinationTilePosition != TilePosition &&
                !Globals.TheMap.IsObstacleForCharacter(destinationTilePosition) &&
                !HasObstacle(destinationTilePosition))
            {
                if (!CanJump()) return;
                StateInitialize();
                DestinationMoveTilePosition = destinationTilePosition;
                Path = new LinkedList<Vector2>();
                Path.AddLast(PositionInWorld);
                Path.AddLast(DestinationMovePositionInWorld);

                if (_isInFighting && NpcIni.ContainsKey((int)CharacterState.FightJump)) SetState(CharacterState.FightJump);
                else SetState(CharacterState.Jump);
                SetDirection(DestinationMovePositionInWorld - PositionInWorld);
                PlayCurrentDirOnce();
            }
        }

        public void Sitdown()
        {
            if (PerformActionOk())
            {
                StateInitialize();
                if (NpcIni.ContainsKey((int)CharacterState.Sit))
                {
                    SetState(CharacterState.Sit);
                    PlayCurrentDirOnce();
                }
            }
        }

        public void UseMagic(Magic magicUse, Vector2 magicDestinationTilePosition)
        {
            if (PerformActionOk())
            {
                StateInitialize();
                ToFightingState();

                MagicUse = magicUse;
                _magicDestination = Map.ToPixelPosition(magicDestinationTilePosition);
                SetState(CharacterState.Magic);
                SetDirection(_magicDestination - PositionInWorld);
                PlayCurrentDirOnce();
            }
        }

        /// <summary>
        /// Character is hurting.
        /// Depending on posibility, character may or may not change to hurting state.
        /// When character is in pertrified state, hurting is ignored.
        /// </summary>
        public void Hurting()
        {
            const int maxRandValue = 4;
            if (Globals.TheRandom.Next(maxRandValue) != 0 ||
                IsPetrified) //Can't hurted when been petrified 
            {
                return;
            }

            if (State != (int)CharacterState.Death &&
                State != (int)CharacterState.Hurt &&
                !IsPetrified)
            {
                StateInitialize();
                TilePosition = TilePosition;//To tile center
                if (NpcIni.ContainsKey((int)CharacterState.Hurt))
                {
                    SetState(CharacterState.Hurt);
                    PlayCurrentDirOnce();
                }
            }
        }

        private static Asf FrozenDie;
        private static Asf PoisonDie;
        private static Asf PetrifiedDie;
        public void Death()
        {
            if (State == (int)CharacterState.Death) return;
            IsDeathInvoked = true;
            _currentRunDeathScript = ScriptManager.RunScript(Utils.GetScriptParser(DeathScript, this));
            StateInitialize();
            if (NpcIni.ContainsKey((int)CharacterState.Death))
            {
                SetState(CharacterState.Death);
                if (IsFrozened)
                {
                    if (FrozenDie == null) FrozenDie = Utils.GetAsf(@"asf\interlude\", "die-冰.asf");
                    Texture = FrozenDie;
                    CurrentDirection = 0;
                    _notAddBody = true;
                }
                else if (IsPoisoned)
                {
                    if (PoisonDie == null) PoisonDie = Utils.GetAsf(@"asf\interlude\", "die-毒.asf");
                    Texture = PoisonDie;
                    CurrentDirection = 0;
                    _notAddBody = true;
                }
                else if (IsPetrified)
                {
                    if (PetrifiedDie == null) PetrifiedDie = Utils.GetAsf(@"asf\interlude\", "die-石.asf");
                    Texture = PetrifiedDie;
                    CurrentDirection = 0;
                    _notAddBody = true;
                }
                ToNormalState();
                PlayCurrentDirOnce();
            }
            else IsDeath = true;
        }

        public void Attacking(Vector2 destinationTilePosition, bool isRun = false)
        {
            if (PerformActionOk())
            {
                _isRunToTarget = isRun;
                DestinationAttackTilePosition = destinationTilePosition;
                if (IsStanding() && AttackingIsOk())
                    PerformeAttack();
            }
        }

        public void CancleAttackTarget()
        {
            DestinationAttackTilePosition = Vector2.Zero;
            _interactiveTarget = null;
        }

        protected bool AttackingIsOk()
        {
            if (DestinationAttackTilePosition != Vector2.Zero)
            {
                int tileDistance = Engine.PathFinder.GetTileDistance(TilePosition, DestinationAttackTilePosition);
               
                if (tileDistance == AttackRadius)
                {
                    var canSeeTarget = Engine.PathFinder.CanViewTarget(TilePosition, 
                        DestinationAttackTilePosition,
                        AttackRadius);

                    if (canSeeTarget) return true;

                    MoveToTarget(DestinationAttackTilePosition, _isRunToTarget);
                }
                if (tileDistance > AttackRadius)
                {
                    MoveToTarget(DestinationAttackTilePosition, _isRunToTarget);
                }
                else
                {
                    //Actack distance too small, move away target
                    if (!MoveAwayTarget(DestinationAttackPositionInWorld, _isRunToTarget)) return true;
                }
            }
            return false;
        }

        protected bool MoveAwayTarget(Vector2 targetPositionInWorld, bool isRun)
        {
            var neighbor = Engine.PathFinder.FindNeighborInDirection(TilePosition,
                        PositionInWorld - targetPositionInWorld);

            if (HasObstacle(neighbor)) return false;

            Path = Engine.PathFinder.FindPath(this, TilePosition, neighbor, PathType);
            if (Path == null) return false;

            MoveToTarget(neighbor, isRun);
            return true;
        }

        private void MoveToTarget(Vector2 destinationTilePosition, bool isRun)
        {
            if (isRun)
                RunToAndKeepingTarget(destinationTilePosition);
            else
                WalkToAndKeepingTarget(destinationTilePosition);
        }

        protected void WalkToAndKeepingTarget(Vector2 destinationTilePosition)
        {
            //keep value
            var attack = DestinationAttackTilePosition;
            var interact = _interactiveTarget;
            WalkTo(destinationTilePosition);
            // restore
            DestinationAttackTilePosition = attack;
            _interactiveTarget = interact;
        }

        protected void RunToAndKeepingTarget(Vector2 destinationTilePosition)
        {
            //keep value
            var attack = DestinationAttackTilePosition;
            var interact = _interactiveTarget;
            RunTo(destinationTilePosition);
            // restore
            DestinationAttackTilePosition = attack;
            _interactiveTarget = interact;
        }

        protected void PerformeAttack()
        {
            PerformeAttack(DestinationAttackPositionInWorld);
        }

        protected virtual bool CanPerformeAttack()
        {
            return true;
        }

        public void PerformeAttack(Vector2 destinationPositionInWorld)
        {
            if (PerformActionOk())
            {
                if (!CanPerformeAttack()) return;
                StateInitialize();
                ToFightingState();
                _attackDestination = destinationPositionInWorld;

                var value = Globals.TheRandom.Next(3);
                if (value == 1 && NpcIni.ContainsKey((int)CharacterState.Attack1))
                    SetState(CharacterState.Attack1);
                else if (value == 2 && NpcIni.ContainsKey((int)CharacterState.Attack2))
                    SetState(CharacterState.Attack2);
                else SetState(CharacterState.Attack);
                SetDirection(destinationPositionInWorld - PositionInWorld);
                PlayCurrentDirOnce();
            }
        }

        public void ToNonFightingState()
        {
            _isInFighting = false;
            if (IsWalking()) SetState(CharacterState.Walk);
            if (IsRuning()) SetState(CharacterState.Run);
            if (State == (int)CharacterState.FightStand) SetState(CharacterState.Stand);
        }

        public void ToFightingState()
        {
            _isInFighting = true;
            _totalNonFightingSeconds = 0;
        }

        public void InteractWith(object target, bool isRun = false)
        {
            if (PerformActionOk())
            {
                _interactiveTarget = target;
                _isRunToTarget = isRun;

                Vector2 destinationTilePositon;
                int interactDistance;
                if (!GetInteractTargetInfo(out destinationTilePositon, out interactDistance))
                {
                    return;
                }
                DestinationMoveTilePosition = destinationTilePositon;
                //if can't reach destination tile positon, find neighbor tile
                if (Globals.TheMap.IsObstacleForCharacter(destinationTilePositon))
                {
                    //find directional neighbor
                    var neighbor = Engine.PathFinder.FindNeighborInDirection(destinationTilePositon,
                    destinationTilePositon - PositionInWorld);
                    //if can't find  or neighbor is obstacle, try other neighbor
                    if (neighbor == Vector2.Zero ||
                        Globals.TheMap.IsObstacleForCharacter(neighbor))
                    {
                        var neighbors = Engine.PathFinder.FindNeighbors(destinationTilePositon);
                        foreach (var tile in neighbors)
                        {
                            if (!Globals.TheMap.IsObstacleForCharacter(tile))
                            {
                                DestinationMoveTilePosition = tile;
                                break;
                            }
                        }
                    }
                    else
                    {
                        DestinationMoveTilePosition = neighbor;
                    }
                }

                if (IsStanding() && InteractIsOk())
                    PerformeInteract();
            }
        }

        private bool GetInteractTargetInfo(out Vector2 tilePosition, out int interactDistance)
        {
            tilePosition = Vector2.Zero;
            interactDistance = 0;
            if (_interactiveTarget == null) return false;
            var character = _interactiveTarget as Character;
            var obj = _interactiveTarget as Obj;
            if (character != null)
            {
                tilePosition = character.TilePosition;
                interactDistance = character.DialogRadius;
            }
            else if (obj != null)
            {
                tilePosition = obj.TilePosition;
                interactDistance = 1;
            }
            else
                return false;
            return true;
        }

        private bool InteractIsOk()
        {
            if (_interactiveTarget == null) return false;
            Vector2 destinationTilePositon;
            int interactDistance;
            if (!GetInteractTargetInfo(out destinationTilePositon, out interactDistance))
            {
                return false;
            }

            var tileDistance = Engine.PathFinder.GetTileDistance(TilePosition, destinationTilePositon);
            if (tileDistance <= interactDistance)
            {
                return true;
            }
            else
            {
                MoveToTarget(DestinationMoveTilePosition, _isRunToTarget);
                return false;
            }
        }

        private void PerformeInteract()
        {
            if (PerformActionOk())
            {
                var character = _interactiveTarget as Character;
                var obj = _interactiveTarget as Obj;
                if (character != null)
                {
                    character.StartInteract(this);
                    SetDirection(character.PositionInWorld - PositionInWorld);
                }
                else if (obj != null)
                {
                    obj.StartInteract();
                    SetDirection(obj.PositionInWorld - PositionInWorld);
                }
                StandingImmediately();
            }
        }

        public void StartInteract(Character from)
        {
            if (from != null && !string.IsNullOrEmpty(ScriptFile))
            {
                _isInInteract = true;
                _directionBeforInteract = CurrentDirection;
                SetDirection(from.PositionInWorld - PositionInWorld);
                _currentRunInteractScript = ScriptManager.RunScript(Utils.GetScriptParser(ScriptFile, this));
            }
        }

        public bool IsInteractEnd()
        {
            if (_currentRunInteractScript != null)
            {
                return _currentRunInteractScript.IsEnd;
            }
            return true;
        }

        #endregion Perform action

        #region Character state set and get method

        /// <summary>
        /// Set character state.Change texture and play sound.
        /// </summary>
        /// <param name="state">State to set</param>
        /// <param name="setIfStateSame">If true renew state if current state is same as the setting state</param>
        public void SetState(CharacterState state, bool setIfStateSame = false)
        {
            if ((State != (int)state || setIfStateSame) &&
                NpcIni.ContainsKey((int)state))
            {
                if (_sound != null)
                {
                    _sound.Stop(true);
                    _sound = null;
                }

                var image = NpcIni[(int)state].Image;
                var sound = NpcIni[(int)state].Sound;
                Texture = image;
                if (sound != null)
                {
                    switch (state)
                    {
                        case CharacterState.Walk:
                        case CharacterState.FightWalk:
                        case CharacterState.Run:
                        case CharacterState.FightRun:
                            {
                                _sound = sound.CreateInstance();
                                _sound.IsLooped = true;
                                SoundManager.Apply3D(_sound,
                                    PositionInWorld - Globals.ListenerPosition);
                                _sound.Play();
                            }
                            break;
                        case CharacterState.Magic:
                        case CharacterState.Attack:
                        case CharacterState.Attack1:
                        case CharacterState.Attack2:
                            {
                                //do nothing
                            }
                            break;
                        default:
                            PlaySoundEffect(sound);
                            break;
                    }
                }
                State = (int)state;
            }
        }

        /// <summary>
        /// Clear frozen, poison, petrifaction
        /// </summary>
        public void ToNormalState()
        {
            ClearFrozen();
            ClearPoison();
            ClearPetrifaction();
        }

        public void ClearFrozen()
        {
            _frozenSeconds = 0;
        }

        public void ClearPoison()
        {
            _poisonSeconds = 0;
        }

        public void ClearPetrifaction()
        {
            _petrifiedSeconds = 0;
        }

        public static float GetTrueDistance(Vector2 distance)
        {
            var unit = Utils.GetDirection8(Utils.GetDirectionIndex(distance, 8));
            distance = (2f - Math.Abs(unit.X)) * distance;
            return distance.Length();
        }

        /// <summary>
        /// Add amount of life.Amount can be negative.
        /// If list is less than 0.Character is death and <see cref="Death"/> is invoked.
        /// </summary>
        /// <param name="amount">Amount to add.If amount is nagetive, life is decreased.</param>
        public void AddLife(int amount)
        {
            Life += amount;
            if (Life <= 0) Death();
        }

        /// <summary>
        /// Decrease amount of life.If life is greater than 0, <see cref="Hurting"/> is invoked.
        /// Amount must be positive, oherwise no effect.
        /// </summary>
        /// <param name="amount">Amount must be positive, oherwise no effect.</param>
        public void DecreaseLifeAddHurt(int amount)
        {
            if(amount <= 0) return;
            AddLife(-amount);
            if (Life > 0)
            {
                Hurting();
            }
        }

        public void AddMana(int amount)
        {
            Mana += amount;
        }

        public void AddThew(int amount)
        {
            Thew += amount;
        }

        public virtual void AddMagic(string magicFileName)
        {

        }

        /// <summary>
        /// Level up to level
        /// </summary>
        /// <param name="level">The level</param>
        public virtual void SetLevelTo(int level)
        {
            if (LevelIni == null)
            {
                Level = level;
                return;
            }
            Utils.LevelDetail detail = null, currentDetail = null;
            if (LevelIni.ContainsKey(level) &&
                LevelIni.ContainsKey(Level))
            {
                detail = LevelIni[level];
                currentDetail = LevelIni[Level];
            }
            if (detail != null)
            {
                LifeMax += (detail.LifeMax - currentDetail.LifeMax);
                ThewMax += (detail.ThewMax - currentDetail.ThewMax);
                ManaMax += (detail.ManaMax - currentDetail.ManaMax);
                Life = LifeMax;
                Thew = ThewMax;
                Mana = ManaMax;
                Attack += (detail.Attack - currentDetail.Attack);
                Defend += (detail.Defend - currentDetail.Defend);
                Evade += (detail.Evade - currentDetail.Evade);
                LevelUpExp = detail.LevelUpExp;
                Level = level;
                if (!string.IsNullOrEmpty(detail.NewMagic))
                {
                    AddMagic(detail.NewMagic);
                }
            }
            else
            {
                Exp = 0;
                LevelUpExp = 0;
            }
        }

        public void FullLife()
        {
            Life = LifeMax;
            IsDeath = false;
            IsDeathInvoked = false;
        }

        public void FullThew()
        {
            Thew = ThewMax;
        }

        public void FullMana()
        {
            Mana = ManaMax;
        }

        public void SetKind(int kind)
        {
            Kind = kind;
        }

        /// <summary>
        /// Set FlyIni
        /// </summary>
        /// <param name="fileName">Magic file name</param>
        public virtual void SetMagicFile(string fileName)
        {
            FlyIni = Utils.GetMagic(fileName);
        }

        public virtual void SetTilePosition(Vector2 tilePosition)
        {
            TilePosition = tilePosition;
        }

        public virtual void SetPosition(Vector2 tilePosition)
        {
            //Stand when reposition
            StandingImmediately();
            TilePosition = tilePosition;
        }

        public virtual void SetRelation(int relation)
        {

            if (
                (Relation == (int)RelationType.Friend &&
                relation == (int)RelationType.Enemy) ||
                (Relation == (int)RelationType.Enemy &&
                 relation != (int)RelationType.Enemy)
                )
            {
                //Character can't follow current target now
                FollowTarget = null;
            }
            Relation = relation;
        }

        /// <summary>
        /// Set NpcIni flle
        /// </summary>
        /// <param name="fileName"></param>
        public void SetNpcIni(string fileName)
        {
            if (string.IsNullOrEmpty(fileName)) return;
            _npcIniFileName = fileName;
            NpcIni = ResFile.ReadFile(@"ini\npcres\" + fileName, ResType.Npc);
        }

        /// <summary>
        /// Set NpcIni flle and refresh draw image
        /// </summary>
        /// <param name="fileName"></param>
        public void SetRes(string fileName)
        {
            SetNpcIni(fileName);
            if (NpcIni != null && NpcIni.Count > 0)
            {
                if (NpcIni.ContainsKey(State))
                {
                    SetState((CharacterState)State, true);
                }
                else
                {
                    foreach (var key in NpcIni.Keys)
                    {
                        SetState((CharacterState)key);
                        break;
                    }
                }
            }
            else
            {
                Texture = null;
            }
        }

        /// <summary>
        /// Set state image and refresh draw
        /// </summary>
        /// <param name="state">State to set</param>
        /// <param name="fileName">Asf file name</param>
        public void SetNpcActionFile(CharacterState state, string fileName)
        {
            ResFile.SetNpcStateImage(NpcIni, state, fileName);
            SetState((CharacterState)State, true);
        }

        public void SetNpcActionType(int type)
        {
            Action = type;
        }

        public void DisableFight()
        {
            IsFightDisabled = true;
        }

        public void EnableFight()
        {
            IsFightDisabled = false;
        }

        public void DisableJump()
        {
            IsJumpDisabled = true;
        }

        public void EnableJump()
        {
            IsJumpDisabled = false;
        }

        public void DisableRun()
        {
            IsRunDisabled = true;
        }

        public void EnableRun()
        {
            IsRunDisabled = false;
        }

        public void SetFightState(bool isFight)
        {
            if (isFight)
            {
                ToFightingState();
                SetState(CharacterState.FightStand);
            }
            else
            {
                ToNonFightingState();
                SetState(CharacterState.Stand);
            }
        }

        /// <summary>
        /// Make this enemy and all neighbor enemy walk to target and follow target 
        /// If follow target is already finded and distance is less than new target,
        /// don't walk to and follow new target
        /// </summary>
        /// <param name="target">The target</param>
        public void NotifyEnemyAndAllNeighbor(Character target)
        {
            //If target is null or this character is not enemy return
            if (target == null || !IsEnemy) return;
            var characters = NpcManager.GetNeighborEnemy(this);
            characters.Add(this);
            foreach (var character in characters)
            {
                if (character.FollowTarget != null &&
                    character.IsFollowTargetFound &&
                    Vector2.Distance(character.PositionInWorld, character.FollowTarget.PositionInWorld) <
                    Vector2.Distance(character.PositionInWorld, target.PositionInWorld))
                {
                    continue;
                }
                character.FollowAndWalkToTarget(target);
            }
        }

        /// <summary>
        /// Walk to target and follow target
        /// </summary>
        /// <param name="target">Target to walk to</param>
        public void FollowAndWalkToTarget(Character target)
        {
            WalkTo(target.TilePosition);
            Follow(target);
        }

        /// <summary>
        /// Set follow target
        /// </summary>
        /// <param name="target">Follow target</param>
        public void Follow(Character target)
        {
            FollowTarget = target;
            IsFollowTargetFound = true;
        }

        public void SetSpecialAction(string asfFileName)
        {
            IsInSpecialAction = true;
            _specialActionLastDirection = CurrentDirection;
            EndPlayCurrentDirOnce();
            Texture = Utils.GetCharacterAsf(asfFileName);
            PlayCurrentDirOnce();
        }
        #endregion Character state set and get method

        #region Update Draw
        public override void Update(GameTime gameTime)
        {
            if (IsInSpecialAction)
            {
                base.Update(gameTime);
                if (IsPlayCurrentDirOnceEnd())
                {
                    IsInSpecialAction = false;
                    ResetState();
                    //Restore direction
                    SetDirectionValue(_specialActionLastDirection);
                }
                return;
            }

            if (IsDeath) return;

            if (_isInInteract && IsInteractEnd())
            {
                EndInteract();
            }

            var elapsedGameTime = gameTime.ElapsedGameTime;

            if (PoisonSeconds > 0)
            {
                PoisonSeconds -= (float)elapsedGameTime.TotalSeconds;
                _poisonedMilliSeconds += (float)elapsedGameTime.TotalMilliseconds;
                if (_poisonedMilliSeconds > 250)
                {
                    _poisonedMilliSeconds = 0;
                    AddLife(-10);
                }
            }

            if (PetrifiedSeconds > 0)
            {
                PetrifiedSeconds -= (float)elapsedGameTime.TotalSeconds;
                return;
            }

            if (FrozenSeconds > 0)
            {
                FrozenSeconds -= (float)elapsedGameTime.TotalSeconds;
                elapsedGameTime = new TimeSpan(elapsedGameTime.Ticks / 2);
            }

            switch ((CharacterState)State)
            {
                case CharacterState.Walk:
                case CharacterState.FightWalk:
                    {
                        if (_fixedPathMoveDestinationPixelPostion != Vector2.Zero)
                        {
                            MoveTo(_fixedPathMoveDestinationPixelPostion - PositionInWorld,
                                (float)elapsedGameTime.TotalSeconds * WalkSpeed * 2);
                            if (MovedDistance >= _fixedPathDistanceToMove)
                            {
                                StandingImmediately();
                                _fixedPathMoveDestinationPixelPostion = Vector2.Zero;
                            }
                        }
                        else
                        {
                            MoveAlongPath((float)elapsedGameTime.TotalSeconds, WalkSpeed);
                        }
                        SoundManager.Apply3D(_sound,
                                    PositionInWorld - Globals.ListenerPosition);
                        Update(gameTime, WalkSpeed);
                    }
                    break;
                case CharacterState.Run:
                case CharacterState.FightRun:
                    MoveAlongPath((float)elapsedGameTime.TotalSeconds, 8);
                    SoundManager.Apply3D(_sound,
                                    PositionInWorld - Globals.ListenerPosition);
                    base.Update(gameTime);
                    break;
                case CharacterState.Jump:
                case CharacterState.FightJump:
                    JumpAlongPath((float)elapsedGameTime.TotalSeconds);
                    base.Update(gameTime);
                    break;
                case CharacterState.Stand:
                case CharacterState.Stand1:
                case CharacterState.Hurt:
                    if (IsPlayCurrentDirOnceEnd()) StandingImmediately();
                    else base.Update(gameTime);
                    break;
                case CharacterState.Attack:
                case CharacterState.Attack1:
                case CharacterState.Attack2:
                    if (IsPlayCurrentDirOnceEnd())
                    {
                        if (NpcIni.ContainsKey(State))
                        {
                            PlaySoundEffect(NpcIni[State].Sound);
                        }
                        var magic = FlyIni;
                        if (FlyIni2 != null && Globals.TheRandom.Next(8) == 0)
                            magic = FlyIni2;
                        MagicManager.UseMagic(this,
                            magic,
                            PositionInWorld,
                            _attackDestination);

                        //Do somethig when attacking
                        OnAttacking(_attackDestination);

                        StandingImmediately();
                    }
                    else base.Update(gameTime);
                    break;
                case CharacterState.Magic:
                    if (IsPlayCurrentDirOnceEnd())
                    {
                        if (NpcIni.ContainsKey((int)CharacterState.Magic))
                        {
                            PlaySoundEffect(NpcIni[(int)CharacterState.Magic].Sound);
                        }
                        if (CanUseMagic())
                            MagicManager.UseMagic(this, MagicUse, PositionInWorld, _magicDestination);
                        StandingImmediately();
                    }
                    else base.Update(gameTime);
                    break;
                case CharacterState.Sit:
                    if (!IsPlayCurrentDirOnceEnd()) base.Update(gameTime);
                    break;
                case CharacterState.Death:
                    if (IsPlayCurrentDirOnceEnd())
                    {
                        IsDeath = true;
                    }
                    else base.Update(gameTime);
                    break;
                default:
                    base.Update(gameTime);
                    break;
            }
            if (_isInFighting)
            {
                _totalNonFightingSeconds += (float)gameTime.ElapsedGameTime.TotalSeconds;
                if (_totalNonFightingSeconds > MaxNonFightSeconds)
                {
                    ToNonFightingState();
                }
            }

            for (var node = MagicSpritesInEffect.First; node != null; )
            {
                var next = node.Next;
                var magicSprite = node.Value;
                if (magicSprite.IsDestroyed)
                    MagicSpritesInEffect.Remove(node);
                node = next;
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            Draw(spriteBatch, GetCurrentTexture());
        }

        public virtual void Draw(SpriteBatch spriteBatch, Texture2D texture)
        {
            if (IsDeath || IsHide) return;
            var color = DrawColor;
            if (FrozenSeconds > 0)
                color = new Color(80, 80, 255);
            if (PoisonSeconds > 0)
                color = new Color(50, 255, 50);
            if (PetrifiedSeconds > 0)
                texture = TextureGenerator.ToGrayScale(texture);
            base.Draw(spriteBatch, texture, color);
        }
        #endregion Update Draw

        #region Type
        public enum CharacterKind
        {
            Normal,
            Fighter,
            Player,
            Follower,
            GroundAnimal,
            Eventer,
            AfraidPlayerAnimal,
            Flyer
        }

        public enum RelationType
        {
            Friend,
            Enemy,
            Neutral
        }

        public enum ActionType
        {
            Stand,
            RandWalk,
            LoopWalk
        }
        #endregion Type
    }
}
