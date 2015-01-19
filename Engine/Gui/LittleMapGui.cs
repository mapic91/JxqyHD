using System;
using System.Collections.Generic;
using System.Drawing;
using Engine.Gui.Base;
using IniParser;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Texture = Engine.Gui.Base.Texture;

namespace Engine.Gui
{
    public sealed class LittleMapGui : GuiItem
    {
        private GuiItem _leftButton;
        private GuiItem _rightButton;
        private GuiItem _upButton;
        private GuiItem _downButton;
        private GuiItem _closeButton;
        private LineText _mapName;
        private int _viewBeginX;
        private int _viewBeginY;
        private const int ViewWidth = 320;
        private const int ViewHeight = 240;
        private const int MapViewDrawBeginX = 160;
        private const int MapViewDrawBeginY = 120;
        const int Ratio = 4;
        private Dictionary<string, string> _showNameDictionary;
        private Texture _player;
        private Texture _enemy;
        private Texture _partner;
        private Texture _neutral;

        public override bool IsShow
        {
            get { return base.IsShow; }
            set
            {
                base.IsShow = value;
                if (value)
                {
                    var texture = Globals.TheMap.LittelMapTexture;
                    if (texture == null)
                    {
                        //Little map texture not exist, can't show little map.
                        base.IsShow = false;
                        return;
                    }

                    ViewBeginX = Globals.TheMap.ViewBeginX / Ratio;
                    ViewBeginY = Globals.TheMap.ViewBeginY / Ratio;
                }
            }
        }

        private Rectangle DrawRegion
        {
            get
            {
                return new Rectangle((int)ScreenPosition.X + MapViewDrawBeginX,
                    (int)ScreenPosition.Y + MapViewDrawBeginY,
                    ViewWidth,
                    ViewHeight);
            }
        }

        private Rectangle ViewRegion
        {
            get
            {
                return new Rectangle(_viewBeginX, 
                    _viewBeginY, 
                    ViewWidth, 
                    ViewHeight);
            }
        }

        public int ViewBeginY
        {
            get { return _viewBeginY; }
            set
            {
                var texture = Globals.TheMap.LittelMapTexture;
                if (texture == null)
                {
                    return;
                }

                _viewBeginY = value;
                if (_viewBeginY + ViewHeight > texture.Height)
                {
                    _viewBeginY = texture.Height - ViewHeight;
                }
                if (_viewBeginY < 0)
                {
                    _viewBeginY = 0;
                }
            }
        }

        public int ViewBeginX
        {
            get { return _viewBeginX; }
            set
            {
                var texture = Globals.TheMap.LittelMapTexture;
                if (texture == null)
                {
                    return;
                }

                _viewBeginX = value;
                if (_viewBeginX + ViewWidth > texture.Width)
                {
                    _viewBeginX = texture.Width - ViewWidth;
                }
                if (_viewBeginX < 0)
                {
                    _viewBeginX = 0;
                }
            }
        }

