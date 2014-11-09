using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine.Gui;
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
        private Dictionary<int, Utils.LevelDetail> _levelIni;
        private const float MaxNonFightingTime = 7f;
        private const int ThewUseAmountWhenAttack = 5;
        private const int ThewUseAmountWhenJump = 10;
        private const float ListRestorePercent = 0.01f;
        private const float ThewRestorePercent = 0.03f;
        private float _standingMilliseconds;
        private float _runningMilliseconds;

        private MagicListManager.MagicItemInfo _currentMagicInUse;

        #region Public properties

        public MagicListManager.MagicItemInfo CurrentMagicInUse
        {
            get { return _currentMagicInUse; }
            set
            {
                if (value != null && value.TheMagic != null)
                    _currentMagicInUse = value;
            }
        }

        public override PathFinder.PathType PathType
        {
            get
            {
                if (base.PathFinder == 1)
                    return Engine.PathFinder.PathType.PerfectMaxTry2000;
                return Engine.PathFinder.PathType.PathOneStep;
            }
        }

        public Dictionary<int, Utils.LevelDetail> LevelIni
        {
            get { return _levelIni; }
            set { _levelIni = value; }
        }

        public int Money
        {
            get { return _money; }
            set { _money = value; }
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
        protected override bool HasObstacle(Vector2 tilePosition)
        {
            return (NpcManager.IsObstacle(tilePosition) ||
                        ObjManager.IsObstacle(tilePosition));
        }

        protected override void PlaySoundEffect(SoundEffect soundEffect)
        {
            SoundManager.PlaySoundEffectOnce(soundEffect);
        }

        public void Equiping(Good equip, Good currentEquip)
        {
            UnEquiping(currentEquip);
            if (equip != null)
            {
                Attack += equip.Attack;
                Defend += equip.Defend;
                Evade += equip.Evade;
                LifeMax += equip.LifeMax;
                ThewMax += equip.ThewMax;
                ManaMax += equip.ManaMax;
            }
        }

        public void UnEquiping(Good equip)
        {
            if (equip != null)
            {
                Attack -= equip.Attack;
                Defend -= equip.Defend;
                Evade -= equip.Evade;
                LifeMax -= equip.LifeMax;
                ThewMax -= equip.ThewMax;
                ManaMax -= equip.ManaMax;
            }
        }

        public bool UseDrug(Good drug)
        {
            if (drug != null)
            {
                Life += drug.Life;
                Thew += drug.Thew;
                Mana += drug.Mana;
                return true;
            }
            return false;
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
            if (Mana < MagicUse.ManaCost)
            {
                GuiManager.ShowMessage("没有足够的内力使用这种武功");
                return false;
            }
            else
            {
                Mana -= MagicUse.ManaCost;
                return true;
            }
        }

        protected override bool CanRunning()
        {
            if (Thew > 0)
            {
                Thew -= 1;
                return true;
            }
            return false;
        }

        protected override bool CanJump()
        {
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

        public override void Update(GameTime gameTime)
        {
            var mouseState = Mouse.GetState();
            var keyboardState = Keyboard.GetState();
            var mouseScreenPosition = new Vector2(mouseState.X, mouseState.Y);
            var mouseWorldPosition = Globals.TheCarmera.ToWorldPosition(mouseScreenPosition);
            var mouseTilePosition = Map.ToTilePosition(mouseWorldPosition);

            Globals.ClearGlobalOutEdge();
            if (!GuiManager.IsMouseStateEated)
            {
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
                        else if (one.IsFriend) edgeColor = Globals.FriendEdgeColor;
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
                        if (mouseTilePosition == one.TilePosition)
                        {
                            Globals.OutEdgeSprite = one;
                            Globals.OutEdgeTexture = TextureGenerator.GetOuterEdge(texture, Globals.ObjEdgeColor);
                            break;
                        }
                    }
                }

                if (mouseState.LeftButton == ButtonState.Pressed)
                {
                    if (Globals.OutEdgeNpc != null)
                    {
                        Attacking(Globals.OutEdgeNpc.TilePosition);
                    }
                    else if (keyboardState.IsKeyDown(Keys.LeftShift) ||
                        keyboardState.IsKeyDown(Keys.RightShift))
                        RunTo(mouseTilePosition);
                    else if (keyboardState.IsKeyDown(Keys.LeftAlt) ||
                        keyboardState.IsKeyDown(Keys.RightAlt))
                        JumpTo(mouseTilePosition);
                    else if (keyboardState.IsKeyDown(Keys.LeftControl) ||
                        keyboardState.IsKeyDown(Keys.RightControl))
                        PerformeAttack(mouseWorldPosition);
                    else WalkTo(mouseTilePosition);
                }
                if (mouseState.RightButton == ButtonState.Pressed &&
                    _lastMouseState.RightButton == ButtonState.Released)
                {
                    if (CurrentMagicInUse == null)
                    {
                        GuiManager.ShowMessage("请先在底部武功栏使用鼠标右键选择武功");
                    }
                    else
                    {
                        if (Globals.OutEdgeNpc != null && Globals.OutEdgeNpc.IsEnemy)
                            UseMagic(CurrentMagicInUse.TheMagic, Globals.OutEdgeNpc.PositionInWorld);
                        else UseMagic(CurrentMagicInUse.TheMagic, mouseWorldPosition);
                    }

                }
            }

            if (keyboardState.IsKeyDown(Keys.V) &&
                _lastKeyboardState.IsKeyUp(Keys.V))
            {
                if (IsSitting()) Standing();
                else Sitdown();
            }

            if (IsStanding() || IsWalking())
            {
                _standingMilliseconds += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                if (_standingMilliseconds >= 1000)
                {
                    Life += (int)(ListRestorePercent * LifeMax);
                    Thew += (int)(ThewRestorePercent * ThewMax);
                    _standingMilliseconds = 0f;
                }
            }
            else _standingMilliseconds = 0f;

            _lastMouseState = GuiManager.IsMouseStateEated
                ? Utils.GetMouseState(mouseState.X, mouseState.Y)
                : mouseState;
            _lastKeyboardState = keyboardState;
            base.Update(gameTime);
        }

        public new void Draw(SpriteBatch spriteBatch)
        {
            var texture = GetCurrentTexture();
            if (texture == null) return;

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
                if (npc.MapY > MapY)
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
            Draw(spriteBatch, texture);

            if (Globals.OutEdgeSprite != null)
            {
                Globals.OutEdgeSprite.Draw(spriteBatch,
                    Globals.OutEdgeTexture,
                    Globals.OffX,
                    Globals.OffY);
            }
            if (Globals.OutEdgeNpc != null)
                InfoDrawer.DrawLife(spriteBatch, Globals.OutEdgeNpc);
        }
    }
}
