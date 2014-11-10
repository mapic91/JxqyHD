using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Engine;
using Engine.Gui;
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
        GraphicsDeviceManager _graphics;
        SpriteBatch _spriteBatch;

        public Game()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = false;
            _graphics.IsFullScreen = false;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            Globals.TheGame = this;
            SoundEffect.MasterVolume = Globals.SoundEffectVolume;
            MediaPlayer.Volume = Globals.MusicVolume;
            Log.Initialize();
            Log.DebugOn = false;

            Log.LogMessageToFile("Game is running...");

            _graphics.PreferredBackBufferWidth = Globals.WindowWidth;
            _graphics.PreferredBackBufferHeight = Globals.WindowHeight;
            _graphics.ApplyChanges();

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
            Globals.FontSize7 = Content.Load<SpriteFont>(@"font\ASCII_Verdana_7_Bold");
            Globals.FontSize10 = Content.Load<SpriteFont>(@"font\GB2312_ASCII_ÃÔÄã¼òÏ¸Ô²_10");
            Globals.FontSize12 = Content.Load<SpriteFont>(@"font\GB2312_ASCII_ÃÔÄã¼òÏ¸Ô²_12");

            Globals.TheMap.LoadMap(@"map\map_005_Ï´½£³Ø.map");
            Globals.TheMap.ViewBeginX = 0;
            Globals.TheMap.ViewBeginY = 0;
            Globals.TheCarmera = new Carmera(Globals.TheMap.ViewBeginX, 
                Globals.TheMap.ViewBeginY,
                Globals.TheMap.ViewWidth, 
                Globals.TheMap.ViewHeight,
                Globals.TheMap.MapPixelWidth, 
                Globals.TheMap.MapPixelHeight);

            Globals.ThePlayer = new Player(@"save\rpg2\player0.ini");
            NpcManager.Load(@"save\rpg2\xijianchi.npc");
            ObjManager.Load(@"save\rpg2\map005_obj.obj");
            GuiManager.Starting();
            GuiManager.Load(@"save\rpg2\Magic0.ini", 
                    @"save\rpg2\Goods0.ini",
                    @"save\rpg2\memo.ini");

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
            if(!IsActive) return;

            GuiManager.Update(gameTime);

            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            var mouseState = Mouse.GetState();

            var keyboardState = Keyboard.GetState();
            if(keyboardState.IsKeyDown(Keys.D1) && _lastKeyboardState.IsKeyUp(Keys.D1))
                Globals.TheMap.SwitchLayerDraw(0);
            if (keyboardState.IsKeyDown(Keys.D2) && _lastKeyboardState.IsKeyUp(Keys.D2))
                Globals.TheMap.SwitchLayerDraw(1);
            if (keyboardState.IsKeyDown(Keys.D3) && _lastKeyboardState.IsKeyUp(Keys.D3))
                Globals.TheMap.SwitchLayerDraw(2);

            Globals.ThePlayer.Update(gameTime);
            MagicManager.Update(gameTime);
            NpcManager.Update(gameTime);
            ObjManager.Update(gameTime);

            Globals.TheMap.Update(gameTime);

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

            _spriteBatch.Begin(SpriteSortMode.Deferred,null);
            Globals.TheMap.Draw(_spriteBatch);
            Globals.ThePlayer.Draw(_spriteBatch);
            _spriteBatch.DrawString(Globals.FontSize12, 
                "Ãü£º " + Globals.ThePlayer.Life.ToString(),
                new Vector2(5, 5), Color.Red);
            _spriteBatch.DrawString(Globals.FontSize12,
                "Ìå£º " + Globals.ThePlayer.Thew.ToString(),
                new Vector2(5, 25), Color.Red);
            _spriteBatch.DrawString(Globals.FontSize12,
                "ÄÚ£º " + Globals.ThePlayer.Mana.ToString(),
                new Vector2(5, 45), Color.Red);
            GuiManager.Draw(_spriteBatch);
            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
