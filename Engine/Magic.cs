using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Engine.ListManager;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

namespace Engine
{
    using StateMapList = Dictionary<int, ResStateInfo>;

    public class Magic
    {
        private string _name;
        private string _intro;
        private int _speed;
        private int _count;
        private int _region;
        private int _moveKind;
        private int _specialKind;
        private int _specialKindValue;
        private int _alphaBlend;
        private int _flyingLum;
        private int _vanishLum;
        private Asf _image;
        private Asf _icon;
        private int _waitFrame;
        private int _lifeFrame;
        private Asf _flyingImage;
        private SoundEffect _flyingSound;
        private Asf _vanishImage;
        private SoundEffect _vanishSound;
        private Asf _superModeImage;
        private int _belong;
        private string _actionFile;
        private Magic _attackFile;
        private Magic _explodeMagicFile;
        private Magic _randMagicFile;
        private int _randMagicProbability;
        private Magic _flyMagic;
        private int _flyInterval;
        private Dictionary<int, Magic> _level;
        private int _currentLevel;
        private int _effectLevel;
        private int _effect;
        private int _effect2;
        private int _effect3;
        private int _effectExt;
        private int _effectMana;
        private int _manaCost;
        private int _thewCost;
        private int _lifeCost;
        private int _levelupExp;
        private bool _isOk;
        private MagicListManager.MagicItemInfo _iteminfo;
        private int _keepMilliseconds;
        private int _maxLevel;
        private int _vibratingScreen;
        private int _maxCount;
        private string _npcFile;
        private int _randomMoveDegree;
        private int _followMouse;
        private int _meteorMove;
        private int _meteorMoveDir = 5;
        private int _moveBack;
        private int _moveImitateUser;
        private int _circleMoveColockwise;
        private int _circleMoveAnticlockwise;
        private int _passThrough;
        private int _passThroughWithDestroyEffect;
        private int _passThroughWall;
        private int _carryUser;
        private int _carryUserSpriteIndex;
        private int _hideUserWhenCarry;
        private int _bounce;
        private int _bounceHurt;
        private int _ball;
        private int _sticky;
        private int _solid;
        private int _noExplodeWhenLifeFrameEnd;
        private int _explodeWhenLifeFrameEnd;
        private int _beginAtMouse;
        private int _beginAtUser;
        private int _beginAtUserAddDirectionOffset;
        private int _beginAtUserAddUserDirectionOffset;
        private int _coldMilliSeconds;
        private int _changeToFriendMilliseconds;

        private int _rangeEffect;
        private int _rangeAddLife;
        private int _rangeAddMana;
        private int _rangeAddThew;
        private int _rangeSpeedUp;
        private int _rangeFreeze;
        private int _rangePoison;
        private int _rangePetrify;
        private int _rangeDamage;
        private int _rangeRadius;
        private int _rangeTimeInerval;
        private int _attackAll;
        private int _discardOppositeMagic;
        private int _exchangeUser;

        private int _disableMoveMilliseconds;
        private int _disableSkillMilliseconds;

        private StateMapList _npcIni;
        private int _attackAddPercent;
        private int _defendAddPercent;
        private int _evadeAddPercent;
        private int _speedAddPercent;

        private int _morphMilliseconds;

        private int _weakMilliseconds;
        private int _weakAttackPercent;
        private int _weakDefendPercent;

        private int _blindMilliseconds;

        private Asf _useActionFile;

        private int _bodyRadius;

        private int _reviveBodyRadius;
        private int _reviveBodyMaxCount;
        private int _reviveBodyLifeMilliSeconds;

        private int _noSpecialKindEffect;

        private Good _goodsName;

        private int _noInterruption;
        private Dictionary<int, List<MagicRegionFileReader.Item>> _regionFile;

        private int _traceEnemy;
        private int _traceSpeed;
        private int _traceEnemyDelayMilliseconds;

        #region Leap
        private int _leapTimes;
        private int _leapFrame;
        private int _effectReducePercentage;
        private Asf _leapImage;
        #endregion Leap

        #region SideEffect
        private int _sideEffectProbability;
        private int _sideEffectPercent;
        private int _sideEffectType;
        #endregion SideEffect

        #region Restore
        private int _restoreProbability;
        private int _restorePercent;
        private int _restoreType;
        #endregion Restore

        private int _dieAfterUse;

        #region Parasitic
        private int _parasitic;
        private Magic _parasiticMagic;
        private int _parasiticInterval = 1000;
        private int _parasiticMaxEffect;
        #endregion Parasitic

