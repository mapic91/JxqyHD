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
    public abstract class Character
    {
        private const int Basespeed = 100;

        private SoundEffectInstance _sound;
        private float _remainDistance;
        private Sprite _figure = new Sprite();
        private int _dir;
        private string _name;
        private int _kind;
        private int _relation;
        private int _pathFinder;
        private int _state;
        private int _visionRadius;
        private int _dialogRadius;
        private int _attackRadius;
        private int _mapX;
        private int _mapY;
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
        private string _flyIni;
        private string _flyIni2;
        private string _scriptFile;
        private string _deathScript;
        private int _expBonus;
        private int _fixedPos;
        private int _idle;

        #region Public properties
        public int Dir
        {
            get { return _dir; }
            set { _dir = value % 8; }
        }

        public Sprite Figure
        {
            get { return _figure; }
            set { _figure = value; }
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

        public int PathFinder
        {
            get { return _pathFinder; }
            set { _pathFinder = value; }
        }

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

        public int MapX
        {
            get { return (int)Map.ToTilePosition(Figure.PositionInWorld).X; }
            set
            {
                _mapX = value;
                Figure.PositionInWorld = Map.ToPixelPosition(value, MapY);
            }
        }

        public int MapY
        {
            get { return (int)Map.ToTilePosition(Figure.PositionInWorld).Y; }
            set
            {
                _mapY = value;
                Figure.PositionInWorld = Map.ToPixelPosition(MapX, value);
            }
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
            set { _attackLevel = value; }
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
            set { _life = value; }
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
            set { _thew = value; }
        }

        public int ThewMax
        {
            get { return _thewMax; }
            set { _thewMax = value; }
        }

        public int Mana
        {
            get { return _mana; }
            set { _mana = value; }
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

        public string FlyIni
        {
            get { return _flyIni; }
            set { _flyIni = value; }
        }

        public string FlyIni2
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

        public Rectangle RegionInWorld
        {
            get
            {
                var regin = Figure.RegionInWorld;
                regin.Offset(-Figure.Texture.Left, -Figure.Texture.Bottom);
                return regin;
            }
        }

        #endregion

        public Character(string filePath)
        {
            Load(filePath);
        }

        public Character() { }

        private void InitlizeFigure()
        {
            if (NpcIni.ContainsKey((int)NpcState.Stand))
            {
                Figure.Set(Map.ToPixelPosition(MapX, MapY),
                    Basespeed,
                    NpcIni[(int)NpcState.Stand].Image, Dir);
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
                    case "FlyIni":
                    case "FlyIni2":
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
                    default:
                        var integerValue = int.Parse(nameValue[1]);
                        info.SetValue(this, integerValue, null);
                        break;
                }
            }
            catch (Exception)
            {
                //Do nothing
                return;
            }

        }

        public bool Load(string filePath)
        {
            try
            {
                var lines = File.ReadAllLines(filePath, Encoding.GetEncoding(936));
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
            InitlizeFigure();
            return true;
        }

        private List<Vector2> _path;
        public void SetPathAndState(List<Vector2> path, PathType type = PathType.WalkRun, NpcState state = NpcState.Stand)
        {
            if (path != null)
            {
                if (path.Count > 1 && type == PathType.WalkRun)
                {
                    SetState(state);
                    _path = path;
                    _path.RemoveAt(0);
                    var target = _path[0];
                    _remainDistance = Vector2.Distance(target, Figure.PositionInWorld);
                }
                else if (path.Count == 2 && type == PathType.Jump)
                {
                    SetState(state);
                    _path = path;
                    var dir = path[1] - Figure.PositionInWorld;
                    Figure.SetDirection(dir);
                    Figure.PlayCurrentDirOnce();
                }
            }
            if (path == null)
            {
                _path = null;
                SetState(NpcState.Stand);
            }
        }

        public void SetState(NpcState state)
        {
            if (State != (int)state)
            {
                State = (int)state;

                if (_sound != null)
                {
                    _sound.Stop(true);
                    _sound = null;
                }
                if (NpcIni.ContainsKey((int)state))
                {
                    var image = NpcIni[(int) state].Image;
                    var sound = NpcIni[(int) state].Sound;
                    if(image != null)Figure.Texture = NpcIni[(int)state].Image;
                    if (sound != null)
                    {
                        if (State == (int) NpcState.Walk ||
                            State == (int) NpcState.Run)
                        {
                            _sound = sound.CreateInstance();
                            _sound.IsLooped = true;
                            _sound.Play();
                        }
                        else sound.Play();
                    }
                }
            }
        }

        public virtual void Update(GameTime gameTime)
        {
            var speed = 1;
            if (_path != null)
            {
                if (_path.Count != 0 && NpcIni.ContainsKey(State))
                {
                    int speedLevel = 1;
                    switch (State)
                    {
                        case (int)NpcState.Walk:
                            speed = WalkSpeed;
                            speedLevel = WalkSpeed;
                            break;
                        case (int)NpcState.Run:
                        case (int)NpcState.Jump:
                            speedLevel = 8;
                            break;
                    }

                    switch (State)
                    {
                        case (int)NpcState.Walk:
                        case (int)NpcState.Run:
                            {
                                var targetPosition = _path[0];
                                var lastPosition = Figure.PositionInWorld;
                                var dir = targetPosition - lastPosition;
                                Figure.MoveTo(dir, (float)gameTime.ElapsedGameTime.TotalSeconds * speedLevel);
                                _remainDistance -= Vector2.Distance(lastPosition, Figure.PositionInWorld);
                                if (_remainDistance < 1)
                                {
                                    Figure.PositionInWorld = targetPosition;
                                    _path.RemoveAt(0);
                                    if (_path.Count != 0)
                                    {
                                        var newTarget = _path[0];
                                        _remainDistance = Vector2.Distance(newTarget, targetPosition);
                                    }
                                    else SetPathAndState(null);
                                }
                            }
                            break;
                        case (int)NpcState.Jump:
                            {
                                if (_path[1] != Vector2.Zero)
                                {
                                    var targetPosition = _path[1];
                                    var lastPosition = Figure.PositionInWorld;
                                    var dir = targetPosition - lastPosition;
                                    Figure.MoveTo(dir, (float)gameTime.ElapsedGameTime.TotalSeconds * speedLevel);
                                    if (Globals.TheMap.IsObstacleForCharacterJump(Map.ToTilePosition(Figure.PositionInWorld)))
                                    {
                                        Figure.PositionInWorld = lastPosition;
                                        _path[1] = Vector2.Zero;
                                    }
                                    if (Vector2.Distance(targetPosition, Figure.PositionInWorld) < 10)
                                    {
                                        Figure.PositionInWorld = targetPosition;
                                        _path[1] = Vector2.Zero;
                                    }
                                }
                                else
                                {
                                    if(Figure.IsPlayCurrentDirOnceEnd())
                                        SetPathAndState(null);
                                }
                            }
                            break;
                    }
                }
            }

            Figure.Update(gameTime, speed);
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            Figure.Draw(spriteBatch);
        }

        public void Draw(SpriteBatch spriteBatch, Color edgeColor)
        {
            Figure.Draw(spriteBatch, edgeColor);
        }
    }
}
