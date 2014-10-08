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
        private Asf _stand, _walk;
        private Player _player1;
        private NpcManager _npcManager;

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
            Globals.TheGame = this;
            Log.DebugOn = true;

            Log.LogMessageToFile("Game is running...\n\n\n");

            _graphics.PreferredBackBufferWidth = 1366;
            _graphics.PreferredBackBufferHeight = 768;
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

            Globals.TheMap.LoadMap(@"map\map_012_ª›∞≤’Ú.map");
            Globals.TheMap.ViewBeginX = 0;
            Globals.TheMap.ViewBeginY = 0;
            Globals.TheCarmera = new Carmera(Globals.TheMap.ViewBeginX, 
                Globals.TheMap.ViewBeginY,
                Globals.TheMap.ViewWidth, 
                Globals.TheMap.ViewHeight,
                Globals.TheMap.MapPixelWidth, 
                Globals.TheMap.MapPixelHeight);
            _stand = new Asf(@"asf\character\npc006_st2.asf");
            _walk = new Asf(@"asf\character\npc006_wlk2.asf");
            _testNpc1 = new Sprite(new Vector2(800f), 50, _stand);
            _testNpc2 = new Sprite(new Vector2(80f), 5, new Asf(@"asf\effect\mag038-2-∂æ“∫.asf"));
            _player1 = new Player(@"ini\save\player0.ini");
            _npcManager = new NpcManager(@"ini\save\huianzhen.npc");

            BackgroundMusic.Play(@"music/Mc003.mp3");

            Globals.TheCarmera.Follow(_player1.Figure); 
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
            var screenPosition = Globals.TheCarmera.ToViewPosition(_testNpc1.PositionInWorld);
            var mouseScreenPosition = new Vector2(mouseState.X, mouseState.Y);
            var mouseWorldPosition = Globals.TheCarmera.ToWorldPosition(mouseScreenPosition);
            var dir = Vector2.Zero;

            var keyboardState = Keyboard.GetState();
            if(keyboardState.IsKeyDown(Keys.D1) && _lastKeyboardState.IsKeyUp(Keys.D1))
                Globals.TheMap.SwitchLayerDraw(0);
            if (keyboardState.IsKeyDown(Keys.D2) && _lastKeyboardState.IsKeyUp(Keys.D2))
                Globals.TheMap.SwitchLayerDraw(1);
            if (keyboardState.IsKeyDown(Keys.D3) && _lastKeyboardState.IsKeyUp(Keys.D3))
                Globals.TheMap.SwitchLayerDraw(2);

            _player1.Update(gameTime);
            _testNpc1.Update(gameTime, dir);
            _testNpc2.Update(gameTime, Vector2.Zero);
            _npcManager.Update(gameTime);


            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                var tilePositionUnderMouse = Map.ToTilePosition(mouseWorldPosition);
                var startTile = Map.ToTilePosition(_player1.Figure.PositionInWorld);
                var path = PathFinder.FindPath(startTile, tilePositionUnderMouse);
                if (keyboardState.IsKeyDown(Keys.LeftShift))
                    _player1.SetPath(path, NpcState.Run);
                else _player1.SetPath(path, NpcState.Walk);
            }
            //else if (_lastMouseState.LeftButton == ButtonState.Pressed)
            //    _testNpc1.Texture = _stand;

            Globals.TheCarmera.Update(gameTime);
            Globals.TheMap.ViewBeginX = Globals.TheCarmera.ViewBeginX;
            Globals.TheMap.ViewBeginY = Globals.TheCarmera.ViewBeginY;

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
            Globals.TheMap.DrawLayer(_spriteBatch, 0);

            var start = Globals.TheMap.GetStartTileInView();
            var end = Globals.TheMap.GetEndTileInView();
            var npc1Position = Map.ToTilePosition(_testNpc1.PositionInWorld);
            for (var y = (int)start.Y; y < (int)end.Y; y++)
            {
                for (var x = (int)start.X; x < (int)end.X; x++)
                {
                    Texture2D texture = Globals.TheMap.GetTileTexture(x, y, 1);
                    Globals.TheMap.DrawTile(_spriteBatch, texture, new Vector2(x, y), 1f);
                    if (y == (int) npc1Position.Y && x == (int) npc1Position.X)
                    {
                        _testNpc1.Draw(_spriteBatch);
                    }
                }
            }
            _testNpc2.Draw(_spriteBatch);
            _npcManager.Draw(_spriteBatch);
            _player1.Draw(_spriteBatch);
            Globals.TheMap.DrawLayer(_spriteBatch, 2);
            _spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