        #region Second magic

        private Magic _secondMagicFile;
        private int _secondMagicDelay;
        #endregion Second magic

        #region Public properties
        public AddonEffect AdditionalEffect { set; get; }
        public string Type { set; get; }

        public MagicListManager.MagicItemInfo ItemInfo
        {
            set
            {
                _iteminfo = value;
                if (_explodeMagicFile != null)
                {
                    _explodeMagicFile.ItemInfo = value;
                }
                if (_flyMagic != null)
                {
                    _flyMagic.ItemInfo = value;
                }
                if (_parasiticMagic != null)
                {
                    _parasiticMagic.ItemInfo = value;
                }
            }
            get { return _iteminfo; }
        }

        public Magic ExplodeMagicFile
        {
            get { return _explodeMagicFile; }
            set { _explodeMagicFile = value; }
        }

        public Magic RandMagicFile
        {
            get { return _randMagicFile; }
            set { _randMagicFile = value; }
        }

        public int RandMagicProbability
        {
            get { return _randMagicProbability; }
            set { _randMagicProbability = value; }
        }

        public Magic FlyMagic
        {
            get { return _flyMagic; }
            set { _flyMagic = value; }
        }

        public int FlyInterval
        {
            get { return _flyInterval; }
            set { _flyInterval = value; }
        }

        public string FileName { private set; get; }

        public int CurrentLevel
        {
            get { return _currentLevel; }
            set { _currentLevel = value; }
        }

        public int EffectLevel
        {
            get { return _effectLevel > 0 ? _effectLevel : CurrentLevel; }
            set { _effectLevel = value; }
        }

        public bool IsOk
        {
            get { return _isOk; }
        }

        public string Name
        {
            get { return _name; }
            set { _name = value; }
        }

        public string Intro
        {
            get { return _intro; }
            set { _intro = value; }
        }

        public int Speed
        {
            get { return _speed; }
            set { _speed = value; }
        }

        public int Count
        {
            get { return _count; }
            set { _count = value; }
        }

        public int Region
        {
            get { return _region; }
            set { _region = value; }
        }

        public int MoveKind
        {
            get { return _moveKind; }
            set { _moveKind = value; }
        }

        public int SpecialKind
        {
            get { return _specialKind; }
            set { _specialKind = value; }
        }

        public int SpecialKindValue
        {
            get { return _specialKindValue; }
            set { _specialKindValue = value; }
        }

        public int AlphaBlend
        {
            get { return _alphaBlend; }
            set { _alphaBlend = value; }
        }

        public int FlyingLum
        {
            get { return _flyingLum; }
            set { _flyingLum = value; }
        }

        public int VanishLum
        {
            get { return _vanishLum; }
            set { _vanishLum = value; }
        }

        public Asf Image
        {
            get { return _image; }
            set { _image = value; }
        }

        public Asf Icon
        {
            get { return _icon; }
            set { _icon = value; }
        }

        public int WaitFrame
        {
            get { return _waitFrame; }
            set { _waitFrame = value; }
        }

        public int LifeFrame
        {
            get { return _lifeFrame; }
            set { _lifeFrame = value; }
        }

        public Asf FlyingImage
        {
            get { return _flyingImage; }
            set { _flyingImage = value; }
        }

        public SoundEffect FlyingSound
        {
            get { return _flyingSound; }
            set { _flyingSound = value; }
        }

        public Asf VanishImage
        {
            get { return _vanishImage; }
            set { _vanishImage = value; }
        }

        public Asf SuperModeImage
        {
            get { return _superModeImage; }
            set { _superModeImage = value; }
        }

        public SoundEffect VanishSound
        {
            get { return _vanishSound; }
            set { _vanishSound = value; }
        }

        public int Belong
        {
            get { return _belong; }
            set { _belong = value; }
        }

        public string ActionFile
        {
            get { return _actionFile; }
            set { _actionFile = value; }
        }

        public Magic AttackFile
        {
            get { return _attackFile; }
            set { _attackFile = value; }
        }

        public int Effect
        {
            get { return _effect; }
            set { _effect = value; }
        }

        public int EffectExt
        {
            get { return _effectExt; }
            set { _effectExt = value; }
        }

        public int Effect2
        {
            get { return _effect2; }
            set { _effect2 = value; }
        }

        public int Effect3
        {
            get { return _effect3; }
            set { _effect3 = value; }
        }

        public int EffectMana
        {
            get { return _effectMana; }
            set { _effectMana = value; }
        }

