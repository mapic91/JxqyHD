using System;
using System.Text.RegularExpressions;
using Engine.Gui;
using Engine.ListManager;
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
        private float _standingMilliseconds;
        private float _sittedMilliseconds;

        private MagicListManager.MagicItemInfo _currentMagicInUse;
        private MagicListManager.MagicItemInfo _xiuLianMagic;

        private bool _isUseMagicByKeyborad;

        #region Public properties

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
            get { return !Globals.IsInputDisabled && MouseInBound(); }
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

        #region Protected method

        protected override bool MagicFromCache
        {
            get { return false; }
        }

        public override bool HasObstacle(Vector2 tilePosition)
        {
            return (NpcManager.IsObstacle(tilePosition) ||
                        ObjManager.IsObstacle(tilePosition));
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

            Mana -= MagicUse.ManaCost;
            return true;
        }

        protected override bool CanRunning()
        {
            if (IsRunDisabled) return false;
            if (IsNotUseThewWhenRun) return true;
            if (Thew > 0)
            {
                Thew -= 1;
                return true;
            }
            return false;
        }

        protected override bool CanJump()
        {
            if (IsJumpDisabled) return false;
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
            Globals.TheMap.RunTileTrapScript(TilePosition);
        }

        protected override void AssignToValue(KeyData keyData)
        {
            base.AssignToValue(keyData);
            try
            {
                switch (keyData.KeyName)
                {
                    case "Money":
                        Money = int.Parse(keyData.Value);
                        break;
                    case "ManaLimit":
                        ManaLimit = int.Parse(keyData.Value) != 0;
                        break;
                    case "IsRunDisabled":
                        IsRunDisabled = (keyData.Value != "0");
                        break;
                    case "IsJumpDisabled":
                        IsJumpDisabled = (keyData.Value != "0");
                        break;
                    case "IsFightDisabled":
                        IsFightDisabled = (keyData.Value != "0");
                        break;
                }
            }
            catch (Exception)
            {
                //do nothing
                return;
            }
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

        public override void WalkTo(Vector2 destinationTilePosition)
        {
            base.WalkTo(destinationTilePosition);
            if (Path == null)
            {
                NpcManager.PartnersMoveTo(destinationTilePosition);
            }
        }

        public override void RunTo(Vector2 destinationTilePosition)
        {
            base.RunTo(destinationTilePosition);
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
            foreach (var partner in partners)
            {
                partner.SetPosition(neighbors[0]);
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
                    Defend += equip.Defend;
                    Evade += equip.Evade;
                    LifeMax += equip.LifeMax;
                    ThewMax += equip.ThewMax;
                    ManaMax += equip.ManaMax;
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
                    Defend -= equip.Defend;
                    Evade -= equip.Evade;
                    LifeMax -= equip.LifeMax;
                    ThewMax -= equip.ThewMax;
                    ManaMax -= equip.ManaMax;
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
                    GuiManager.ShowMessage("错误");
                }
            }
        }

        public void AddExp(int amount)
        {
            Exp += amount;
            if (Exp > LevelUpExp)
            {
                ToLevel(Exp);
                GuiManager.ShowMessage(Name + "的等级提升了");
            }
        }

        public void AddMagicExp(MagicListManager.MagicItemInfo info, int amount)
        {
            if (info == null)
            {
                //info is null, maybe attack magic(FlyIni,FlyIni2), add exp to current use magic
                info = CurrentMagicInUse;
            }
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

        public override void SetPosition(Vector2 tilePosition)
        {
            base.SetPosition(tilePosition);
            Globals.TheCarmera.CenterPlayerInCamera();
            //Reset parter position relate to player position
            ResetPartnerPosition();
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
        }

        #endregion Public method

        public override void Update(GameTime gameTime)
        {
            var mouseState = Mouse.GetState();
            var keyboardState = Keyboard.GetState();
            var mouseScreenPosition = new Vector2(mouseState.X, mouseState.Y);
            var mouseWorldPosition = Globals.TheCarmera.ToWorldPosition(mouseScreenPosition);
            var mouseTilePosition = Map.ToTilePosition(mouseWorldPosition);
            _isUseMagicByKeyborad = false;

            Globals.ClearGlobalOutEdge();
            if (!GuiManager.IsMouseStateEated && CanInput)
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
                        Globals.OutEdgeTexture = TextureGenerator.GetOuterEdge(texture, edgeColor);
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
                            Globals.OutEdgeTexture = TextureGenerator.GetOuterEdge(texture, Globals.ObjEdgeColor);
                            Globals.OffX = one.OffX;
                            Globals.OffY = one.OffY;
                            break;
                        }
                    }
                }

                if (!IsPetrified)
                {
                    if (mouseState.LeftButton == ButtonState.Pressed)
                    {
                        var isRun = (keyboardState.IsKeyDown(Keys.LeftShift) ||
                                     keyboardState.IsKeyDown(Keys.RightShift)) &&
                                     !IsRunDisabled;

                        if (!IsFightDisabled &&
                            Globals.OutEdgeNpc != null &&
                            Globals.OutEdgeNpc.IsEnemy)
                        {
                            Attacking(Globals.OutEdgeNpc.TilePosition, isRun);
                        }
                        else if (Globals.OutEdgeNpc != null &&
                            Globals.OutEdgeNpc.HasInteractScript)
                        {
                            if (_lastMouseState.LeftButton == ButtonState.Released)
                                InteractWith(Globals.OutEdgeNpc, isRun);
                        }
                        else if (Globals.OutEdgeObj != null &&
                                 Globals.OutEdgeObj.HasInteractScript)
                        {
                            if (_lastMouseState.LeftButton == ButtonState.Released)
                                InteractWith(Globals.OutEdgeObj, isRun);
                        }
                        else if (isRun)
                        {
                            RunTo(mouseTilePosition);
                        }
                        else if (keyboardState.IsKeyDown(Keys.LeftAlt) ||
                                 keyboardState.IsKeyDown(Keys.RightAlt))
                        {
                            JumpTo(mouseTilePosition);
                        }
                        else if (keyboardState.IsKeyDown(Keys.LeftControl) ||
                                 keyboardState.IsKeyDown(Keys.RightControl))
                        {
                            if (!IsFightDisabled)
                            {
                                PerformeAttack(mouseWorldPosition);
                            }
                        }
                        else WalkTo(mouseTilePosition);
                    }
                    if ((!IsFightDisabled &&
                        mouseState.RightButton == ButtonState.Pressed &&
                        _lastMouseState.RightButton == ButtonState.Released) ||
                        _isUseMagicByKeyborad)
                    {
                        if (CurrentMagicInUse == null)
                        {
                            if (!_isUseMagicByKeyborad)
                            {
                                GuiManager.ShowMessage("请在武功栏使用鼠标右键选择武功");
                            }
                        }
                        else
                        {
                            if (Globals.OutEdgeNpc != null && Globals.OutEdgeNpc.IsEnemy)
                                UseMagic(CurrentMagicInUse.TheMagic, Globals.OutEdgeNpc.TilePosition);
                            else UseMagic(CurrentMagicInUse.TheMagic, mouseTilePosition);
                        }

                    }
                }

                if (keyboardState.IsKeyDown(Keys.V) &&
                _lastKeyboardState.IsKeyUp(Keys.V) &&
                !IsPetrified)
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
                    Life += (int)(ListRestorePercent * LifeMax);
                    Thew += (int)(ThewRestorePercent * ThewMax);
                    if (IsManaRestore)
                        Mana += (int)(ManaMax * ManaRestorePercent);
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

        public override void Draw(SpriteBatch spriteBatch)
        {
            var texture = GetCurrentTexture();
            if (texture == null) return;

            //Make new texture is case of texture be locked after draw
            var data = new Color[texture.Width * texture.Height];
            texture.GetData(data);
            texture = new Texture2D(texture.GraphicsDevice, texture.Width, texture.Height);
            texture.SetData(data);

            var tilePosition = new Vector2(MapX, MapY);
            var start = tilePosition - new Vector2(3, 15);
            var end = tilePosition + new Vector2(3, 15);
            if (start.X < 0) start.X = 0;
            if (start.Y < 0) start.Y = 0;
            if (end.X > Globals.TheMap.MapColumnCounts) end.X = Globals.TheMap.MapColumnCounts;
            if (end.Y > Globals.TheMap.MapRowCounts) end.Y = Globals.TheMap.MapRowCounts;
            var textureRegion = new Rectangle();
            var region = RegionInWorld;
            foreach (var npc in NpcManager.NpcsInView)
            {
                if (npc.MapY > MapY && !npc.IsHide)
                    Collider.MakePixelCollidedTransparent(region, texture, npc.RegionInWorld, npc.GetCurrentTexture());
            }
            foreach (var magicSprite in MagicManager.MagicSpritesInView)
            {
                if (magicSprite.MapY >= MapY)
                    Collider.MakePixelCollidedTransparent(region, texture, magicSprite.RegionInWorld, magicSprite.GetCurrentTexture());
            }
            for (var y = (int)start.Y; y < (int)end.Y; y++)
            {
                for (var x = (int)start.X; x < (int)end.X; x++)
                {
                    Texture2D tileTexture;
                    if (y > MapY)
                    {
                        tileTexture = Globals.TheMap.GetTileTextureAndRegion(x, y, 1, ref textureRegion);
                        Collider.MakePixelCollidedTransparent(region, texture, textureRegion, tileTexture);
                    }
                    tileTexture = Globals.TheMap.GetTileTextureAndRegion(x, y, 2, ref textureRegion);
                    Collider.MakePixelCollidedTransparent(region, texture, textureRegion, tileTexture);
                }
            }
            base.Draw(spriteBatch, texture);

            if (Globals.OutEdgeSprite != null &&
                !(Globals.OutEdgeNpc != null && Globals.OutEdgeNpc.IsHide))
            {
                Globals.OutEdgeSprite.Draw(spriteBatch,
                    Globals.OutEdgeTexture,
                    Globals.OffX,
                    Globals.OffY);
            }
            if (Globals.OutEdgeNpc != null &&
                !Globals.OutEdgeNpc.IsHide)
                InfoDrawer.DrawLife(spriteBatch, Globals.OutEdgeNpc);
        }

        public void BuyGood(Good good)
        {
            if (good == null) return;
            var cost = good.Cost;
            if (Money > cost)
            {
                Money -= cost;
                GoodsListManager.AddGoodToList(good.FileName);
                GuiManager.UpdateGoodsView();
            }
            else
            {
                GuiManager.ShowMessage("没有足够的钱！");
            }
        }
    }
}
