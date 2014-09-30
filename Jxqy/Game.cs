using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Engine;
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
        private Sprite _testNpc1, _testNpc2;
        private Carmera _cam;
        private Map _map = new Map();
        private Asf _stand, _walk;

        public Game()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            this.IsMouseVisible = true;
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
            GlobalData.TheGame = this;
            Log.DebugOn = true;

            Log.LogMessageToFile("Game is running...\n\n\n");

            _graphics.PreferredBackBufferWidth = 640;
            _graphics.PreferredBackBufferHeight = 480;
            _graphics.ApplyChanges();

            _map.ViewWidth = _graphics.PreferredBackBufferWidth;
            _map.ViewHeight = _graphics.PreferredBackBufferHeight;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _map.LoadMap(this, @"map\map_003_Îäµ±É½ÏÂ.map");
            _map.ViewBeginX = 0;
            _map.ViewBeginY = 0;
            _cam = new Carmera(_map.ViewBeginX, _map.ViewBeginY,_map.ViewWidth, _map.ViewHeight,_map.MapPixelWidth, _map.MapPixelHeight);
            _stand = new Asf(@"asf\character\npc006_st2.asf");
            _walk = new Asf(@"asf\character\npc006_wlk2.asf");
            _testNpc1 = new Sprite(new Vector2(800f), 50, _stand);
            _testNpc2 = new Sprite(new Vector2(80f), 5, new Asf(@"asf\effect\mag038-2-¶¾Òº.asf"));

            BackgroundMusic.Play(@"music/Mc003.mp3");

            _cam.Follow(_testNpc1); 
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
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();

            var mouseState = Mouse.GetState();
            var screenPosition = _cam.ToViewPosition(_testNpc1.PositionInWorld);
            var mousePosition = new Vector2(mouseState.X, mouseState.Y);
            var dir = Vector2.Zero;

            var keyboardState = Keyboard.GetState();
            if(keyboardState.IsKeyDown(Keys.D1) && _lastKeyboardState.IsKeyUp(Keys.D1))
                _map.SwitchLayerDraw(0);
            if (keyboardState.IsKeyDown(Keys.D2) && _lastKeyboardState.IsKeyUp(Keys.D2))
                _map.SwitchLayerDraw(1);
            if (keyboardState.IsKeyDown(Keys.D3) && _lastKeyboardState.IsKeyUp(Keys.D3))
                _map.SwitchLayerDraw(2);

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                if(_lastMouseState.LeftButton == ButtonState.Released)
                    _testNpc1.Texture = _walk;
                dir = mousePosition - screenPosition;
            }
            else if(_lastMouseState.LeftButton == ButtonState.Pressed)
                _testNpc1.Texture = _stand;

            
            _testNpc1.Update(gameTime, dir);
            _testNpc2.Update(gameTime, Vector2.Zero);

            _cam.Update(gameTime);
            _map.ViewBeginX = _cam.ViewBeginX;
            _map.ViewBeginY = _cam.ViewBeginY;

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
            _map.DrawLayer(_spriteBatch, 0);

            var start = _map.GetStartTileInView();
            var end = _map.GetEndTileInView();
            var npc1Position = Map.ToTilePosition(_testNpc1.PositionInWorld, _testNpc1.Texture);
            for (var y = (int)start.Y; y < (int)end.Y; y++)
            {
                for (var x = (int)start.X; x < (int)end.X; x++)
                {
                    Texture2D texture = _map.GetTileTexture(x, y, 1);
                    _map.DrawTile(_spriteBatch, texture, new Vector2(x, y), 1f);
                    if (y == (int) npc1Position.Y && x == (int) npc1Position.X)
                    {
                        _testNpc1.Draw(_spriteBatch, _cam);
                    }
                }
            }
            _testNpc2.Draw(_spriteBatch, _cam);
            _map.DrawLayer(_spriteBatch, 2);
            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
