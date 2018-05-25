using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Engine.Gui;
using Engine.ListManager;
using Engine.Map;
using Engine.Script;
using IniParser.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Engine
{
    public class Player : Character
    {
        private int _doing;
        private int _desX;
        private int _desY;
        private int _belong;
        private int _fight;
        private int _money;
        private const int ThewUseAmountWhenAttack = 5;
        private const int ThewUseAmountWhenJump = 10;
        private const float ListRestorePercent = 0.01f;
        private const float ThewRestorePercent = 0.03f;
        private const float ManaRestorePercent = 0.02f;
        private const int MaxAutoInteractTileDistance = 13;
        private float _standingMilliseconds;
        private float _sittedMilliseconds;
        private bool _isRun;
        /// <summary>
        /// Used to add extra life restore when equiping special equipment
        /// </summary>
        private float _extraLifeRestorePercent;

        /// <summary>
        /// Used to override character's FlyIni when equiping special equipment 
        /// </summary>
        private Magic _flyIniReplace;
        /// <summary>
        ///  Used to override character's FlyIni2 when equiping special equipment 
        /// </summary>
        private Magic _flyIni2Replace;
        

        private MagicListManager.MagicItemInfo _currentMagicInUse;
        private MagicListManager.MagicItemInfo _xiuLianMagic;

        private bool _isUseMagicByKeyborad;

        #region Public properties

        public Magic FlyIniReplace
        {
            get { return _flyIniReplace; }
            set
            {
                if (value != null)
                {
                    RemoveMagicFromInfos(FlyIni, AttackRadius);
                    AddMagicToInfos(value, AttackRadius);
                }
                else
                {
                    RemoveMagicFromInfos(_flyIniReplace, AttackRadius);
                    AddMagicToInfos(FlyIni, AttackRadius);
                }
                _flyIniReplace = value;
            }
        }

        public Magic FlyIni2Replace
        {
            get { return _flyIni2Replace; }
            set
            {
                if (value != null)
                {
                    RemoveMagicFromInfos(FlyIni2, AttackRadius);
                    AddMagicToInfos(value, AttackRadius);
                }
                else
                {
                    RemoveMagicFromInfos(_flyIni2Replace, AttackRadius);
                    AddMagicToInfos(FlyIni2, AttackRadius);
                }
                _flyIni2Replace = value;
            }
        }

        public bool IsNotUseThewWhenRun { set; get; }
        public bool IsManaRestore { set; get; }

        /// <summary>
        /// Can't use mana
        /// </summary>
        public bool ManaLimit { set; get; }

        /// <summary>
        /// Index of npc ini.
        /// For example:
        /// No npcini - 0
        /// z-杨影枫.ini - 1
        /// z-杨影枫2.ini - 2
        /// </summary>
        public int NpcIniIndex { protected set; get; }

        /// <summary>
        /// Texture when use xiulian magic attack magic
        /// </summary>
        public Asf SpecialAttackTexture { protected set; get; }

        /// <summary>
        /// Current use magic index in magic list
        /// </summary>
        public int CurrentUseMagicIndex
        {
            set
            {
                CurrentMagicInUse = MagicListManager.GetItemInfo(value);

            }
            get
            {
                return MagicListManager.GetItemIndex(CurrentMagicInUse);
            }
        }
        public bool CanInput
        {
            get { return !Globals.IsInputDisabled && !ScriptManager.IsInRunningScript && MouseInBound(); }
        }

        public MagicListManager.MagicItemInfo CurrentMagicInUse
        {
            get { return _currentMagicInUse; }
            set
            {
                if (value != null && value.TheMagic != null)
                {
                    _currentMagicInUse = value;
                }
                else
                {
                    _currentMagicInUse = null;
                }
            }
        }

        public MagicListManager.MagicItemInfo XiuLianMagic
        {
            get { return _xiuLianMagic; }
            set
            {
                if (value != null && value.TheMagic == null)
                {
                    //No magic
                    value = null;
                }
                _xiuLianMagic = value;

                //Get character special attack texture
                Asf asf = null;
                if (_xiuLianMagic != null &&
                    _xiuLianMagic.TheMagic.AttackFile != null &&
                    !string.IsNullOrEmpty(_xiuLianMagic.TheMagic.ActionFile))
                {
                    asf = Utils.GetAsf(@"asf\character\",
                        _xiuLianMagic.TheMagic.ActionFile + NpcIniIndex + ".asf");
                }
                SpecialAttackTexture = asf;
            }
        }

        public override PathFinder.PathType PathType
        {
            get
            {
                if (base.PathFinder == 1)
                    return Engine.PathFinder.PathType.PerfectMaxPlayerTry;
                return Engine.PathFinder.PathType.PathOneStep;
            }
        }

        public int Money
        {
            get { return _money; }
            private set
            {
                _money = value;
                if (_money < 0) _money = 0;
            }
        }

        public int Fight
        {
            get { return _fight; }
            set { _fight = value; }
        }

        public int Belong
        {
            get { return _belong; }
            set { _belong = value; }
        }

        public int DesY
        {
            get { return _desY; }
            set { _desY = value; }
        }

        public int DesX
        {
            get { return _desX; }
            set { _desX = value; }
        }

        public int Doing
        {
            get { return _doing; }
            set { _doing = value; }
        }

        public Character ControledCharacter;

        #endregion

        public Player() { }

        public Player(string filePath)
            : base(filePath)
        {

        }

        private MouseState _lastMouseState;
        private KeyboardState _lastKeyboardState;

        private void SetFlyIniAdditionalEffect(Magic.AddonEffect effect)
        {
            if (FlyIni != null) FlyIni.AdditionalEffect = effect;
            if (FlyIni2 != null) FlyIni2.AdditionalEffect = effect;
        }

        private void ToLevel(int exp)
        {
            if (LevelIni != null)
            {
                var count = LevelIni.Count;
                var i = 1;
                for (; i <= count; i++)
                {
                    if (LevelIni.ContainsKey(i))
                    {
                        if (LevelIni[i].LevelUpExp > exp)
                            break;
                    }
                }
                SetLevelTo(i);
            }
        }

        private bool MouseInBound()
        {
            var mouseState = Mouse.GetState();
            var region = new Rectangle(0,
                0,
                Globals.TheGame.GraphicsDevice.PresentationParameters.BackBufferWidth,
                Globals.TheGame.GraphicsDevice.PresentationParameters.BackBufferHeight);
            return region.Contains(mouseState.X, mouseState.Y);
        }

        private void HandleKeyboardInput()
        {
            var state = Keyboard.GetState();
            var lastState = Globals.TheGame.LastKeyboardState;

            if (state.IsKeyDown(Keys.Z) && lastState.IsKeyUp(Keys.Z))
            {
                GuiManager.UsingBottomGood(0);
            }
            if (state.IsKeyDown(Keys.X) && lastState.IsKeyUp(Keys.X))
            {
                GuiManager.UsingBottomGood(1);
            }
            if (state.IsKeyDown(Keys.C) && lastState.IsKeyUp(Keys.C))
            {
                GuiManager.UsingBottomGood(2);
            }

            var index = -1;
            if (state.IsKeyDown(Keys.A) && lastState.IsKeyUp(Keys.A))
            {
                index = 0;
            }
            if (state.IsKeyDown(Keys.S) && lastState.IsKeyUp(Keys.S))
            {
                index = 1;
            }
            if (state.IsKeyDown(Keys.D) && lastState.IsKeyUp(Keys.D))
            {
                index = 2;
            }
            if (state.IsKeyDown(Keys.F) && lastState.IsKeyUp(Keys.F))
            {
                index = 3;
            }
            if (state.IsKeyDown(Keys.G) && lastState.IsKeyUp(Keys.G))
            {
                index = 4;
            }
            if (index != -1)
            {
                var info = GuiManager.GetBottomMagicItemInfo(index);
                if (info != null && info.TheMagic != null)
                {
                    CurrentMagicInUse = info;
                    _isUseMagicByKeyborad = true;
                }
            }
        }

        private void MoveToDirection(int direction)
        {
            var neighbers = Engine.PathFinder.FindNeighborInDirection(TilePosition, direction);
            if (_isRun)
            {
                RunTo(neighbers);
            }
            else
            {
                WalkTo(neighbers);
            }
        }

        private void HandleMoveKeyboardInput()
        {
            var state = Keyboard.GetState();
            if (state.IsKeyDown(Keys.Up))
            {
                MoveToDirection(CurrentDirection);
            }
            else if (state.IsKeyDown(Keys.Left) &&
                _lastKeyboardState.IsKeyUp(Keys.Left))
            {
                SetDirection(CurrentDirection - 1);
            }
            else if (state.IsKeyDown(Keys.Right) &&
                _lastKeyboardState.IsKeyUp(Keys.Right))
            {
                SetDirection(CurrentDirection + 1);
            }
            else if (state.IsKeyDown(Keys.Down) &&
                     _lastKeyboardState.IsKeyUp(Keys.Down))
            {
                SetDirection(CurrentDirection + 4);
            }

            if (state.IsKeyDown(Keys.NumPad2))
            {
                MoveToDirection(0);
            }
            else if (state.IsKeyDown(Keys.NumPad1))
            {
                MoveToDirection(1);
            }
            else if (state.IsKeyDown(Keys.NumPad4))
            {
                MoveToDirection(2);
            }
            else if (state.IsKeyDown(Keys.NumPad7))
            {
                MoveToDirection(3);
            }
            else if (state.IsKeyDown(Keys.NumPad8))
            {
                MoveToDirection(4);
            }
            else if (state.IsKeyDown(Keys.NumPad9))
            {
                MoveToDirection(5);
            }
            else if (state.IsKeyDown(Keys.NumPad6))
            {
                MoveToDirection(6);
            }
            else if (state.IsKeyDown(Keys.NumPad3))
            {
                MoveToDirection(7);
            }
        }

        #region Protected method

        protected override bool IsMagicFromCache
        {
            get { return false; }
        }

        public override bool HasObstacle(Vector2 tilePosition)
        {
            return (NpcManager.IsObstacle(tilePosition) ||
                        ObjManager.IsObstacle(tilePosition) ||
                        MagicManager.IsObstacle(tilePosition));
        }

        protected override void PlaySoundEffect(SoundEffect soundEffect)
        {
            SoundManager.PlaySoundEffectOnce(soundEffect);
        }

        protected override bool CanPerformeAttack()
        {
            if (Thew < ThewUseAmountWhenAttack)
            {
                GuiManager.ShowMessage("体力不足!");
                return false;
            }
            else
            {
                Thew -= ThewUseAmountWhenAttack;
                return true;
            }
        }

        protected override bool CanUseMagic()
        {
            if (MagicUse == null)
            {
                return false;
            }
            if (Mana < MagicUse.ManaCost || ManaLimit)
            {
                GuiManager.ShowMessage("没有足够的内力使用这种武功");
                return false;
            }

            if (MagicUse.GoodsName != null && !string.IsNullOrEmpty(MagicUse.GoodsName.FileName))
            {
                if (!GoodsListManager.DeleteGood(MagicUse.GoodsName.FileName, 1))
                {
                    GuiManager.ShowMessage("缺少物品" + MagicUse.GoodsName.Name);
                    return false;
                }
            }

            Mana -= MagicUse.ManaCost;
            return true;
        }

        protected override void MagicUsedHook(Magic magic)
        {
            if (magic.ItemInfo != null)
            {
                magic.ItemInfo.RemainColdMilliseconds = magic.ColdMilliSeconds;
            }
        }

        protected override bool CanRunning()
        {
            if (CanRun())
            {
                if (!IsNotUseThewWhenRun && (IsInFighting || Globals.IsUseThewWhenNormalRun))
                {
                    Thew -= 1;
                }
                return true;
            }
            return false;
        }

        private bool CanRun()
        {
            if (IsRunDisabled) return false;
            if (IsNotUseThewWhenRun || !IsInFighting) return true;
            if (Thew > 0) return true;
            return false;
        }

        protected override bool CanJump()
        {
            if (IsJumpDisabled ||
                NpcIni == null ||
                !NpcIni.ContainsKey((int)CharacterState.Jump) ||
                NpcIni[(int)CharacterState.Jump].Image == null)
            {
                return false;
            }
            if (Thew < ThewUseAmountWhenJump)
            {
                GuiManager.ShowMessage("体力不足!");
                return false;
            }
            else
            {
                Thew -= ThewUseAmountWhenJump;
                return true;
            }
        }


        /// <summary>
        /// Check whether current tile in map has trap, if has run it
        /// </summary>
        protected override void CheckMapTrap()
        {
            MapBase.Instance.RunTileTrapScript(TilePosition);
        }

        protected override bool CheckMapTrapByPath(LinkedList<Vector2> pixelPositionPathList, out Vector2 trapTilePosition)
        {
            if (pixelPositionPathList != null)
            {
                foreach (var pixelPostiion in pixelPositionPathList)
                {
                    var tilePosition = MapBase.ToTilePosition(pixelPostiion);
                    if (MapBase.Instance.HasTrapScript(tilePosition))
                    {
                        trapTilePosition = tilePosition;
                        return true;
                    }
                }
            }

            trapTilePosition = Vector2.Zero;
            return false;
        }

        protected override void AssignToValue(KeyData keyData)
        {
            try
            {
                switch (keyData.KeyName)
                {
                    case "Money":
                        Money = int.Parse(keyData.Value);
                        return;
                    case "ManaLimit":
                        ManaLimit = int.Parse(keyData.Value) != 0;
                        return;
                    case "IsRunDisabled":
                        IsRunDisabled = (keyData.Value != "0");
                        return;
                    case "IsJumpDisabled":
                        IsJumpDisabled = (keyData.Value != "0");
                        return;
                    case "IsFightDisabled":
                        IsFightDisabled = (keyData.Value != "0");
                        return;
                }
            }
            catch (Exception)
            {
                //do nothing
                return;
            }
            base.AssignToValue(keyData);
        }

        protected override void OnPerformeAttack()
        {
            if (SpecialAttackTexture != null &&
                State == (int)CharacterState.Attack2)
            {
                Texture = SpecialAttackTexture;
            }
        }

        protected override void OnAttacking(Vector2 attackDestinationPixelPosition)
        {
            if (State == (int)CharacterState.Attack2 &&
                XiuLianMagic != null &&
                XiuLianMagic.TheMagic != null &&
                XiuLianMagic.TheMagic.AttackFile != null)
            {
                MagicManager.UseMagic(this,
                    XiuLianMagic.TheMagic.AttackFile,
                    PositionInWorld,
                    attackDestinationPixelPosition);
            }
        }

        protected override void OnSitDown()
        {
            _sittedMilliseconds = 0;
        }

        #endregion Protected method

        #region Public method
        public override void Save(KeyDataCollection keyDataCollection)
        {
            base.Save(keyDataCollection);
            AddKey(keyDataCollection, "Money", Money);
            AddKey(keyDataCollection, "CurrentUseMagicIndex", CurrentUseMagicIndex);
            AddKey(keyDataCollection, "LevelIni", LevelIniFile);
            AddKey(keyDataCollection, "ManaLimit", ManaLimit);
            AddKey(keyDataCollection, "IsRunDisabled", IsRunDisabled);
            AddKey(keyDataCollection, "IsJumpDisabled", IsJumpDisabled);
            AddKey(keyDataCollection, "IsFightDisabled", IsFightDisabled);
        }

        public override void SetMagicFile(string fileName)
        {
            FlyIni = Utils.GetMagic(fileName, false);
        }

        public override void WalkTo(Vector2 destinationTilePosition, PathFinder.PathType pathType = Engine.PathFinder.PathType.End)
        {
            base.WalkTo(destinationTilePosition, pathType);
            if (Path == null)
            {
                NpcManager.PartnersMoveTo(destinationTilePosition);
            }
        }

        public override void RunTo(Vector2 destinationTilePosition, PathFinder.PathType pathType = Engine.PathFinder.PathType.End)
        {
            base.RunTo(destinationTilePosition, pathType);
            if (Path == null)
            {
                NpcManager.PartnersMoveTo(destinationTilePosition);
            }
        }

        public void ResetPartnerPosition()
        {
            var partners = NpcManager.GetAllPartner();
            if (partners.Count == 0) return;
            var neighbors = Engine.PathFinder.FindAllNeighbors(TilePosition);
            var index = CurrentDirection + 4;
            foreach (var partner in partners)
            {
                if (index == CurrentDirection) index++;
                partner.SetPosition(neighbors[index%8]);
                index++;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="equip"></param>
        /// <param name="currentEquip"></param>
        /// <param name="justEffectType">Don't applay Attack, Defend,Evade,LifeMax,ThewMax,ManaMax, just equip effect</param>
        public void Equiping(Good equip, Good currentEquip, bool justEffectType = false)
        {
            //Save for restore
            var life = Life;
            var thew = Thew;
            var mana = Mana;

            UnEquiping(currentEquip, justEffectType);
            if (equip != null)
            {
                if (!justEffectType)
                {
                    Attack += equip.Attack;
                    Attack2 += equip.Attack2;
                    Defend += equip.Defend;
                    Defend2 += equip.Defend2;
                    Evade += equip.Evade;
                    LifeMax += equip.LifeMax;
                    ThewMax += equip.ThewMax;
                    ManaMax += equip.ManaMax;

                    if (!string.IsNullOrEmpty(equip.MagicIniWhenUse))
                    {
                        if (MagicListManager.IsMagicHided(equip.MagicIniWhenUse))
                        {
                            var info = MagicListManager.SetMagicHide(equip.MagicIniWhenUse, false);
                            if (info != null)
                            {
                                GuiManager.ShowMessage("武功" + info.TheMagic.Name + "已可使用");
                                GuiManager.UpdateMagicView();
                            }
                            else
                            {
                                GuiManager.ShowMessage("Good-MagicIniWhenUse错误");
                            }
                        }
                        else
                        {
                            AddMagic(equip.MagicIniWhenUse);
                        }
                    }
                }

                switch (equip.TheEffectType)
                {
                    case Good.GoodEffectType.ThewNotLoseWhenRun:
                        IsNotUseThewWhenRun = true;
                        break;
                    case Good.GoodEffectType.ManaRestore:
                        IsManaRestore = true;
                        break;
                    case Good.GoodEffectType.EnemyFrozen:
                        SetFlyIniAdditionalEffect(Magic.AddonEffect.Frozen);
                        break;
                    case Good.GoodEffectType.EnemyPoisoned:
                        SetFlyIniAdditionalEffect(Magic.AddonEffect.Poision);
                        break;
                    case Good.GoodEffectType.EnemyPetrified:
                        SetFlyIniAdditionalEffect(Magic.AddonEffect.Petrified);
                        break;
                }

                switch (equip.SpecialEffect)
                {
                    case 1://不断恢复生命
                        _extraLifeRestorePercent = equip.SpecialEffectValue/100.0f;
                        break;
                }

                if (!string.IsNullOrEmpty(equip.FlyIni))
                {
                    FlyIniReplace = Utils.GetMagic(equip.FlyIni, IsMagicFromCache);
                }
                if (!string.IsNullOrEmpty(equip.FlyIni2))
                {
                    FlyIni2Replace = Utils.GetMagic(equip.FlyIni2, IsMagicFromCache);
                }

                if (!string.IsNullOrEmpty(equip.AddMagicEffectName))
                {
                    if (!AddMagicEffectWithName.ContainsKey(equip.AddMagicEffectName))
                    {
                        AddMagicEffectWithName[equip.AddMagicEffectName] = new Dictionary<string, AddmagicEffectInfo>();
                    }
                    AddMagicEffectWithName[equip.AddMagicEffectName][equip.Name] = new AddmagicEffectInfo(equip.AddMagicEffectPercent, equip.AddMagicEffectAmount);
                }
                else if (!string.IsNullOrEmpty(equip.AddMagicEffectType))
                {
                    if (!AddMagicEffectWithType.ContainsKey(equip.AddMagicEffectType))
                    {
                        AddMagicEffectWithType[equip.AddMagicEffectType] = new Dictionary<string, AddmagicEffectInfo>();
                    }
                    AddMagicEffectWithType[equip.AddMagicEffectType][equip.Name] = new AddmagicEffectInfo(equip.AddMagicEffectPercent, equip.AddMagicEffectAmount);
                }
                else
                {
                    AddMagicEffectPercent += equip.AddMagicEffectPercent;
                    AddMagicEffectAmount += equip.AddMagicEffectAmount;
                }

                ChangeMoveSpeedPercent += equip.ChangeMoveSpeedPercent;
            }

            //Restore
            Life = life;
            Thew = thew;
            Mana = mana;
        }

        public void UnEquiping(Good equip, bool justEffectType = false)
        {
            if (equip != null)
            {
                if (!justEffectType)
                {
                    Attack -= equip.Attack;
                    Attack2 -= equip.Attack2;
                    Defend -= equip.Defend;
                    Defend2 -= equip.Defend2;
                    Evade -= equip.Evade;
                    LifeMax -= equip.LifeMax;
                    ThewMax -= equip.ThewMax;
                    ManaMax -= equip.ManaMax;

                    if (!string.IsNullOrEmpty(equip.MagicIniWhenUse))
                    {
                        var info = MagicListManager.SetMagicHide(equip.MagicIniWhenUse, true);
                        if (info != null)
                        {
                            GuiManager.ShowMessage("武功" + info.TheMagic.Name + "已不可使用");

                            OnDeleteMagic(info);

                            GuiManager.UpdateMagicView();
                        }
                        else
                        {
                            GuiManager.ShowMessage("Good-MagicIniWhenUse错误");
                        }
                    }
                }
                switch (equip.TheEffectType)
                {
                    case Good.GoodEffectType.ThewNotLoseWhenRun:
                        IsNotUseThewWhenRun = false;
                        break;
                    case Good.GoodEffectType.ManaRestore:
                        IsManaRestore = false;
                        break;
                    case Good.GoodEffectType.EnemyFrozen:
                    case Good.GoodEffectType.EnemyPoisoned:
                    case Good.GoodEffectType.EnemyPetrified:
                        SetFlyIniAdditionalEffect(Magic.AddonEffect.None);
                        break;
                }

                switch (equip.SpecialEffect)
                {
                    case 1://不断恢复生命
                        _extraLifeRestorePercent = 0.0f;
                        break;
                }

                if (!string.IsNullOrEmpty(equip.FlyIni))
                {
                    FlyIniReplace = null;
                }
                if (!string.IsNullOrEmpty(equip.FlyIni2))
                {
                    FlyIni2Replace = null;
                }

                if (!string.IsNullOrEmpty(equip.AddMagicEffectName))
                {
                    AddMagicEffectWithName[equip.AddMagicEffectName].Remove(equip.Name);
                    if (AddMagicEffectWithName[equip.AddMagicEffectName].Count == 0)
                    {
                        AddMagicEffectWithName.Remove(equip.AddMagicEffectName);
                    }
                }
                else if (!string.IsNullOrEmpty(equip.AddMagicEffectType))
                {
                    AddMagicEffectWithType[equip.AddMagicEffectType].Remove(equip.Name);
                    if (AddMagicEffectWithType[equip.AddMagicEffectType].Count == 0)
                    {
                        AddMagicEffectWithType.Remove(equip.AddMagicEffectType);
                    }
                }
                else
                {
                    AddMagicEffectPercent -= equip.AddMagicEffectPercent;
                    AddMagicEffectAmount -= equip.AddMagicEffectAmount;
                }

                ChangeMoveSpeedPercent -= equip.ChangeMoveSpeedPercent;
            }
        }

        public void OnDeleteMagic(MagicListManager.MagicItemInfo info)
        {
            if (info == null || info.TheMagic == null) return;

            if (XiuLianMagic != null &&
                Utils.EqualNoCase(XiuLianMagic.TheMagic.Name, info.TheMagic.Name))
            {
                XiuLianMagic = null;
            }

            if (CurrentMagicInUse != null &&
                Utils.EqualNoCase(CurrentMagicInUse.TheMagic.Name, info.TheMagic.Name))
            {
                CurrentMagicInUse = null;
            }
        }

        public bool UseDrug(Good drug)
        {
            if (drug != null && drug.Kind == Good.GoodKind.Drug)
            {
                LifeMax += drug.LifeMax;
                ThewMax += drug.ThewMax;
                ManaMax += drug.ManaMax;
                Life += drug.Life;
                Thew += drug.Thew;
                Mana += drug.Mana;
                switch (drug.TheEffectType)
                {
                    case Good.GoodEffectType.ClearFrozen:
                        ClearFrozen();
                        break;
                    case Good.GoodEffectType.ClearPoison:
                        ClearPoison();
                        break;
                    case Good.GoodEffectType.ClearPetrifaction:
                        ClearPetrifaction();
                        break;
                }
                return true;
            }
            return false;
        }

        public void AddMoney(int money)
        {
            if (money > 0)
            {
                Money += money;
                GuiManager.ShowMessage("你得到了 " + money + " 两银子。");
            }
            else if (money < 0)
            {
                Money += money;
                money = -money;
                GuiManager.ShowMessage("你失去了 " + money + " 两银子。");
            }
        }

        /// <summary>
        /// Just add money amount do nothing else.
        /// </summary>
        /// <param name="amount">Amount to add</param>
        public void AddMoneyValue(int amount)
        {
            Money += amount;
        }

        public void SetMoney(int amount)
        {
            Money = amount;
        }

        public int GetMoneyAmount()
        {
            return Money;
        }

        public override void AddMagic(string magicFileName)
        {
            if (string.IsNullOrEmpty(magicFileName)) return;

            int index;
            Magic magic;
            var result = MagicListManager.AddMagicToList(
                magicFileName,
                out index,
                out magic);
            if (result)
            {
                GuiManager.ShowMessage("你学会了" + magic.Name);
                GuiManager.UpdateMagicView();
            }
            else
            {
                if (magic != null)
                {
                    GuiManager.ShowMessage("你已经学会了" + magic.Name);
                }
                else
                {
                    GuiManager.ShowMessage("武功栏已满");
                }
            }
        }

        public void AddExp(int amount, bool addMagicExp = false)
        {
            if (addMagicExp)
            {
                if (XiuLianMagic != null)
                {
                    AddMagicExp(XiuLianMagic, (int) (amount*Utils.XiuLianMagicExpFraction));
                }
                if (CurrentMagicInUse != null)
                {
                    AddMagicExp(CurrentMagicInUse, (int) (amount*Utils.UseMagicExpFraction));
                }
            }

            if(LevelUpExp <= 0) return;
            Exp += amount;
            if (Exp > LevelUpExp)
            {
                GuiManager.ShowMessage(Name + "的等级提升了");
                ToLevel(Exp);
            }
        }

        public void AddMagicExp(MagicListManager.MagicItemInfo info, int amount)
        {
            if (info == null ||
                info.TheMagic == null ||
                info.TheMagic.LevelupExp == 0 //Max level
                )
            {
                return;
            }
            info.Exp += amount;
            var levelupExp = info.TheMagic.LevelupExp;
            if (info.Exp >= levelupExp)
            {
                info.TheMagic = info.TheMagic.GetLevel(info.TheMagic.CurrentLevel + 1);
                if (info.TheMagic.LevelupExp == 0)
                {
                    //Magic is max level, make exp equal max exp
                    info.Exp = levelupExp;
                }
                GuiManager.ShowMessage("武功 " + info.TheMagic.Name + " 升级了");
            }
        }

        /// <summary>
        /// To next level
        /// </summary>
        public void LevelUp()
        {
            if (LevelUpExp == 0)
            {
                //Can't level up
                return;
            }
            AddExp(LevelUpExp - Exp + 1);
        }

        private static readonly Regex NpcIniIndexRegx = new Regex(@".*([0-9]+)\.ini");
        public override void SetNpcIni(string fileName)
        {
            base.SetNpcIni(fileName);

            var match = NpcIniIndexRegx.Match(fileName);
            int value;
            if (match.Success &&
                int.TryParse(match.Groups[1].Value, out value))
            {
                NpcIniIndex = value;
            }
            else
            {
                NpcIniIndex = 1;
            }
            //Renew xiulian magic
            XiuLianMagic = XiuLianMagic;
        }

        public override void Death()
        {
            base.Death();
            Globals.IsInputDisabled = true;
        }

        public override void FullLife()
        {
            if (IsDeath)
            {
                Globals.IsInputDisabled = false;
            }
            base.FullLife();
        }

        public override void SetLevelTo(int level)
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
                Attack2 += (detail.Attack2 - currentDetail.Attack2);
                Defend += (detail.Defend - currentDetail.Defend);
                Defend2 += (detail.Defend2 - currentDetail.Defend2);
                Evade += (detail.Evade - currentDetail.Evade);
                LevelUpExp = detail.LevelUpExp;
                if (!string.IsNullOrEmpty(detail.NewMagic))
                {
                    AddMagic(detail.NewMagic);
                }
                if (!string.IsNullOrEmpty(detail.NewGood))
                {
                    ScriptExecuter.AddGoods(detail.NewGood);
                }
            }
            else
            {
                Exp = 0;
                LevelUpExp = 0;
            }

            Level = level;
        }

        public void EndControlCharacter()
        {
            if (ControledCharacter != null)
            {
                //Clear others follow target, because controled character Releation value change
                NpcManager.CleartFollowTargetIfEqual(ControledCharacter);

                ControledCharacter.ControledMagicSprite = null;
                ControledCharacter = null;
            }
        }

        public bool canRun(KeyboardState keyboardState)
        {
            return (keyboardState.IsKeyDown(Keys.LeftShift) ||
                      keyboardState.IsKeyDown(Keys.RightShift)) &&
                      !IsRunDisabled;;
        }

        #endregion Public method

        public override void Update(GameTime gameTime)
        {
            var mouseState = Mouse.GetState();
            var keyboardState = Keyboard.GetState();
            var mouseScreenPosition = new Vector2(mouseState.X, mouseState.Y);
            var mouseWorldPosition = Globals.TheCarmera.ToWorldPosition(mouseScreenPosition);
            var mouseTilePosition = MapBase.ToTilePosition(mouseWorldPosition);
            _isUseMagicByKeyborad = false;

            _isRun = canRun(keyboardState);

            Globals.ClearGlobalOutEdge();

            if (Kind == (int)CharacterKind.Follower)
            {
                //If player kind is changed to follower, follow the player kind npc.
                PartnerMoveTo(Globals.PlayerTilePosition);
            }
            else if (!GuiManager.IsMouseStateEated && CanInput)
            {
                HandleKeyboardInput();

                foreach (var one in NpcManager.NpcsInView)
                {
                    if (!one.IsInteractive) continue;
                    var texture = one.GetCurrentTexture();
                    if (Collider.IsPixelCollideForNpcObj(mouseWorldPosition,
                        one.RegionInWorld,
                        texture))
                    {
                        Globals.OutEdgeNpc = one;
                        Globals.OutEdgeSprite = one;
                        var edgeColor = Globals.NpcEdgeColor;
                        if (one.IsEnemy) edgeColor = Globals.EnemyEdgeColor;
                        else if (one.IsFighterFriend) edgeColor = Globals.FriendEdgeColor;
                        Globals.OutEdgeColor = edgeColor;
                        break;
                    }
                }
                if (Globals.OutEdgeSprite == null) //not finded, try obj
                {
                    foreach (var one in ObjManager.ObjsInView)
                    {
                        if (!one.IsInteractive) continue;
                        var texture = one.GetCurrentTexture();
                        if (mouseTilePosition == one.TilePosition ||
                            Collider.IsPixelCollideForNpcObj(mouseWorldPosition,
                            one.RegionInWorld,
                            texture))
                        {
                            Globals.OutEdgeObj = one;
                            Globals.OutEdgeSprite = one;
                            Globals.OutEdgeColor = Globals.ObjEdgeColor;
                            Globals.OffX = one.OffX;
                            Globals.OffY = one.OffY;
                            break;
                        }
                    }
                }


                var character = this as Character;
                if (ControledCharacter != null)
                {
                    character = ControledCharacter;
                }
                if (!IsPetrified)
                {
                    if (mouseState.LeftButton == ButtonState.Pressed)
                    {
                        if (!IsFightDisabled &&
                            Globals.OutEdgeNpc != null &&
                            Globals.OutEdgeNpc.IsEnemy)
                        {
                            character.Attacking(Globals.OutEdgeNpc.TilePosition, _isRun);
                        }
                        else if (Globals.OutEdgeNpc != null &&
                            Globals.OutEdgeNpc != ControledCharacter &&
                            Globals.OutEdgeNpc.HasInteractScript)
                        {
                            if (_lastMouseState.LeftButton == ButtonState.Released)
                                character.InteractWith(Globals.OutEdgeNpc, _isRun);
                        }
                        else if (Globals.OutEdgeObj != null &&
                                 Globals.OutEdgeObj.HasInteractScript)
                        {
                            if (_lastMouseState.LeftButton == ButtonState.Released)
                                character.InteractWith(Globals.OutEdgeObj, _isRun);
                        }
                        else if (_isRun)
                        {
                            if (CanRun())
                            {
                                character.RunTo(mouseTilePosition);
                            }
                            else
                            {
                                character.WalkTo(mouseTilePosition);
                            }
                        }
                        else if (keyboardState.IsKeyDown(Keys.LeftAlt) ||
                                 keyboardState.IsKeyDown(Keys.RightAlt))
                        {
                            character.JumpTo(mouseTilePosition);
                        }
                        else if (keyboardState.IsKeyDown(Keys.LeftControl) ||
                                 keyboardState.IsKeyDown(Keys.RightControl))
                        {
                            if (!IsFightDisabled)
                            {
                                character.PerformeAttack(mouseWorldPosition, GetRamdomMagicWithUseDistance(AttackRadius));
                            }
                        }
                        else character.WalkTo(mouseTilePosition);
                    }
                    else
                    {
                        if (ControledCharacter == null)
                        {
                            HandleMoveKeyboardInput();
                        }

                        if (Globals.TheGame.LastKeyboardState.IsKeyUp(Keys.Q) && keyboardState.IsKeyDown(Keys.Q))
                        {
                            var closestObj =
                                ObjManager.GetClosestCanInteractObj(character.TilePosition,
                                    MaxAutoInteractTileDistance);
                            if (closestObj != null)
                            {
                                character.InteractWith(closestObj, _isRun);
                            }
                        }
                        else if (Globals.TheGame.LastKeyboardState.IsKeyUp(Keys.E) && keyboardState.IsKeyDown(Keys.E))
                        {
                            var closestNpc =
                                NpcManager.GetClosestCanInteractChracter(character.TilePosition,
                                    MaxAutoInteractTileDistance);
                            if (closestNpc != null)
                            {
                                character.InteractWith(closestNpc, _isRun);
                            }
                        }
                    }
                    var rightButtonPressed = mouseState.RightButton == ButtonState.Pressed;
                    if (!IsFightDisabled &&
                        ControledCharacter == null && //Can't use magic when controling other character
                        (rightButtonPressed || _isUseMagicByKeyborad)
                       )
                    {
                        if (CurrentMagicInUse == null)
                        {
                            if (!_isUseMagicByKeyborad)
                            {
                                GuiManager.ShowMessage("请在武功栏使用鼠标右键选择武功");
                            }
                        }
                        else if (CurrentMagicInUse.RemainColdMilliseconds > 0)
                        {
                            GuiManager.ShowMessage("武功尚未冷却");
                        }
                        else
                        {
                            if(PerformActionOk() && !AttackClosedAnemy(character))
                            {
                                if (CurrentMagicInUse.TheMagic.BodyRadius > 0 &&
                                    (Globals.OutEdgeNpc == null || !Globals.OutEdgeNpc.IsEnemy))
                                {
                                    GuiManager.ShowMessage("无有效目标");
                                }
                                else if (CurrentMagicInUse.TheMagic.MoveKind == 21 && Globals.OutEdgeNpc == null)
                                {
                                    GuiManager.ShowMessage("无目标");
                                }
                                else
                                {
                                    if (Globals.OutEdgeNpc != null)
                                        UseMagic(CurrentMagicInUse.TheMagic, Globals.OutEdgeNpc.TilePosition, Globals.OutEdgeNpc);
                                    else UseMagic(CurrentMagicInUse.TheMagic, mouseTilePosition);
                                }
                            }
                        }

                    }
                }

                if (keyboardState.IsKeyDown(Keys.V) &&
                _lastKeyboardState.IsKeyUp(Keys.V) &&
                !IsPetrified &&
                ControledCharacter == null)
                {
                    if (IsSitting()) StandingImmediately();
                    else Sitdown();
                }
            }

            if ((IsStanding() || IsWalking()) && BodyFunctionWell)
            {
                _standingMilliseconds += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                if (_standingMilliseconds >= 1000)
                {
                    Life += (int)((ListRestorePercent + _extraLifeRestorePercent) * LifeMax);
                    Thew += (int)(ThewRestorePercent * ThewMax);
                    if (IsManaRestore)
                    {
                        Mana += (int)(ManaMax * ManaRestorePercent);
                    }
                    _standingMilliseconds = 0f;
                }
            }
            else _standingMilliseconds = 0f;

            if (IsSitted)
            {
                int changeManaAmount = ManaMax / 100;
                if (changeManaAmount == 0) changeManaAmount = 1;
                if (Mana < ManaMax && Thew > changeManaAmount)
                {
                    _sittedMilliseconds += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    const float changeManaIntervel = 150f;
                    if (_sittedMilliseconds >= changeManaIntervel)
                    {
                        _sittedMilliseconds -= changeManaIntervel;
                        Thew -= changeManaAmount;
                        Mana += changeManaAmount;
                    }
                }
                else
                {
                    StandingImmediately();
                }
            }

            _lastMouseState = mouseState;
            _lastKeyboardState = keyboardState;
            base.Update(gameTime);
        }

        private bool AttackClosedAnemy(Character attacker)
        {
            var keyboardState = Keyboard.GetState();
            if (keyboardState.IsKeyDown(Keys.LeftControl) ||
                keyboardState.IsKeyDown(Keys.RightControl))
            {
                var closedEnemy = NpcManager.GetClosestEnemy(attacker, PositionInWorld);
                if (closedEnemy != null)
                {
                    UseMagic(CurrentMagicInUse.TheMagic, closedEnemy.TilePosition, closedEnemy);
                    return true;
                }
            }
            return false;
        }

        private static readonly BlendState NoWriteColorBlendState = new BlendState()
        {
            ColorWriteChannels = ColorWriteChannels.None,
        };

        private static readonly DepthStencilState StencilStateMask = new DepthStencilState()
        {
            StencilEnable = true,
            StencilFunction = CompareFunction.Always,
            StencilPass = StencilOperation.Replace,
            ReferenceStencil = 1,
            //DepthBufferEnable = false,
        };

        private static readonly DepthStencilState StencilStateDrawOpaque = new DepthStencilState()
        {
            StencilEnable = true,
            StencilFunction = CompareFunction.Equal,
            ReferenceStencil = 0,
            //DepthBufferEnable = false,
        };

        private static readonly DepthStencilState StencilStateDrawHalfTransparent = new DepthStencilState()
        {
            StencilEnable = true,
            StencilFunction = CompareFunction.Equal,
            ReferenceStencil = 1,
        };

        public override void Draw(SpriteBatch spriteBatch)
        {
            var texture = GetCurrentTexture();
            if (texture == null) return;

            if (IsDraw)
            {
                var tilePosition = new Vector2(MapX, MapY);
                var start = tilePosition - new Vector2(3, 15);
                var end = tilePosition + new Vector2(3, 15);
                if (start.X < 0) start.X = 0;
                if (start.Y < 0) start.Y = 0;
                if (end.X > MapBase.Instance.MapColumnCounts) end.X = MapBase.Instance.MapColumnCounts;
                if (end.Y > MapBase.Instance.MapRowCounts) end.Y = MapBase.Instance.MapRowCounts;
                var textureWorldRegion = new Rectangle();
                var region = RegionInWorld;
                const int maxSamplerTextures = 10;
                int currentCount = 0;

                //Enable stencile
                spriteBatch.End();
                var alphaTestEffect = Globals.TheGame.AlphaTestEffect;
                alphaTestEffect.Parameters["MinAlpha"].SetValue(0.0f);
                spriteBatch.Begin(SpriteSortMode.Immediate, NoWriteColorBlendState, null, StencilStateMask, null, alphaTestEffect);

                for (var y = (int)start.Y; y < (int)end.Y; y++)
                {
                    for (var x = (int)start.X; x < (int)end.X; x++)
                    {
                        Texture2D tileTexture;
                        Nullable<Rectangle> sourceRegion;
                        if (y > MapY)
                        {
                            tileTexture = MapBase.Instance.GetTileTextureAndRegionInWorld(x, y, 1, out sourceRegion, ref textureWorldRegion);
                            if (tileTexture != null && Collider.IsBoxCollide(region, textureWorldRegion))
                            {
                                MapBase.Instance.DrawTile(spriteBatch, Color.White, 1, new Vector2(x, y));
                                currentCount++;
                            }
                        }
                        tileTexture = MapBase.Instance.GetTileTextureAndRegionInWorld(x, y, 2, out sourceRegion, ref textureWorldRegion);
                        if (tileTexture != null && Collider.IsBoxCollide(region, textureWorldRegion))
                        {
                            MapBase.Instance.DrawTile(spriteBatch, Color.White, 2, new Vector2(x, y));
                            currentCount++;
                        }
                    }
                }
                foreach (var npc in NpcManager.NpcsInView)
                {
                    if (currentCount >= maxSamplerTextures) break;

                    if (npc.MapY > MapY && !npc.IsHide)
                    {
                        if (Collider.IsBoxCollide(region, npc.RegionInWorld))
                        {
                            npc.Draw(spriteBatch, Color.White);
                            currentCount++;
                        }
                    }
                }
                foreach (var magicSprite in MagicManager.MagicSpritesInView)
                {
                    if (currentCount >= maxSamplerTextures) break;
                    if (magicSprite.MapY >= MapY)
                    {
                        if (Collider.IsBoxCollide(region, magicSprite.RegionInWorld))
                        {
                            magicSprite.Draw(spriteBatch, Color.White);
                            currentCount++;
                        }
                    }
                }
                spriteBatch.End();

                if (currentCount > 0)
                {
                    var drawColor = DrawColor;
                    var useGrayScale = DrawColor == Color.Black;
                    if (FrozenSeconds > 0 && IsFronzenVisualEffect)
                        drawColor = new Color(80, 80, 255);
                    if (PoisonSeconds > 0 && IsPoisionVisualEffect)
                        drawColor = new Color(50, 255, 50);
                    if (PetrifiedSeconds > 0 && IsPetrifiedVisualEffect)
                    {
                        useGrayScale = true;
                    }
                    spriteBatch.Begin(SpriteSortMode.Immediate, null, null, StencilStateDrawOpaque, null, useGrayScale ? Globals.TheGame.GrayScaleEffect : null);
                    base.Draw(spriteBatch, texture, useGrayScale ? Color.White : drawColor);
                    spriteBatch.End();
                    var halfTransparentEffect = Globals.TheGame.TransparentEffect;
                    halfTransparentEffect.Parameters["alpha"].SetValue(0.5f);
                    halfTransparentEffect.Parameters["useGrayScale"].SetValue(useGrayScale ? 1 : 0);
                    spriteBatch.Begin(SpriteSortMode.Immediate, null, null, StencilStateDrawHalfTransparent, null, halfTransparentEffect);
                    base.Draw(spriteBatch, texture, useGrayScale ? Color.White : DrawColor);
                    spriteBatch.End();

                    Globals.TheGame.GraphicsDevice.Clear(ClearOptions.Stencil, Color.Black, 0, 0);
                    JxqyGame.BeginSpriteBatch(spriteBatch);
                    DrawRangeRadius(spriteBatch);
                }
                else
                {
                    JxqyGame.BeginSpriteBatch(spriteBatch);
                    base.Draw(spriteBatch, texture);
                }
            }

            if (Globals.OutEdgeSprite != null &&
                !(Globals.OutEdgeNpc != null && Globals.OutEdgeNpc.IsHide))
            {
                var ct = Globals.OutEdgeSprite.GetCurrentTexture();
                if (ct != null)
                {
                    spriteBatch.End();
                    Globals.TheGame.OutEdgeEffect.Parameters["EdgeColor"].SetValue(new Vector4(
                        Globals.OutEdgeColor.R/255.0f, 
                        Globals.OutEdgeColor.G/255.0f,
                        Globals.OutEdgeColor.B/255.0f,
                        Globals.OutEdgeColor.A/255.0f));
                    Globals.TheGame.OutEdgeEffect.Parameters["radiusx"].SetValue(1.0f / ct.Width);
                    Globals.TheGame.OutEdgeEffect.Parameters["radiusy"].SetValue(1.0f/ ct.Height);
                    JxqyGame.BeginSpriteBatch(spriteBatch, Globals.TheGame.OutEdgeEffect);
                    Globals.OutEdgeSprite.Draw(spriteBatch,
                        ct,
                        Color.White,
                        Globals.OffX,
                        Globals.OffY);
                    spriteBatch.End();
                    JxqyGame.BeginSpriteBatch(spriteBatch);
                }
            }
            if (Globals.OutEdgeNpc != null &&
                !Globals.OutEdgeNpc.IsHide)
                InfoDrawer.DrawLife(spriteBatch, Globals.OutEdgeNpc);
        }

        public bool BuyGood(Good good)
        {
            if (good == null) return false;
            var cost = good.Cost;
            if (Money >= cost)
            {
                if (GoodsListManager.AddGoodToList(good.FileName))
                {
                    Money -= cost;
                    GuiManager.UpdateGoodsView();
                    return true;
                }
            }
            else
            {
                GuiManager.ShowMessage("没有足够的钱！");
            }
            return false;
        }
    }
}
