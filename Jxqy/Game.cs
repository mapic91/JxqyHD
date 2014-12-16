using System;
using System.Windows.Forms;
using Engine;
using Engine.Gui;
using Engine.ListManager;
using Engine.Script;
using Engine.Storage;
using Engine.Weather;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Jxqy
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Effect _waterEffect;
        private Texture2D _waterfallTexture;
        private RenderTarget2D _renderTarget;

        IntPtr _drawSurface;
        Form _parentForm;
        PictureBox _pictureBox;
        Control _gameForm;

        public bool IsPaused { get; set; }

        public Game()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
            _graphics.IsFullScreen = false;
            GameState.State = GameState.StateType.Start;
        }

        public Game(IntPtr drawSurface, Form parentForm, PictureBox surfacePictureBox)
            : this()
        {
            _drawSurface = drawSurface;
            _parentForm = parentForm;
            _pictureBox = surfacePictureBox;

            _graphics.PreparingDeviceSettings += graphics_PreparingDeviceSettings;
            Mouse.WindowHandle = drawSurface;

            _gameForm = Control.FromHandle(Window.Handle);
            _gameForm.VisibleChanged += gameForm_VisibleChanged;
            _pictureBox.SizeChanged += pictureBox_SizeChanged;
        }

        public void SetDrawSizeToDrawSurfaceSize()
        {
            if (_parentForm.WindowState != FormWindowState.Minimized)
            {
                _graphics.PreferredBackBufferWidth = _pictureBox.Width;
                _graphics.PreferredBackBufferHeight = _pictureBox.Height;
                Globals.WindowWidth = 
                    Globals.TheCarmera.ViewWidth = 
                    Globals.TheMap.ViewWidth = 
                    _pictureBox.Width;
                Globals.WindowHeight = 
                    Globals.TheCarmera.ViewHeight = 
                    Globals.TheMap.ViewHeight = 
                    _pictureBox.Height;
                _graphics.ApplyChanges();
            }
        }

        private void pictureBox_SizeChanged(object sender, EventArgs e)
        {
            SetDrawSizeToDrawSurfaceSize();
        }

        private void gameForm_VisibleChanged(object sender, EventArgs e)
        {
            if (_gameForm.Visible)
                _gameForm.Visible = false;
        }

        private void graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            e.GraphicsDeviceInformation.PresentationParameters.
                DeviceWindowHandle = _drawSurface;
        }

        #region Utils
        private void LoadEffect()
        {
            _waterEffect = Content.Load<Effect>(@"effect\refraction");
            _waterfallTexture = Content.Load<Texture2D>(@"effect\waterfall");
        }

        private void WaterEffectBegin()
        {
            GraphicsDevice.SetRenderTarget(_renderTarget);
        }

        private void WaterEffectEnd(GameTime gameTime)
        {
            _spriteBatch.End();
            GraphicsDevice.SetRenderTarget(null);
            // Set an effect parameter to make the
            // displacement texture scroll in a giant circle.
            _waterEffect.Parameters["DisplacementScroll"].SetValue(
                                        MoveInCircle(gameTime, 0.2f));
            // Set the displacement texture.
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
        #endregion Utils

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            Globals.Initialize();
            Globals.TheGame = this;
            TalkTextList.Initialize();
            Log.Initialize();
            Log.LogOn = Globals.IsLogOn;

            Log.LogMessageToFile("Game is running...");

            _graphics.IsFullScreen = Globals.IsFullScreen;
            _graphics.PreferredBackBufferWidth = Globals.WindowWidth;
            _graphics.PreferredBackBufferHeight = Globals.WindowHeight;
            _graphics.ApplyChanges();

            //Set back in case of width height not work
            Globals.WindowWidth = _graphics.PreferredBackBufferWidth;
            Globals.WindowHeight = _graphics.PreferredBackBufferHeight;

            Globals.TheCarmera.ViewWidth = _graphics.PreferredBackBufferWidth;
            Globals.TheCarmera.ViewHeight = _graphics.PreferredBackBufferHeight;
            Globals.TheMap.ViewWidth = _graphics.PreferredBackBufferWidth;
            Globals.TheMap.ViewHeight = _graphics.PreferredBackBufferHeight;

            //Game run in editor
            if (_parentForm != null)
            {
                //Make draw size correct
                SetDrawSizeToDrawSurfaceSize();
            }

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            LoadEffect();
            _renderTarget = new RenderTarget2D(GraphicsDevice,
                Globals.WindowWidth,
                Globals.WindowHeight);
            Globals.FontSize7 = Content.Load<SpriteFont>(@"font\ASCII_Verdana_7_Bold");
            Globals.FontSize10 = Content.Load<SpriteFont>(@"font\GB2312_ASCII_√‘ƒ„ºÚœ∏‘≤_10");
            Globals.FontSize12 = Content.Load<SpriteFont>(@"font\GB2312_ASCII_√‘ƒ„ºÚœ∏‘≤_12");

            GuiManager.Starting();
            Loader.LoadGame();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        private KeyboardState _lastKeyboardState;
        private MouseState _lastMouseState;
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if(IsPaused) return;
            var mouseState = Mouse.GetState();
            var keyboardState = Keyboard.GetState();

            //Fullscreen toggle
            if (keyboardState.IsKeyDown(Keys.LeftAlt) || keyboardState.IsKeyDown(Keys.RightAlt))
            {
                if (keyboardState.IsKeyDown(Keys.Enter) &&
                    _lastKeyboardState.IsKeyUp(Keys.Enter))
                {
                    _graphics.ToggleFullScreen();
                }
            }

            //Map layer draw toggle
            if (keyboardState.IsKeyDown(Keys.D1) && _lastKeyboardState.IsKeyUp(Keys.D1))
                Globals.TheMap.SwitchLayerDraw(0);
            if (keyboardState.IsKeyDown(Keys.D2) && _lastKeyboardState.IsKeyUp(Keys.D2))
                Globals.TheMap.SwitchLayerDraw(1);
            if (keyboardState.IsKeyDown(Keys.D3) && _lastKeyboardState.IsKeyUp(Keys.D3))
                Globals.TheMap.SwitchLayerDraw(2);

            if (ScriptExecuter.IsInPlayingMovie)
            {
                //Stop movie when Esc key pressed
                if (keyboardState.IsKeyDown(Keys.Escape) &&
                    _lastKeyboardState.IsKeyUp(Keys.Escape))
                {
                    ScriptExecuter.StopMovie();
                }
            }
            else
            {
                switch (GameState.State)
                {
                    case GameState.StateType.Start:
                        break;
                    case GameState.StateType.Title:
                        GuiManager.Update(gameTime);
                        break;
                    case GameState.StateType.Playing:
                        if (Globals.IsInSuperMagicMode)
                        {
                            Globals.SuperModeMagicSprite.Update(gameTime);
                            if (Globals.SuperModeMagicSprite.IsDestroyed)
                            {
                                Globals.IsInSuperMagicMode = false;
                                Globals.SuperModeMagicSprite = null;
                            }
                            break;//Just update super magic
                        }
                        GuiManager.Update(gameTime);
                        //Player
                        Globals.ThePlayer.Update(gameTime);
                        //Magic list
                        MagicManager.Update(gameTime);
                        //Npc list
                        NpcManager.Update(gameTime);
                        //Obj list
                        ObjManager.Update(gameTime);
                        //Map
                        Globals.TheMap.Update(gameTime);
                        //Weather
                        WeatherManager.Update(gameTime);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            //Update script after GuiManager, because script executing rely GUI state.
            ScriptManager.Update(gameTime);

            _lastKeyboardState = keyboardState;
            _lastMouseState = mouseState;

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
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
                    case GameState.StateType.Playing:
                        if (Globals.IsWaterEffectEnabled)
                        {
                            WaterEffectBegin();
                        }
                        //Map npcs objs magic sprite
                        Globals.TheMap.Draw(_spriteBatch);
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
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                //Fade in, fade out
                if (ScriptExecuter.IsInFadeIn || ScriptExecuter.IsInFadeOut)
                {
                    ScriptExecuter.DrawFade(_spriteBatch);
                }
                //GUI
                GuiManager.Draw(_spriteBatch);
            }
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
