using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Engine.Script;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace Engine
{
    using StateMapList = Dictionary<int, ResStateInfo>;
    public abstract class Character : Sprite
    {
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
        private StateMapList _npcIni;
        private Obj _bodyIni;
        private Magic _flyIni;
        private Magic _flyIni2;
        private ScriptParser _scriptFile;
        private ScriptParser _deathScript;
        private int _expBonus;
        private int _fixedPos;
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
        private bool _isRunToTarget;
        private bool _isDeath;
        private float _poisonedMilliSeconds;
        private float _frozenSeconds;
        private float _poisonSeconds;
        private float _petrifiedSeconds;
        private bool _notAddBody;
        private Dictionary<int, Utils.LevelDetail> _levelIni;
        protected Magic MagicUse;

        #region Public properties

        public LinkedList<MagicSprite> MagicSpritesInEffect = new LinkedList<MagicSprite>();

        public Dictionary<int, Utils.LevelDetail> LevelIni
        {
            get { return _levelIni; }
            set { _levelIni = value; }
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

        public virtual int PathFinder
        {
            get { return _pathFinder; }
            set { _pathFinder = value; }
        }

        public abstract Engine.PathFinder.PathType PathType { get; }

        public int VisionRadius
        {
            get { return _visionRadius; }
            set { _visionRadius = value; }
        }

        public int DialogRadius
        {
            get { return _dialogRadius; }
            set { _dialogRadius = value; }
        }

        public int AttackRadius
        {
            get { return _attackRadius; }
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

        public StateMapList NpcIni
        {
            get { return _npcIni; }
            set { _npcIni = value; }
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

        public ScriptParser ScriptFile
        {
            get { return _scriptFile; }
            set { _scriptFile = value; }
        }

        public ScriptParser DeathScript
        {
            get { return _deathScript; }
            set { _deathScript = value; }
        }

        public int ExpBonus
        {
            get { return _expBonus; }
            set { _expBonus = value; }
        }

        public int FixedPos
        {
            get { return _fixedPos; }
            set { _fixedPos = value; }
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

        protected LinkedList<Vector2> Path
        {
            get { return _path; }
            set
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

        public bool IsInDeathing { get { return State == (int)CharacterState.Death; } }

        #endregion Public properties

        public Character(string filePath)
        {
            Load(filePath);
        }

        public Character() { }

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

        protected virtual bool MagicFromCache { get { return true; } }

        private void AssignToValue(string[] nameValue)
        {
            try
            {
                var info = this.GetType().GetProperty(nameValue[0]);
                switch (nameValue[0])
                {
                    case "FixedPos":
                    case "Name":
                        info.SetValue(this, nameValue[1], null);
                        break;
                    case "ScriptFile":
                    case "DeathScript":
                        if (!string.IsNullOrEmpty(nameValue[1]))
                            info.SetValue(this,
                                new ScriptParser(Utils.GetScriptFilePath(nameValue[1]), this),
                                null);
                        break;
                    case "NpcIni":
                        info.SetValue(this,
                            ResFile.ReadFile(@"ini\npcres\" + nameValue[1], ResType.Npc),
                            null);
                        break;
                    case "BodyIni":
                        info.SetValue(this, new Obj(@"ini\obj\" + nameValue[1]), null);
                        break;
                    case "Defence":
                        Defend = int.Parse(nameValue[1]);
                        break;
                    case "LevelIni":
                        info.SetValue(this, Utils.GetLevelLists(@"ini\level\" + nameValue[1]), null);
                        break;
                    case "FlyIni":
                    case "FlyIni2":
                        info.SetValue(this, Utils.GetMagic(nameValue[1], MagicFromCache), null);
                        break;
                    case "Life":
                        _life = int.Parse(nameValue[1]);
                        break;
                    case "Thew":
                        _thew = int.Parse(nameValue[1]);
                        break;
                    case "Mana":
                        _mana = int.Parse(nameValue[1]);
                        break;
                    default:
                        {
                            var integerValue = int.Parse(nameValue[1]);
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
            direction.Normalize();
            var distance = Vector2.Distance(from, to);

            if (TilePosition == tileFrom)
            {
                if (HasObstacle(tileTo)) //Obstacle in the way
                {
                    //PositionInWorld = from;
                    var path = Engine.PathFinder.FindPath(TilePosition, DestinationMoveTilePosition, PathType);
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
                if (DestinationMovePositionInWorld != Path.Last.Value) // new destination
                {
                    var destination = DestinationMovePositionInWorld;
                    PositionInWorld = to;
                    Path = Engine.PathFinder.FindPath(TilePosition, Map.ToTilePosition(destination), PathType);
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
                        StandingImmediately();
                        MovedDistance = 0;
                    }
                }
                CheckMapTrap();
                if (AttackingIsOk()) PerformeAttack();
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

        protected abstract bool HasObstacle(Vector2 tilePosition);
        protected virtual void CheckMapTrap() { }

        public bool Load(string filePath)
        {
            try
            {
                var lines = File.ReadAllLines(filePath, Globals.SimpleChinaeseEncoding);
                return Load(lines);
            }
            catch (Exception exception)
            {
                Log.LogFileLoadError("Character", filePath, exception);
                return false;
            }
        }

        public bool Load(string[] lines)
        {
            foreach (var line in lines)
            {
                var nameValue = Utils.GetNameValue(line);
                if (!string.IsNullOrEmpty(nameValue[0]))
                    AssignToValue(nameValue);
            }
            Initlize();
            return true;
        }

        public void SetState(CharacterState state)
        {
            if (State != (int)state && NpcIni.ContainsKey((int)state))
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

        public void Standing()
        {
            WalkTo(Vector2.Zero);
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
                IsPetrified) return false;
            return true;
        }

        public void StateInitialize()
        {
            EndPlayCurrentDirOnce();
            DestinationMoveTilePosition = Vector2.Zero;
            Path = null;
            ClearTarget();
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

        public void WalkTo(Vector2 destinationTilePosition)
        {
            if (PerformActionOk() &&
                destinationTilePosition != TilePosition)
            {
                if (IsWalking())
                {
                    DestinationMoveTilePosition = destinationTilePosition;
                    ClearTarget();
                }
                else
                {
                    StateInitialize();
                    Path = Engine.PathFinder.FindPath(TilePosition, destinationTilePosition, PathType);
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

        public void RunTo(Vector2 destinationTilePosition)
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
                        Path = Engine.PathFinder.FindPath(TilePosition, destinationTilePosition, PathType);
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

        protected virtual bool CanRunning()
        {
            return true;
        }

        protected virtual bool CanJump()
        {
            return true;
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
                _isInFighting = true;
                _totalNonFightingSeconds = 0;

                MagicUse = magicUse;
                _magicDestination = Map.ToPixelPosition(magicDestinationTilePosition);
                SetState(CharacterState.Magic);
                SetDirection(_magicDestination - PositionInWorld);
                PlayCurrentDirOnce();
            }
        }

        public void Hurting()
        {
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
            ScriptManager.RunScript(DeathScript);
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

        public void ClearTarget()
        {
            DestinationAttackTilePosition = Vector2.Zero;
            _interactiveTarget = null;
        }

        protected bool AttackingIsOk()
        {
            if (DestinationAttackTilePosition != Vector2.Zero)
            {
                int tileDistance;
                var attackCanReach = Engine.PathFinder.CanMagicReach(TilePosition, DestinationAttackTilePosition,
                    out tileDistance);
                if (tileDistance == AttackRadius)
                {
                    if (attackCanReach) return true;
                    else
                        MoveToTarget(DestinationAttackTilePosition);
                }
                if (tileDistance > AttackRadius)
                {
                    MoveToTarget(DestinationAttackTilePosition);
                }
                else
                {
                    var neighbor = Engine.PathFinder.FindNeighborInDirection(TilePosition,
                        PositionInWorld - DestinationAttackPositionInWorld);

                    if (HasObstacle(neighbor)) return true;

                    Path = Engine.PathFinder.FindPath(TilePosition, neighbor, PathType);
                    if (Path == null) return true;

                    MoveToTarget(neighbor);
                }
            }
            return false;
        }

        private void MoveToTarget(Vector2 destinationTilePosition)
        {
            if (_isRunToTarget)
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

        public void PerformeAttack(Vector2 destination)
        {
            if (PerformActionOk())
            {
                if (!CanPerformeAttack()) return;
                StateInitialize();
                _isInFighting = true;
                _totalNonFightingSeconds = 0;
                _attackDestination = destination;

                var value = Globals.TheRandom.Next(3);
                if (value == 1 && NpcIni.ContainsKey((int)CharacterState.Attack1))
                    SetState(CharacterState.Attack1);
                else if (value == 2 && NpcIni.ContainsKey((int)CharacterState.Attack2))
                    SetState(CharacterState.Attack2);
                else SetState(CharacterState.Attack);
                SetDirection(destination - PositionInWorld);
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

        public void InteractWith(object target, bool isRun = false)
        {
            if (PerformActionOk())
            {
                _isRunToTarget = isRun;
                _interactiveTarget = target;
                if (IsStanding() && InteractIsOk())
                    PerformeInteract();
            }
        }

        private bool InteractIsOk()
        {
            if (_interactiveTarget == null) return false;
            var character = _interactiveTarget as Character;
            var obj = _interactiveTarget as Obj;
            Vector2 destinationTilePositon;
            int interactDistance;
            if (character != null)
            {
                destinationTilePositon = character.TilePosition;
                interactDistance = character.DialogRadius;
            }
            else if (obj != null)
            {
                destinationTilePositon = obj.TilePosition;
                interactDistance = 1;
            }
            else
                return false;

            var tileDistance = Engine.PathFinder.GetTileDistance(TilePosition, destinationTilePositon);
            if (tileDistance <= interactDistance)
            {
                return true;
            }
            else
            {
                MoveToTarget(destinationTilePositon);
                return false;
            }
        }

        private void PerformeInteract()
        {
            if (PerformActionOk())
            {
                var character = _interactiveTarget as Character;
                var obj = _interactiveTarget as Obj;
                ScriptParser script = null;
                if (character != null)
                {
                    script = character.ScriptFile;
                    SetDirection(character.PositionInWorld - PositionInWorld);
                }
                else if (obj != null)
                {
                    script = obj.ScriptFile;
                    SetDirection(obj.PositionInWorld - PositionInWorld);
                }
                StandingImmediately();
                ScriptManager.RunScript(script);
            }
        }
        #endregion Perform action

        public static float GetTrueDistance(Vector2 distance)
        {
            var unit = Utils.GetDirection8(Utils.GetDirectionIndex(distance, 8));
            distance = (2f - Math.Abs(unit.X)) * distance;
            return distance.Length();
        }

        public void AddLife(int amount)
        {
            Life += amount;
            if (Life <= 0) Death();
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

        protected abstract void PlaySoundEffect(SoundEffect soundEffect);

        protected virtual bool CanUseMagic()
        {
            return true;
        }

        public override void Update(GameTime gameTime)
        {
            if (IsDeath) return;

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
                        MoveAlongPath((float)elapsedGameTime.TotalSeconds, WalkSpeed);
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
                        if (BodyIni != null && !_notAddBody)
                        {
                            BodyIni.PositionInWorld = PositionInWorld;
                            BodyIni.CurrentDirection = CurrentDirection;
                            ObjManager.AddObj(BodyIni);
                        }
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
                magicSprite.Update(gameTime);
                if (magicSprite.IsDestroyed)
                    MagicSpritesInEffect.Remove(node);
                node = next;
            }
        }

        public void DrawMagicSpriteInEffect(SpriteBatch spriteBatch)
        {
            foreach (var magicSprite in MagicSpritesInEffect)
            {
                magicSprite.Draw(spriteBatch);
            }
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            Draw(spriteBatch, GetCurrentTexture());
        }

        public virtual void Draw(SpriteBatch spriteBatch, Texture2D texture)
        {
            if (IsDeath) return;
            var color = Color.White;
            if (FrozenSeconds > 0)
                color = new Color(80, 80, 255);
            if (PoisonSeconds > 0)
                color = new Color(50, 255, 50);
            if (PetrifiedSeconds > 0)
                texture = TextureGenerator.ToGrayScale(texture);
            DrawMagicSpriteInEffect(spriteBatch);
            base.Draw(spriteBatch, texture, color);
        }
    }
}