        public int ManaCost
        {
            get { return _manaCost; }
            set { _manaCost = value; }
        }

        public int ThewCost
        {
            get { return _thewCost; }
            set { _thewCost = value; }
        }

        public int LifeCost
        {
            get { return _lifeCost; }
            set { _lifeCost = value; }
        }

        public int LevelupExp
        {
            get { return _levelupExp; }
            set { _levelupExp = value; }
        }

        public int LeapTimes
        {
            get { return _leapTimes; }
            set { _leapTimes = value; }
        }

        public int LeapFrame
        {
            get { return _leapFrame; }
            set { _leapFrame = value; }
        }

        public int EffectReducePercentage
        {
            get { return _effectReducePercentage; }
            set { _effectReducePercentage = value; }
        }

        public Asf LeapImage
        {
            get { return _leapImage; }
            set { _leapImage = value; }
        }

        public int SideEffectType
        {
            get { return _sideEffectType; }
            set
            {
                if (!Enum.IsDefined(typeof (SideEffectDamageType), value))
                {
                    value = 0;
                }
                _sideEffectType = value;
            }
        }

        public int SideEffectPercent
        {
            get { return _sideEffectPercent; }
            set { _sideEffectPercent = value; }
        }

        public int SideEffectProbability
        {
            get { return _sideEffectProbability; }
            set { _sideEffectProbability = value.Clamp(0, 100); }
        }

        public int RestoreType
        {
            get { return _restoreType; }
            set
            {
                if (!Enum.IsDefined(typeof (RestorePropertyType), value))
                {
                    value = 0;
                }
                _restoreType = value;
            }
        }

        public int RestorePercent
        {
            get { return _restorePercent; }
            set { _restorePercent = value; }
        }

        public int RestoreProbability
        {
            get { return _restoreProbability; }
            set { _restoreProbability = value.Clamp(0, 100); }
        }

        public int DieAfterUse
        {
            get { return _dieAfterUse; }
            set { _dieAfterUse = value; }
        }

        public int Parasitic
        {
            get { return _parasitic; }
            set { _parasitic = value; }
        }

        public Magic ParasiticMagic
        {
            get { return _parasiticMagic; }
            set { _parasiticMagic = value; }
        }

        public int ParasiticInterval
        {
            get { return _parasiticInterval; }
            set { _parasiticInterval = value; }
        }

        public int ParasiticMaxEffect
        {
            get { return _parasiticMaxEffect; }
            set { _parasiticMaxEffect = value; }
        }

        public Magic SecondMagicFile
        {
            get { return _secondMagicFile; }
            set { _secondMagicFile = value; }
        }

        public int SecondMagicDelay
        {
            get { return _secondMagicDelay; }
            set { _secondMagicDelay = value; }
        }

        public int KeepMilliseconds
        {
            get { return _keepMilliseconds; }
            set { _keepMilliseconds = value; }
        }

        public int MaxLevel
        {
            get { return _maxLevel; }
            set { _maxLevel = value; }
        }

        public int VibratingScreen
        {
            get { return _vibratingScreen; }
            set { _vibratingScreen = value; }
        }

        public int MaxCount
        {
            get { return _maxCount; }
            set { _maxCount = value; }
        }

        public string NpcFile
        {
            get { return _npcFile; }
            set { _npcFile = value; }
        }

        public int RandomMoveDegree
        {
            get { return _randomMoveDegree; }
            set { _randomMoveDegree = value; }
        }

        public int FollowMouse
        {
            get { return _followMouse; }
            set { _followMouse = value; }
        }

        public int MeteorMove
        {
            get { return _meteorMove; }
            set { _meteorMove = value; }
        }

        public int MeteorMoveDir
        {
            get { return _meteorMoveDir; }
            set { _meteorMoveDir = value; }
        }

        public int MoveBack
        {
            get { return _moveBack; }
            set { _moveBack = value; }
        }

        public int MoveImitateUser
        {
            get { return _moveImitateUser; }
            set { _moveImitateUser = value; }
        }

        public int CircleMoveColockwise
        {
            get { return _circleMoveColockwise; }
            set { _circleMoveColockwise = value; }
        }

        public int CircleMoveAnticlockwise
        {
            get { return _circleMoveAnticlockwise; }
            set { _circleMoveAnticlockwise = value; }
        }

        public int PassThrough
        {
            get { return _passThrough; }
            set { _passThrough = value; }
        }