        public LittleMapGui()
        {
            BaseTexture = new Texture(Utils.GetAsf(@"asf\ui\littlemap\", "panel.asf"));
            Width = BaseTexture.Width;
            Height = BaseTexture.Height;
            Position = new Vector2(
                (Globals.WindowWidth - Width) / 2f,
                0f);
            LoadItems();
            RegisterHadler();
            LoadNameList();
            LoadTexture();

            IsShow = false;
        }

        private void RegisterHadler()
        {
            _leftButton.MouseLeftClicking += (arg1, arg2) => ViewBeginX -= 32/Ratio;
            _rightButton.MouseLeftClicking += (arg1, arg2) => ViewBeginX += 32/Ratio;
            _upButton.MouseLeftClicking += (arg1, arg2) => ViewBeginY -= 16/Ratio;
            _downButton.MouseLeftClicking += (arg1, arg2) => ViewBeginY += 16/Ratio;
            _closeButton.Click += (arg1, arg2) => IsShow = false;
        }

        private void LoadTexture()
        {
            _player = new Texture(Utils.GetAsf(@"asf\ui\littlemap\", "主角坐标.asf"));
            _enemy = new Texture(Utils.GetAsf(@"asf\ui\littlemap\", "敌人坐标.asf"));
            _partner = new Texture(Utils.GetAsf(@"asf\ui\littlemap\", "同伴坐标.asf"));
            _neutral = new Texture(Utils.GetAsf(@"asf\ui\littlemap\", "路人坐标.asf"));
        }

        private void LoadNameList()
        {
            const string filePath = @"ini\map\mapname.ini";
            try
            {
                _showNameDictionary = new Dictionary<string, string>();
                var data = new FileIniDataParser().ReadFile(filePath, Globals.SimpleChineseEncoding);
                var section = Utils.GetFirstSection(data);
                foreach (var keyData in section)
                {
                    _showNameDictionary[keyData.KeyName] = keyData.Value;
                }
            }
            catch (Exception exception)
            {
                Log.LogFileLoadError("Map name list", filePath, exception);
            }
        }

        private void LoadItems()
        {
            var sound = Utils.GetSoundEffect("界-浏览.wav");
            var asf = Utils.GetAsf(@"asf\ui\littlemap\", "btnleft.asf");
            _leftButton = new GuiItem(this,
                new Vector2(437, 379),
                asf.Width,
                asf.Height,
                new Texture(asf, 0, 1),
                null,
                new Texture(asf, 1, 1),
                null,
                sound);

            asf = Utils.GetAsf(@"asf\ui\littlemap\", "btnright.asf");
            _rightButton = new GuiItem(this,
                new Vector2(464, 379),
                asf.Width,
                asf.Height,
                new Texture(asf, 0, 1),
                null,
                new Texture(asf, 1, 1),
                null,
                sound);

            asf = Utils.GetAsf(@"asf\ui\littlemap\", "btnup.asf");
            _upButton = new GuiItem(this,
                new Vector2(448, 368),
                asf.Width,
                asf.Height,
                new Texture(asf, 0, 1),
                null,
                new Texture(asf, 1, 1),
                null,
                sound);

            asf = Utils.GetAsf(@"asf\ui\littlemap\", "btndown.asf");
            _downButton = new GuiItem(this,
                new Vector2(448, 395),
                asf.Width,
                asf.Height,
                new Texture(asf, 0, 1),
                null,
                new Texture(asf, 1, 1),
                null,
                sound);

            asf = Utils.GetAsf(@"asf\ui\littlemap\", "btnclose.asf");
            _closeButton = new GuiItem(this,
                new Vector2(448, 379),
                asf.Width,
                asf.Height,
                new Texture(asf, 0, 1),
                null,
                new Texture(asf, 1, 1),
                null,
                sound);

            _mapName = new LineText(this, 
                new Vector2(210, 92),
                220,
                30,
                LineText.Align.Center, 
                string.Empty,
                new Color(76,56,48) * 0.8f,
                Globals.FontSize12);
        }

        private void DrawMapView(SpriteBatch spriteBatch)
        {
            var viewBeginInWorld = new Vector2(_viewBeginX, _viewBeginY)*Ratio;
            var worldRegion = new Rectangle(_viewBeginX*Ratio,
                _viewBeginY*Ratio,
                ViewWidth*Ratio,
                ViewHeight*Ratio);
            var drawRegion = DrawRegion;
            var drawBeginPosition = new Vector2(drawRegion.X, drawRegion.Y);

            if (Globals.TheMap.LittelMapTexture != null)
            {
                spriteBatch.Draw(Globals.TheMap.LittelMapTexture,
                    new Rectangle((int)drawBeginPosition.X, (int)drawBeginPosition.Y, ViewWidth, ViewHeight),
                    new Rectangle(_viewBeginX, _viewBeginY, ViewWidth, ViewHeight),
                    Color.White);
            }

            var npcs = NpcManager.NpcList;
            foreach (var npc in npcs)
            {
                DrawCharacter(spriteBatch, npc, worldRegion, viewBeginInWorld, drawBeginPosition);
            }
            DrawCharacter(spriteBatch, Globals.ThePlayer, worldRegion, viewBeginInWorld, drawBeginPosition);
        }

        private void DrawCharacter(SpriteBatch spriteBatch, 
            Character character, 
            Rectangle worldRegion,
            Vector2 viewBeginInWorld, 
            Vector2 drawBeginPosition)
        {
            if(character == null) return;

            if (worldRegion.Contains((int)character.PositionInWorld.X, (int)character.PositionInWorld.Y))
            {
                var drawPositon = (character.PositionInWorld - viewBeginInWorld)/4 + drawBeginPosition;
                if (character.IsEnemy)
                {
                    _enemy.Draw(spriteBatch, drawPositon);
                }
                else if (character.IsPartner)
                {
                    _partner.Draw(spriteBatch, drawPositon);
                }
                else if (character.IsPlayer)
                {
                    _player.Draw(spriteBatch, drawPositon);
                }
                else if(character.Kind == (int)Character.CharacterKind.Normal ||
                    character.Kind == (int)Character.CharacterKind.Fighter ||
                    character.Kind == (int)Character.CharacterKind.Eventer)
                {
                    _neutral.Draw(spriteBatch, drawPositon);
                }
            }
        }

        public override void Update(GameTime gameTime)
        {
            if(!IsShow) return;
            base.Update(gameTime);
            GuiManager.IsMouseStateEated = true;

            //Buttons
            _leftButton.Update(gameTime);
            _rightButton.Update(gameTime);
            _upButton.Update(gameTime);
            _downButton.Update(gameTime);
            _closeButton.Update(gameTime);

            //Textures
            _player.Update(gameTime);
            _enemy.Update(gameTime);
            _partner.Update(gameTime);
            _neutral.Update(gameTime);

            if (_showNameDictionary.ContainsKey(Globals.TheMap.MapFileNameWithoutExtension))
            {
                _mapName.Text = _showNameDictionary[Globals.TheMap.MapFileNameWithoutExtension];
            }
            else
            {
                _mapName.Text = "无名地图";
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsShow) return;
            DrawMapView(spriteBatch);
            base.Draw(spriteBatch);
            _leftButton.Draw(spriteBatch);
            _rightButton.Draw(spriteBatch);
            _upButton.Draw(spriteBatch);
            _downButton.Draw(spriteBatch);
            _closeButton.Draw(spriteBatch);
            _mapName.Draw(spriteBatch);
        }
    }
}