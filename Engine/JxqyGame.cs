using System;
using System.Linq;
using System.Windows.Forms;
using Engine.Benchmark;
using Engine.Gui;
using Engine.ListManager;
using Engine.Map;
using Engine.Script;
using Engine.Weather;
using IniParser;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Engine
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class JxqyGame : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Effect _waterEffect;
        private Texture2D _waterfallTexture;
        private RenderTarget2D _renderTarget;
        private RenderTarget2D _snapShotRenderTarget;
        private bool _isInSnapShot;

        private double _totalInactivedSeconds;
        private const double MaxInactivedSeconds = 1.0;
        private bool _isInactivedReachMaxInterval;

        #region End Ads

        private float _elapsedSecondsAd;
        private const float AdKeepSeconds = 3;
        private Texture2D _adPic;

        #endregion

        private readonly Color BackgroundColor = Color.Black;

        IntPtr _drawSurface;
        Form _parentForm;
        PictureBox _pictureBox;
        Control _gameForm;

        //Magic kind 23: stop world time
        public MagicSprite TimeStoperMagicSprite;

        public Effect GrayScaleEffect;
        public Effect OutEdgeEffect;
        public Effect TransparentEffect;
        public Effect AlphaTestEffect;

        public bool IsInEditMode { private set; get; }
        public bool IsPaused { get; set; }
        public bool IsGamePlayPaused { get; set; }
        public KeyboardState LastKeyboardState { private set; get; }
        public MouseState LastMouseState { private set; get; }

        public GraphicsDeviceManager Graphics
        {
            get { return _graphics; }
        }

        /// <summary>
        /// Indicates weather game window is lost focus.
        /// Is game is run in edit mode, the value is always flase.
        /// </summary>
        public bool IsFocusLost
        {
            get { return _isInactivedReachMaxInterval; }
        }

        public JxqyGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
            _graphics.IsFullScreen = false;
            _graphics.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;
            GameState.State = GameState.StateType.Start;
        }

        public JxqyGame(IntPtr drawSurface, Form parentForm, PictureBox surfacePictureBox)
            : this()
        {
            IsInEditMode = true;
            _drawSurface = drawSurface;
            _parentForm = parentForm;
            _pictureBox = surfacePictureBox;

            _graphics.PreparingDeviceSettings += graphics_PreparingDeviceSettings;
            Mouse.WindowHandle = drawSurface;

            _gameForm = Control.FromHandle(Window.Handle);
            _gameForm.VisibleChanged += gameForm_VisibleChanged;
            _pictureBox.SizeChanged += pictureBox_SizeChanged;
        }

        public void AdjustDrawSizeToDrawSurfaceSize()
        {
            if (_parentForm.WindowState != FormWindowState.Minimized)
            {
                _graphics.PreferredBackBufferWidth = _pictureBox.Width;
                _graphics.PreferredBackBufferHeight = _pictureBox.Height;
                Globals.WindowWidth =
                    Globals.TheCarmera.ViewWidth =
                    MapBase.ViewWidth =
                    _pictureBox.Width;
                Globals.WindowHeight =
                    Globals.TheCarmera.ViewHeight =
                    MapBase.ViewHeight =
                    _pictureBox.Height;
                _graphics.ApplyChanges();

                ReCreateRenderTarget();

                GuiManager.Adjust(_pictureBox.Width, _pictureBox.Height);
            }
        }

        /// <summary>
        /// If game window inactived total times is exceed max interval, pause the game.
        /// </summary>
        /// <param name="gameTime"></param>
        private void UpdateGameActiveState(GameTime gameTime)
        {
            if (!IsActive)
            {
                _totalInactivedSeconds += gameTime.ElapsedGameTime.TotalSeconds;
            }
            else
            {
                _totalInactivedSeconds = 0.0;
            }

            _isInactivedReachMaxInterval = (_totalInactivedSeconds >= MaxInactivedSeconds);
        }

        private void pictureBox_SizeChanged(object sender, EventArgs e)
        {
            AdjustDrawSizeToDrawSurfaceSize();
        }

        private void gameForm_VisibleChanged(object sender, EventArgs e)
        {
            if (_gameForm.Visible)
            {
                _gameForm.Visible = false;

                //Didn't no why, below solved fps slow than normal
                _parentForm.Visible = false;
                _parentForm.WindowState = FormWindowState.Minimized;
                _parentForm.WindowState = FormWindowState.Normal;
                _parentForm.Visible = true;
            }
        }

        private void graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            e.GraphicsDeviceInformation.PresentationParameters.
                DeviceWindowHandle = _drawSurface;
        }

        #region Utils

        public static void BeginSpriteBatch(SpriteBatch spriteBatch)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred, null);
        }

        public static void BeginSpriteBatch(SpriteBatch spriteBatch, Effect effect)
        {
            spriteBatch.Begin(SpriteSortMode.Deferred,
                null,
                null,
                null,
                null,
                effect);
        }

        private void LoadEffect()
        {
            _waterEffect = Content.Load<Effect>(@"effect\refraction");
            _waterfallTexture = Content.Load<Texture2D>(@"effect\waterfall");

            GrayScaleEffect = Content.Load<Effect>(@"effect\grayscale");
            OutEdgeEffect = Content.Load<Effect>(@"effect\outedge");
            TransparentEffect = Content.Load<Effect>(@"effect\Transparent");
            AlphaTestEffect = Content.Load<Effect>(@"effect\AlphaTest");
        }

        private void WaterEffectBegin()
        {
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.SetRenderTarget(_renderTarget);
        }

        private void WaterEffectEnd(GameTime gameTime)
        {
            _spriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);
            if (_isInSnapShot)
            {
                GraphicsDevice.SetRenderTarget(_snapShotRenderTarget);
            }
            // Set an effect parameter to make the
            // displacement texture scroll in a giant circle.
            _waterEffect.Parameters["DisplacementScroll"].SetValue(
                                            MoveInCircle(gameTime, 0.2f));
            // Set the displacement texture.
            GraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;
            _graphics.GraphicsDevice.Textures[1] = _waterfallTexture;

                _spriteBatch.Begin(SpriteSortMode.Deferred,
                    null,
                    null,
                    null,
                    null,
                    _waterEffect);
                _spriteBatch.Draw(_renderTarget, Vector2.Zero, Color.White);
                _spriteBatch.End();       
            _spriteBatch.Begin(SpriteSortMode.Deferred, null);
        }

        static Vector2 MoveInCircle(GameTime gameTime, float speed)
        {
            double time = gameTime.TotalGameTime.TotalSeconds * speed;

            float x = (float)Math.Cos(time);
            float y = (float)Math.Sin(time);

            return new Vector2(x, y);
        }

        private GameTime GetFold(GameTime gameTime, int fold)
        {
            return new GameTime(
                    new TimeSpan(gameTime.TotalGameTime.Ticks * fold),
                    new TimeSpan(gameTime.ElapsedGameTime.Ticks * fold));
        }

        private void UpdatePlaying(GameTime gameTime)
        {
            if (Globals.IsInSuperMagicMode)
            {
                Globals.SuperModeMagicSprite.Update(gameTime);
                if (Globals.SuperModeMagicSprite.IsDestroyed)
                {
                    Globals.IsInSuperMagicMode = false;
                    Globals.SuperModeMagicSprite = null;
                }
                return;//Just update super magic
            }
            else if (TimeStoperMagicSprite != null)
            {
                TimeStoperMagicSprite.BelongCharacter.Update(gameTime);
                TimeStoperMagicSprite.Update(gameTime);
                MapBase.Instance.Update(gameTime);
                MagicManager.UpdateMagicSpritesInView();
                NpcManager.UpdateNpcsInView();
                ObjManager.UpdateObjsInView();
                return;
            }
            //Player
            Globals.ThePlayer.Update(gameTime);
            //Magic list
            MagicManager.Update(gameTime);
            MagicManager.UpdateMagicSpritesInView();
            //Npc list
            NpcManager.Update(gameTime);
            NpcManager.UpdateNpcsInView();
            //Obj list
            ObjManager.Update(gameTime);
            ObjManager.UpdateObjsInView();
            //Map
            MapBase.Instance.Update(gameTime);
            //Weather
            WeatherManager.Update(gameTime);
        }

        /// <summary>
        /// Draw map npc obj magic etc.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        private void DrawGamePlay(GameTime gameTime)
        {
            if (Globals.IsWaterEffectEnabled)
            {
                WaterEffectBegin();
            }
            //Map npcs objs magic sprite
            MapBase.Instance.Draw(_spriteBatch);
            //Player
            Globals.ThePlayer.Draw(_spriteBatch);
            //Weather
            WeatherManager.Draw(_spriteBatch);
            //Super magic
            if (Globals.IsInSuperMagicMode)
            {
                Globals.SuperModeMagicSprite.Draw(_spriteBatch);
            }
            if (Globals.IsWaterEffectEnabled)
            {
                WaterEffectEnd(gameTime);
            }
            //Fade in, fade out
            if (ScriptExecuter.IsInFadeIn || ScriptExecuter.IsInFadeOut)
            {
                ScriptExecuter.DrawFade(_spriteBatch);
            }
        }

        private void DrawAds(GameTime gameTime)
        {
            _spriteBatch.Draw(_adPic, new Rectangle(0, 0, Globals.WindowWidth, Globals.WindowHeight), Color.White);
        }

        private void DrawGameInfo(SpriteBatch spriteBatch)
        {
            var text = "FPS=" + Fps.FpsValue + "\n" +
                       MapBase.MapFileName + "\n" +
                       NpcManager.FileName + "\n" +
                       ObjManager.FileName + "\n";
            spriteBatch.DrawString(Globals.FontSize12, text, new Vector2(3, 3), Color.White*0.9f );
        }
        #endregion Utils

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            Log.Initialize();
            Globals.TheGame = this;

            if (_graphics.GraphicsDevice.Adapter.IsProfileSupported(GraphicsProfile.HiDef))
            {
                _graphics.GraphicsProfile = GraphicsProfile.HiDef;
                _graphics.ApplyChanges();
            }
            else
            {
                Log.LogMessage("您的显卡不支持高配模式，最大只支持2048x2048的图片，如果TMX地图中存在大于2048x2048的图片，将会显示不正确。");
            }

            Globals.LoadSetting();
            TalkTextList.Initialize();

            Log.LogMessage("Game is running...");

            if (IsInEditMode)
            {
                //Can't full screen in edit mode
                _graphics.IsFullScreen = false;
            }
            else
            {
                _graphics.IsFullScreen = Globals.IsFullScreen;
            }

            _graphics.PreferredBackBufferWidth = Globals.WindowWidth;
            _graphics.PreferredBackBufferHeight = Globals.WindowHeight;
            _graphics.ApplyChanges();

            //Set back in case of width height not work
            Globals.WindowWidth = _graphics.PreferredBackBufferWidth;
            Globals.WindowHeight = _graphics.PreferredBackBufferHeight;

            Globals.TheCarmera.ViewWidth = _graphics.PreferredBackBufferWidth;
            Globals.TheCarmera.ViewHeight = _graphics.PreferredBackBufferHeight;
            MapBase.ViewWidth = _graphics.PreferredBackBufferWidth;
            MapBase.ViewHeight = _graphics.PreferredBackBufferHeight;

            //Game run in editor
            if (_parentForm != null)
            {
                //Make draw size correct
                AdjustDrawSizeToDrawSurfaceSize();
            }

            base.Initialize();
        }

        private void ReCreateRenderTarget()
        {
            _renderTarget = new RenderTarget2D(GraphicsDevice,
                Globals.WindowWidth,
                Globals.WindowHeight,
                false,
                SurfaceFormat.Color,
                DepthFormat.Depth24Stencil8);
            _snapShotRenderTarget = new RenderTarget2D(GraphicsDevice,
                GraphicsDevice.PresentationParameters.BackBufferWidth,
                GraphicsDevice.PresentationParameters.BackBufferHeight,
                false,
                SurfaceFormat.Color,
                DepthFormat.Depth24Stencil8);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            LoadEffect();
            ReCreateRenderTarget();
            Globals.FontSize7 = Content.Load<SpriteFont>(@"font\ASCII_Verdana_7_Bold");
            Globals.FontSize10 = Content.Load<SpriteFont>(@"font\GB2312_ASCII_迷你简细圆_10");
            Globals.FontSize12 = Content.Load<SpriteFont>(@"font\GB2312_ASCII_迷你简细圆_12");

            //Load partner name list
            PartnerList.Load();

            //Load magic exp list
            Utils.LoadMagicExpList();

            //Start gui
            GuiManager.Starting();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            Content.Dispose();
            Utils.ClearTextureCache();
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            //Update mouse scroll state.
            MouseScrollHandler.Update();

            UpdateGameActiveState(gameTime);

            if (IsPaused || IsFocusLost)
            {
                base.Update(gameTime);
                SuppressDraw();
                return;
            }

            var mouseState = Mouse.GetState();
            var keyboardState = Keyboard.GetState();

            //Fullscreen toggle
            if (!IsInEditMode &&
                (keyboardState.IsKeyDown(Keys.LeftAlt) || keyboardState.IsKeyDown(Keys.RightAlt)))
            {
                if (keyboardState.IsKeyDown(Keys.Enter) &&
                    LastKeyboardState.IsKeyUp(Keys.Enter))
                {
                    _graphics.ToggleFullScreen();
                    Globals.IsFullScreen = !Globals.IsFullScreen;
                }
            }

            //Map layer draw toggle
            if (IsInEditMode)
            {
                if (keyboardState.IsKeyDown(Keys.D1) && LastKeyboardState.IsKeyUp(Keys.D1))
                    MapBase.SwitchLayerDraw(0);
                if (keyboardState.IsKeyDown(Keys.D2) && LastKeyboardState.IsKeyUp(Keys.D2))
                    MapBase.SwitchLayerDraw(1);
                if (keyboardState.IsKeyDown(Keys.D3) && LastKeyboardState.IsKeyUp(Keys.D3))
                    MapBase.SwitchLayerDraw(2);
                if (keyboardState.IsKeyDown(Keys.D4) && LastKeyboardState.IsKeyUp(Keys.D4))
                    MapBase.SwitchLayerDraw(3);
                if (keyboardState.IsKeyDown(Keys.D5) && LastKeyboardState.IsKeyUp(Keys.D5))
                    MapBase.SwitchLayerDraw(4);
            }

            if (ScriptExecuter.IsInPlayingMovie)
            {
                //Stop movie when Esc key pressed
                if (keyboardState.IsKeyDown(Keys.Escape) &&
                    LastKeyboardState.IsKeyUp(Keys.Escape))
                {
                    ScriptExecuter.StopMovie();
                }
            }
            else
            {
                //Update GUI first, GUI will decide whether user input be intercepted or pass through
                GuiManager.Update(gameTime);

                switch (GameState.State)
                {
                    case GameState.StateType.Start:
                        ScriptManager.RunScript(Utils.GetScriptParser("title.txt"));
                        GameState.State = GameState.StateType.Title;
                        break;
                    case GameState.StateType.Title:
                        break;
                    case GameState.StateType.Playing:
                        if (IsGamePlayPaused) break;
                        UpdatePlaying(gameTime);
                        break;
                    case GameState.StateType.EndAds:
                        _elapsedSecondsAd += (float)gameTime.ElapsedGameTime.TotalSeconds;
                        if (_elapsedSecondsAd >= AdKeepSeconds ||
                            (keyboardState.IsKeyDown(Keys.Escape) &&
                             LastKeyboardState.IsKeyUp(Keys.Escape)))
                        {
                            Exit();
                        }
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            //Update script after GuiManager, because script executing rely GUI state.
            ScriptManager.Update(gameTime);

            LastKeyboardState = Keyboard.GetState();
            LastMouseState = mouseState;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            //Update fps measurer
            Fps.Update(gameTime);

            GraphicsDevice.Clear(BackgroundColor);
            _spriteBatch.Begin(SpriteSortMode.Deferred, null);
            if (ScriptExecuter.IsInPlayingMovie)//Movie
            {
                ScriptExecuter.DrawVideo(_spriteBatch);
            }
            else
            {
                switch (GameState.State)
                {
                    case GameState.StateType.Start:
                        break;
                    case GameState.StateType.Title:
                        break;
                    case GameState.StateType.Playing:
                        DrawGamePlay(gameTime);
                        break;
                    case GameState.StateType.EndAds:
                        DrawAds(gameTime);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                //GUI
                GuiManager.Draw(_spriteBatch);
            }

            if (IsInEditMode)
            {
                DrawGameInfo(_spriteBatch);
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        /// <summary>
        /// Take snapshot.
        /// </summary>
        /// <returns>Texture of snapshot be taken.</returns>
        public Texture2D TakeSnapShot()
        {
            _isInSnapShot = true;
            GraphicsDevice.SetRenderTarget(_snapShotRenderTarget);
            Draw(new GameTime());
            GraphicsDevice.SetRenderTarget(null);
            _isInSnapShot = false;

            return _snapShotRenderTarget;
        }

        /// <summary>
        /// Not safe if player is followed by enemy or existing enemy magic.
        /// </summary>
        /// <returns>True if safe.Otherwise false.</returns>
        public bool IsSafe()
        {
            var npcs = NpcManager.NpcList;
            if (npcs.Any(npc => npc.IsEnemy &&
                npc.IsFollowTargetFound &&
                npc.FollowTarget != null &&
                (npc.FollowTarget.IsPlayer || npc.FollowTarget.IsPartner)))
            {
                return false;
            }
            var magics = MagicManager.MagicSpritesList;
            if (magics.Any(magicSprite => magicSprite.BelongCharacter.IsEnemy))
            {
                return false;
            }
            var workList = MagicManager.WorkList;
            if (workList.Any(workItem => workItem.TheSprite.BelongCharacter.IsEnemy))
            {
                return false;
            }

            return true;
        }

        public void ExitGame()
        {
            Globals.SaveSetting();
            ToEndAdsScene();
        }

        public void ExitGameImmediately()
        {
            Exit();
        }

        public void ToEndAdsScene()
        {
            GameState.State = GameState.StateType.EndAds;
            GuiManager.IsShow = false;
            var data = new FileIniDataParser().ReadFile("img/show.ini", Globals.LocalEncoding);
            var index = Globals.TheRandom.Next(1, int.Parse(data.Sections["Init"]["Count"])+1);
            if (!data.Sections["Init"].ContainsKey(index.ToString())) index = 1;
            _adPic = Utils.LoadTexture2DFromFile("img/" + data.Sections["Init"][index.ToString()]);
        }
    }
}