        public int PassThroughWithDestroyEffect
        {
            get { return _passThroughWithDestroyEffect; }
            set { _passThroughWithDestroyEffect = value; }
        }

        public int PassThroughWall
        {
            get { return _passThroughWall; }
            set { _passThroughWall = value; }
        }

        public int CarryUser
        {
            get { return _carryUser; }
            set { _carryUser = value; }
        }

        public int CarryUserSpriteIndex
        {
            get { return _carryUserSpriteIndex; }
            set { _carryUserSpriteIndex = value; }
        }

        public int HideUserWhenCarry
        {
            get { return _hideUserWhenCarry; }
            set { _hideUserWhenCarry = value; }
        }

        public int Bounce
        {
            get { return _bounce; }
            set { _bounce = value; }
        }

        public int BounceHurt
        {
            get { return _bounceHurt; }
            set { _bounceHurt = value; }
        }

        public int Ball
        {
            get { return _ball; }
            set { _ball = value; }
        }

        public int Sticky
        {
            get { return _sticky; }
            set { _sticky = value; }
        }

        public int Solid
        {
            get { return _solid; }
            set { _solid = value; }
        }

        public int NoExplodeWhenLifeFrameEnd
        {
            get { return _noExplodeWhenLifeFrameEnd; }
            set { _noExplodeWhenLifeFrameEnd = value; }
        }

        public int ExplodeWhenLifeFrameEnd
        {
            get { return _explodeWhenLifeFrameEnd; }
            set { _explodeWhenLifeFrameEnd = value; }
        }

        public int BeginAtMouse
        {
            get { return _beginAtMouse; }
            set { _beginAtMouse = value; }
        }

        public int BeginAtUser
        {
            get { return _beginAtUser; }
            set { _beginAtUser = value; }
        }

        public int BeginAtUserAddDirectionOffset
        {
            get { return _beginAtUserAddDirectionOffset; }
            set { _beginAtUserAddDirectionOffset = value; }
        }

        public int BeginAtUserAddUserDirectionOffset
        {
            get { return _beginAtUserAddUserDirectionOffset; }
            set { _beginAtUserAddUserDirectionOffset = value; }
        }

        public int ColdMilliSeconds
        {
            get { return _coldMilliSeconds; }
            set { _coldMilliSeconds = value; }
        }

        public int ChangeToFriendMilliseconds
        {
            get { return _changeToFriendMilliseconds; }
            set { _changeToFriendMilliseconds = value; }
        }

        public int RangeEffect
        {
            get { return _rangeEffect; }
            set { _rangeEffect = value; }
        }

        public int RangeAddLife
        {
            get { return _rangeAddLife; }
            set { _rangeAddLife = value; }
        }

        public int RangeAddMana
        {
            get { return _rangeAddMana; }
            set { _rangeAddMana = value; }
        }

        public int RangeAddThew
        {
            get { return _rangeAddThew; }
            set { _rangeAddThew = value; }
        }

        public int RangeSpeedUp
        {
            get { return _rangeSpeedUp; }
            set { _rangeSpeedUp = value; }
        }

        public int RangeFreeze
        {
            get { return _rangeFreeze; }
            set { _rangeFreeze = value; }
        }

        public int RangePetrify
        {
            get { return _rangePetrify; }
            set { _rangePetrify = value; }
        }

        public int RangePoison
        {
            get { return _rangePoison; }
            set { _rangePoison = value; }
        }

        public int RangeDamage
        {
            get { return _rangeDamage; }
            set { _rangeDamage = value; }
        }

        public int RangeRadius
        {
            get { return _rangeRadius; }
            set { _rangeRadius = value; }
        }

        public int RangeTimeInerval
        {
            get { return _rangeTimeInerval; }
            set { _rangeTimeInerval = value; }
        }

        public int AttackAll
        {
            get { return _attackAll; }
            set { _attackAll = value; }
        }

        public int DiscardOppositeMagic
        {
            get { return _discardOppositeMagic; }
            set { _discardOppositeMagic = value; }
        }

        public int ExchangeUser
        {
            get { return _exchangeUser; }
            set { _exchangeUser = value; }
        }

        public int DisableMoveMilliseconds
        {
            get { return _disableMoveMilliseconds; }
            set { _disableMoveMilliseconds = value; }
        }

        public int DisableSkillMilliseconds
        {
            get { return _disableSkillMilliseconds; }
            set { _disableSkillMilliseconds = value; }
        }

        public StateMapList NpcIni
        {
            get { return _npcIni; }
            set { _npcIni = value; }
        }

