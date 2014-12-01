using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Engine;
using Engine.Gui;
using Engine.ListManager;
using Engine.Script;
using Engine.Weather;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

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

        public Game()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
            _graphics.IsFullScreen = false;
            GameState.State = GameState.StateType.Playing;
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
            SoundEffect.MasterVolume = Globals.SoundEffectVolume;
            MediaPlayer.Volume = Globals.MusicVolume;
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

            Globals.TheMap.ViewWidth = _graphics.PreferredBackBufferWidth;
            Globals.TheMap.ViewHeight = _graphics.PreferredBackBufferHeight;

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

            Globals.TheMap.LoadMap("map_005_œ¥Ω£≥ÿ.map");
            Globals.TheMap.ViewBeginX = 0;
            Globals.TheMap.ViewBeginY = 0;
            Globals.TheMap.LoadTrap(@"ini\save\traps.ini");
            Globals.TheCarmera.ViewWidth = Globals.WindowWidth;
            Globals.TheCarmera.ViewHeight = Globals.WindowHeight;

            Globals.ThePlayer = new Player(@"save\rpg2\player0.ini");
            NpcManager.Load(@"xijianchi.npc");
            ObjManager.Load(@"map005_obj.obj");
            GuiManager.Starting();
            GuiManager.Load(@"save\rpg2\Magic0.ini",
                    @"save\rpg2\Goods0.ini",
                    @"save\rpg2\memo.ini");
            GoodsListManager.ApplyEquipSpecialEffectFromList(Globals.ThePlayer);

            //BackgroundMusic.Play(@"music/Mc003.mp3");

            Globals.TheCarmera.Follow(Globals.ThePlayer);
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
            //Pausing game when inactived
            if (!IsActive) return;

            var mouseState = Mouse.GetState();
            var keyboardState = Keyboard.GetState();

            //Fullscreen toggle
            if (keyboardState.IsKeyDown(Keys.LeftAlt) || keyboardState.IsKeyDown(Keys.RightAlt))
            {
                if (keyboardState.IsKeyDown(Keys.Enter) &&
                    _lastKeyboardState.IsKeyUp(Keys.Enter))
                    _graphics.ToggleFullScreen();
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

            //Update script
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
