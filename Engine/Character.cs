using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
        private string _scriptFile;
        private string _deathScript;
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
        private bool _isDeath;
        public Magic MagicUse;

        #region Public properties
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
            set { _life = value < 0 ? 0 : value; }
        }

        public int Level
        {
            get { return _level; }
            set { _level = value; }
        }

        public int LifeMax
        {
            get { return _lifeMax; }
            set { _lifeMax = value; }
        }

        public int Thew
        {
            get { return _thew; }
            set { _thew = value < 0 ? 0 : value; }
        }

        public int ThewMax
        {
            get { return _thewMax; }
            set { _thewMax = value; }
        }

        public int Mana
        {
            get { return _mana; }
            set { _mana = value < 0 ? 0 : value; }
        }

        public int ManaMax
        {
            get { return _manaMax; }
            set { _manaMax = value; }
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

        public string ScriptFile
        {
            get { return _scriptFile; }
            set { _scriptFile = value; }
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

        #endregion Public properties

        public Character(string filePath)
        {
            Load(filePath);
        }

        public Character() { }

        private void Initlize()
        {
            if (NpcIni.ContainsKey((int)NpcState.Stand))
            {
                Set(Map.ToPixelPosition(MapX, MapY),
                    Globals.BaseSpeed,
                    NpcIni[(int)NpcState.Stand].Image, Dir);
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

        private void AssignToValue(string[] nameValue)
        {
            try
            {
                var info = this.GetType().GetProperty(nameValue[0]);
                switch (nameValue[0])
                {
                    case "DeathScript":
                    case "FixedPos":
                    case "Name":
                    case "ScriptFile":
                        info.SetValue(this, nameValue[1], null);
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
                        info.SetValue(this, Utils.GetMagic(nameValue[1]), null);
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

        protected abstract bool HasObstacle(Vector2 tilePosition);

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
                if (AttackingIsOk()) PerformeAttack();
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
            if (IsPlayCurrentDirOnceEnd()) StandingImmediately();
        }

        public bool Load(string filePath)
        {
            try
            {
                var lines = File.ReadAllLines(filePath, Globals.SimpleChinaeseEncoding);
                return Load(lines);
            }
            catch (Exception exception)
            {
                Log.LogMessageToFile("Character load failed [" + filePath + "]." + exception);
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

        public void SetState(NpcState state)
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
                        case NpcState.Walk:
                        case NpcState.FightWalk:
                        case NpcState.Run:
                        case NpcState.FightRun:
                            {
                                _sound = sound.CreateInstance();
                                _sound.IsLooped = true;
                                SoundManager.Apply3D(_sound,
                                    PositionInWorld - Globals.ListenerPosition);
                                _sound.Play();
                            }
                            break;
                        case NpcState.Magic:
                        case NpcState.Attack:
                        case NpcState.Attack1:
                        case NpcState.Attack2:
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

        #region Perform action
        private void StandingImmediately()
        {
            StateInitialize();
            if (_isInFighting && NpcIni.ContainsKey((int)NpcState.FightStand)) SetState(NpcState.FightStand);
            else
            {
                if (NpcIni.ContainsKey((int)NpcState.Stand1) &&
                    Globals.TheRandom.Next(4) == 1 &&
                    State != (int)NpcState.Stand1) SetState(NpcState.Stand1);
                else SetState(NpcState.Stand);
                PlayCurrentDirOnce();
            }
        }

        public void Standing()
        {
            WalkTo(Vector2.Zero);
        }

        public bool PerformActionOk()
        {
            if (State == (int)NpcState.Jump ||
                State == (int)NpcState.Attack ||
                State == (int)NpcState.Attack1 ||
                State == (int)NpcState.Attack2 ||
                State == (int)NpcState.Magic ||
                State == (int)NpcState.Hurt ||
                State == (int)NpcState.Death ||
                State == (int)NpcState.FightJump) return false;
            return true;
        }

        public void StateInitialize()
        {
            EndPlayCurrentDirOnce();
            DestinationMoveTilePosition = Vector2.Zero;
            DestinationAttackTilePosition = Vector2.Zero;
            Path = null;
        }

        public bool IsStanding()
        {
            return (State == (int)NpcState.Stand ||
                    State == (int)NpcState.Stand1 ||
                    State == (int)NpcState.FightStand);
        }

        public bool IsWalking()
        {
            return (State == (int)NpcState.Walk ||
                    State == (int)NpcState.FightWalk);
        }

        public bool IsRuning()
        {
            return (State == (int)NpcState.Run ||
                    State == (int)NpcState.FightRun);
        }

        public bool IsSitting()
        {
            return State == (int)NpcState.Sit;
        }

        public void WalkTo(Vector2 destinationTilePosition)
        {
            if (PerformActionOk() &&
                destinationTilePosition != TilePosition)
            {
                if (IsWalking())
                    DestinationMoveTilePosition = destinationTilePosition;
                else
                {
                    StateInitialize();
                    Path = Engine.PathFinder.FindPath(TilePosition, destinationTilePosition, PathType);
                    if (Path == null) StandingImmediately();
                    else
                    {
                        DestinationMoveTilePosition = destinationTilePosition;
                        if (_isInFighting && NpcIni.ContainsKey((int)NpcState.FightWalk)) SetState(NpcState.FightWalk);
                        else SetState(NpcState.Walk);
                    }
                }
            }

        }

        public void RunTo(Vector2 destinationTilePosition)
        {
            if (PerformActionOk() &&
                destinationTilePosition != TilePosition)
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
                        if (_isInFighting && NpcIni.ContainsKey((int)NpcState.FightRun)) SetState(NpcState.FightRun);
                        else SetState(NpcState.Run);
                    }
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
                StateInitialize();
                DestinationMoveTilePosition = destinationTilePosition;
                Path = new LinkedList<Vector2>();
                Path.AddLast(PositionInWorld);
                Path.AddLast(DestinationMovePositionInWorld);

                if (_isInFighting && NpcIni.ContainsKey((int)NpcState.FightJump)) SetState(NpcState.FightJump);
                else SetState(NpcState.Jump);
                SetDirection(DestinationMovePositionInWorld - PositionInWorld);
                PlayCurrentDirOnce();
            }
        }

        public void Sitdown()
        {
            if (PerformActionOk())
            {
                StateInitialize();
                if (NpcIni.ContainsKey((int)NpcState.Sit))
                {
                    SetState(NpcState.Sit);
                    PlayCurrentDirOnce();
                }
            }
        }

        public void UseMagic(Magic magicUse, Vector2 magicDestinationPosition)
        {
            if (PerformActionOk())
            {
                StateInitialize();
                _isInFighting = true;
                _totalNonFightingSeconds = 0;

                MagicUse = magicUse;
                _magicDestination = magicDestinationPosition;
                SetState(NpcState.Magic);
                SetDirection(_magicDestination - PositionInWorld);
                PlayCurrentDirOnce();
            }
        }

        public void Hurting()
        {
            if(State != (int)NpcState.Death &&
                State != (int)NpcState.Hurt)
            {
                StateInitialize();
                TilePosition = TilePosition;//To tile center
                if (NpcIni.ContainsKey((int)NpcState.Hurt))
                {
                    SetState(NpcState.Hurt);
                    PlayCurrentDirOnce();
                }
            }
        }

        public void Death()
        {
            if (State == (int)NpcState.Death) return;
            StateInitialize();
            if (NpcIni.ContainsKey((int)NpcState.Death))
            {
                SetState(NpcState.Death);
                PlayCurrentDirOnce();
            }
            else IsDeath = true;
        }

        public void Attacking(Vector2 destinationTilePosition)
        {
            DestinationAttackTilePosition = destinationTilePosition;
            if (IsStanding() && AttackingIsOk())
                PerformeAttack();
        }

        public void ClearAttackingTarget()
        {
            DestinationAttackTilePosition = Vector2.Zero;
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
                    else WalkToAndKeepingAttactTarget(DestinationAttackTilePosition);
                }
                if (tileDistance > AttackRadius)
                {
                    WalkToAndKeepingAttactTarget(DestinationAttackTilePosition);
                }
                else
                {
                    var neighbor = Engine.PathFinder.FindNeighborInDirection(TilePosition,
                        PositionInWorld - DestinationAttackPositionInWorld);

                    if (HasObstacle(neighbor)) return true;

                    Path = Engine.PathFinder.FindPath(TilePosition, neighbor, PathType);
                    if (Path == null) return true;

                    WalkToAndKeepingAttactTarget(neighbor);
                }
            }
            return false;
        }

        protected void WalkToAndKeepingAttactTarget(Vector2 destinationTilePosition)
        {
            var keep = DestinationAttackTilePosition;//keep value
            WalkTo(destinationTilePosition);
            DestinationAttackTilePosition = keep; // restore
        }

        protected void PerformeAttack()
        {
            PerformeAttack(DestinationAttackPositionInWorld);
        }

        public void PerformeAttack(Vector2 destination)
        {
            if (PerformActionOk())
            {
                StateInitialize();
                _isInFighting = true;
                _totalNonFightingSeconds = 0;
                _attackDestination = destination;

                var value = Globals.TheRandom.Next(3);
                if (value == 1 && NpcIni.ContainsKey((int)NpcState.Attack1))
                    SetState(NpcState.Attack1);
                else if (value == 2 && NpcIni.ContainsKey((int)NpcState.Attack2))
                    SetState(NpcState.Attack2);
                else SetState(NpcState.Attack);
                SetDirection(destination - PositionInWorld);
                PlayCurrentDirOnce();
            }
        }

        public void ToNonFightingState()
        {
            _isInFighting = false;
            if (IsWalking()) SetState(NpcState.Walk);
            if (IsRuning()) SetState(NpcState.Run);
            if (State == (int)NpcState.FightStand) SetState(NpcState.Stand);
        }
        #endregion Perform action

        public static float GetTrueDistance(Vector2 distance)
        {
            var unit = Utils.GetDirection8(Utils.GetDirectionIndex(distance, 8));
            distance = (2f - Math.Abs(unit.X)) * distance;
            return distance.Length();
        }

        protected abstract void PlaySoundEffect(SoundEffect soundEffect);

        public override void Update(GameTime gameTime)
        {
            if (IsDeath) return;

            var elapsedGameTime = gameTime.ElapsedGameTime;

            switch ((NpcState)State)
            {
                case NpcState.Walk:
                case NpcState.FightWalk:
                    {
                        MoveAlongPath((float)elapsedGameTime.TotalSeconds, WalkSpeed);
                        SoundManager.Apply3D(_sound,
                                    PositionInWorld - Globals.ListenerPosition);
                        Update(gameTime, WalkSpeed);
                    }
                    break;
                case NpcState.Run:
                case NpcState.FightRun:
                    MoveAlongPath((float)elapsedGameTime.TotalSeconds, 8);
                    SoundManager.Apply3D(_sound,
                                    PositionInWorld - Globals.ListenerPosition);
                    base.Update(gameTime);
                    break;
                case NpcState.Jump:
                case NpcState.FightJump:
                    JumpAlongPath((float)elapsedGameTime.TotalSeconds);
                    base.Update(gameTime);
                    break;
                case NpcState.Stand:
                case NpcState.Stand1:
                case NpcState.Hurt:
                    if (IsPlayCurrentDirOnceEnd()) StandingImmediately();
                    else base.Update(gameTime);
                    break;
                case NpcState.Attack:
                case NpcState.Attack1:
                case NpcState.Attack2:
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
                case NpcState.Magic:
                    if (IsPlayCurrentDirOnceEnd())
                    {
                        if (NpcIni.ContainsKey((int)NpcState.Magic))
                        {
                            PlaySoundEffect(NpcIni[(int)NpcState.Magic].Sound);
                        }
                        MagicManager.UseMagic(this, MagicUse, PositionInWorld, _magicDestination);
                        StandingImmediately();
                    }
                    else base.Update(gameTime);
                    break;
                case NpcState.Sit:
                    if (!IsPlayCurrentDirOnceEnd()) base.Update(gameTime);
                    break;
                case NpcState.Death:
                    if (IsPlayCurrentDirOnceEnd())
                    {
                        IsDeath = true;
                        if (BodyIni != null)
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
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (IsDeath) return;
            base.Draw(spriteBatch);
        }
    }
}