        public int AttackAddPercent
        {
            get { return _attackAddPercent; }
            set { _attackAddPercent = value; }
        }

        public int DefendAddPercent
        {
            get { return _defendAddPercent; }
            set { _defendAddPercent = value; }
        }

        public int EvadeAddPercent
        {
            get { return _evadeAddPercent; }
            set { _evadeAddPercent = value; }
        }

        public int SpeedAddPercent
        {
            get { return _speedAddPercent; }
            set { _speedAddPercent = value; }
        }

        public int MorphMilliseconds
        {
            get { return _morphMilliseconds; }
            set { _morphMilliseconds = value; }
        }

        public int WeakMilliseconds
        {
            get { return _weakMilliseconds; }
            set { _weakMilliseconds = value; }
        }

        public int WeakAttackPercent
        {
            get { return _weakAttackPercent; }
            set
            {
                _weakAttackPercent = value;
                if (_weakAttackPercent > 100)
                {
                    _weakAttackPercent = 100;
                }
            }
        }

        public int WeakDefendPercent
        {
            get { return _weakDefendPercent; }
            set
            {
                _weakDefendPercent = value;
                if (_weakDefendPercent > 100)
                {
                    _weakDefendPercent = 100;
                }
            }
        }

        public int BlindMilliseconds
        {
            get { return _blindMilliseconds; }
            set{ _blindMilliseconds = value; }
        }

        public Asf UseActionFile
        {
            get { return _useActionFile; }
            set { _useActionFile = value; }
        }

        public int BodyRadius
        {
            get { return _bodyRadius; }
            set { _bodyRadius = value; }
        }

        public int ReviveBodyRadius
        {
            get { return _reviveBodyRadius; }
            set { _reviveBodyRadius = value; }
        }

        public int ReviveBodyMaxCount
        {
            get { return _reviveBodyMaxCount; }
            set { _reviveBodyMaxCount = value; }
        }

        public int ReviveBodyLifeMilliSeconds
        {
            get { return _reviveBodyLifeMilliSeconds; }
            set { _reviveBodyLifeMilliSeconds = value; }
        }

        public int NoSpecialKindEffect
        {
            get { return _noSpecialKindEffect; }
            set { _noSpecialKindEffect = value; }
        }

        public Good GoodsName
        {
            get { return _goodsName; }
            set { _goodsName = value; }
        }

        public int NoInterruption
        {
            get { return _noInterruption; }
            set { _noInterruption = value; }
        }

        public Dictionary<int, List<MagicRegionFileReader.Item>> RegionFile
        {
            get { return _regionFile; }
            set { _regionFile = value; }
        }

        public int TraceEnemy
        {
            get { return _traceEnemy; }
            set { _traceEnemy = value; }
        }

        public int TraceSpeed
        {
            get { return _traceSpeed; }
            set { _traceSpeed = value; }
        }

        public int TraceEnemyDelayMilliseconds
        {
            get { return _traceEnemyDelayMilliseconds; }
            set { _traceEnemyDelayMilliseconds = value; }
        }

        #endregion

        //noAttackFile - resolve recursive problem of AttackFile
        public Magic(string filePath, bool noLevel = false, bool noAttackFile = false)
        {
            Load(filePath, noLevel, noAttackFile);
        }

        private void AssignToValue(string[] nameValue, bool noAttackFile)
        {
            try
            {
                var info = this.GetType().GetProperty(nameValue[0]);
                switch (nameValue[0])
                {
                    case "Name":
                    case "Intro":
                    case "ActionFile":
                    case "NpcFile":
                    case "Type":
                        info.SetValue(this, nameValue[1], null);
                        break;
                    case "Image":
                    case "Icon":
                        info.SetValue(this, Utils.GetAsf(@"asf\magic\", nameValue[1]), null);
                        break;
                    case "NpcIni":
                        NpcIni = ResFile.ReadFile(@"ini\npcres\" + nameValue[1], ResType.Npc);
                        break;
                    case "FlyingImage":
                    case "VanishImage":
                    case "SuperModeImage":
                    case "LeapImage":
                        info.SetValue(this, Utils.GetAsf(@"asf\effect\", nameValue[1]), null);
                        break;
                    case "AttackFile":
                        if (File.Exists(@"ini\magic\" + nameValue[1]) && !noAttackFile)
                            info.SetValue(this, new Magic(@"ini\magic\" + nameValue[1], true, true), null);
                        break;
                    case "FlyingSound":
                    case "VanishSound":
                        info.SetValue(this, Utils.GetSoundEffect(nameValue[1]), null);
                        break;
                    case "ExplodeMagicFile":
                    case "FlyMagic":
                    case "ParasiticMagic":
                    case "SecondMagicFile":
                    case "RandMagicFile":
                        info.SetValue(this, Utils.GetMagic(nameValue[1], false), null);
                        break;
                    case "RegionFile":
                        if (!string.IsNullOrEmpty(nameValue[1]))
                        {
                            info.SetValue(this, MagicRegionFileReader.Load(@"ini\magic\" + nameValue[1]), null);
                        }
                        break;
                    case "UseActionFile":
                        if (!string.IsNullOrEmpty(nameValue[1]))
                        {
                            info.SetValue(this, Utils.GetAsf(@"asf\character\", nameValue[1]), null);
                        }
                        break;
                    case "GoodsName":
                        GoodsName = Utils.GetGood(nameValue[1]);
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

        public bool Load(string filePath, bool noLevel = false, bool noAttackFile = false)
        {
            try
            {
                FileName = Path.GetFileName(filePath);
                return Load(File.ReadAllLines(filePath, Globals.LocalEncoding),
                    noLevel, noAttackFile);
            }
            catch (Exception ecxeption)
            {
                Log.LogFileLoadError("Magic", filePath, ecxeption);
                return false;
            }
        }

        public bool Load(string[] lines, bool noLevel = false, bool noAttackFile = false)
        {
            var count = lines.Count();
            var i = 0;
            var hasLevel = false;
            for (; i < count; i++)
            {
                if (Regex.Match(lines[i], @"\[Level[0-9]+\]").Groups[0].Success)
                {
                    hasLevel = true;
                    break;
                }

                var nameValue = Utils.GetNameValue(lines[i]);
                if (!string.IsNullOrEmpty(nameValue[0]))
                    AssignToValue(nameValue, noAttackFile);
            }

            _level = new Dictionary<int, Magic>();
            if (!noLevel)
            {
                for (var li = 0; li < 11; li++)
                {
                    var levelMagic = (Magic)this.MemberwiseClone();
                    levelMagic.CurrentLevel = (li == 0 ? 1 : li);
                    _level.Add(li, levelMagic);
                }

                if (hasLevel)
                {
                    while (i < count)
                    {
                        var groups = Regex.Match(lines[i], @"\[Level([0-9]+)\]").Groups;
                        if (groups[0].Success)
                        {
                            i++;
                            int key;
                            if (int.TryParse(groups[1].Value, out key))
                            {
                                var magic = _level[key];
                                while (i < count && !string.IsNullOrEmpty(lines[i]))
                                {
                                    magic.AssignToValue(Utils.GetNameValue(lines[i]), noAttackFile);
                                    i++;
                                }
                            }
                        }
                        i++;
                    }
                }
            }
            _isOk = true;
            return true;
        }

        public Magic GetLevel(int level)
        {
            if (_level == null ||
                !_level.ContainsKey(level)) return this;
            var magic = _level[level];
            //Assign explode magic level
            if (magic.ExplodeMagicFile != null)
            {
                magic.ExplodeMagicFile = magic.ExplodeMagicFile.GetLevel(level);
            }
            if(magic.RandMagicFile != null)
            {
                magic.RandMagicFile = magic.RandMagicFile.GetLevel(level);
            }
            //Assign FlyMagic level
            if (magic.FlyMagic != null)
            {
                magic.FlyMagic = magic.FlyMagic.GetLevel(level);
            }
            if (magic.ParasiticMagic != null)
            {
                magic.ParasiticMagic = magic.ParasiticMagic.GetLevel(level);
            }
            if (magic.SecondMagicFile != null)
            {
                magic.SecondMagicFile = magic.SecondMagicFile.GetLevel(level);
            }

            magic.AdditionalEffect = AdditionalEffect;
            //Assign item info to level magic
            magic.ItemInfo = ItemInfo;
            return magic;
        }

        public void CopyInfo(Magic magic)
        {
            AdditionalEffect = magic.AdditionalEffect;
            ItemInfo = magic.ItemInfo;
        }

        public enum AddonEffect
        {
            None = 0,
            Frozen,
            Poision,
            Petrified
        }

        public enum SideEffectDamageType
        {
            Life = 0,
            Mana,
            Thew
        }

        public enum RestorePropertyType
        {
            Life = 0,
            Mana,
            Thew
        }
    }
}
