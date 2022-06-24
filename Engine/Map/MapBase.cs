using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Engine.Script;
using Engine.Weather;
using IniParser;
using IniParser.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Map
{
    public abstract class MapBase
    {
        public static MapBase Instance;

        protected int _mapColumnCounts;
        protected int _mapRowCounts;
        protected int _mapPixelWidth;
        protected int _mapPixelHeight;

        public const int MaxLayer = 5;
        /// <summary>
        /// layer1, layer2, layer3, trap, obstacle
        /// 0,      1,      2,      3,      4
        /// </summary>
        protected static readonly bool[] _isLayerDraw = new bool[MaxLayer] { true, true, true, false, false };

        protected static Color _drawColor = Color.White;
        protected static Color _rainDrawColor = Color.White;

        //traps
        protected static readonly Dictionary<string, Dictionary<int, string>> _traps = new Dictionary<string, Dictionary<int, string>>();
        protected static readonly List<int> _ingnoredTrapsIndex = new List<int>();
        protected static bool _isInRunMapTrap;
        protected static ScriptRunner _currentRunTrapScript;

        protected static int _viewBeginX;
        protected static int _viewBeginY;
        protected static int _viewWidth;
        protected static int _viewHeight;

        protected static string _mapFileNameWithoutExtension;
        protected static string _mapFileName;

        #region Public Properties

        public bool IsOk { protected set; get; }

        public static string MapFileNameWithoutExtension
        {
            get { return _mapFileNameWithoutExtension; }
            protected set { _mapFileNameWithoutExtension = value; }
        }

        public static string MapFileName
        {
            get { return _mapFileName; }
            protected set { _mapFileName = value; }
        }

        public static int MapTime;

        public static Texture2D LittelMapTexture { set; get; }

        public static int ViewHeight
        {
            get { return _viewHeight; }
            set { _viewHeight = value < 0 ? 0 : value; }
        }

        public static int ViewWidth
        {
            get { return _viewWidth; }
            set { _viewWidth = value < 0 ? 0 : value; }
        }

        public int ViewBeginX
        {
            get { return _viewBeginX; }
            set
            {
                if (value <= 0) _viewBeginX = 0;
                else if (value + ViewWidth > _mapPixelWidth) _viewBeginX = _mapPixelWidth - ViewWidth;
                else _viewBeginX = value;
                if (_viewBeginX < 0) _viewBeginX = 0;
            }
        }

        public int ViewBeginY
        {
            get { return _viewBeginY; }
            set
            {
                if (value <= 0) _viewBeginY = 0;
                else if (value + ViewHeight > _mapPixelHeight) _viewBeginY = _mapPixelHeight - ViewHeight;
                else _viewBeginY = value;
                if (_viewBeginY < 0) _viewBeginY = 0;
            }
        }

       

        public int MapPixelWidth
        {
            get { return _mapPixelWidth; }
        }

        public int MapPixelHeight
        {
            get { return _mapPixelHeight; }
        }

        public int MapColumnCounts
        {
            get { return _mapColumnCounts; }
        }

        public int MapRowCounts
        {
            get { return _mapRowCounts; }
        }

        public static Color DrawColor
        {
            get
            {
                if (WeatherManager.IsRaining)
                {
                    return _rainDrawColor;
                }
                else
                {
                    return _drawColor;
                }
            }
            set
            {
                if (WeatherManager.IsRaining)
                {
                    _rainDrawColor = value;
                }
                else
                {
                    _drawColor = value;
                }
            }
        }

        #endregion Public Properties

        #region public static method
        public static Vector2 ToTilePosition(int pixelX, int pixelY, bool boundCheck = true)
        {
            if (boundCheck && (pixelX < 0f || pixelY < 0f)) return new Vector2(0);

            var nx = pixelX / 64;
            var ny = 1 + (pixelY / 32) * 2;

            //now calculate real position, please see maptile.jpg
            var dx = pixelX - nx * 64f;
            var dy = pixelY - ((int)(ny / 2)) * 32f;
            if (dx < 32f)
            {
                if (dy < (32f - dx) / 2f) // 1
                {
                    ny--;
                }
                else if (dy > (dx / 2f + 16f)) // 2
                {
                    ny++;
                }
            }
            if (dx > 32f)
            {
                if (dy < (dx - 32f) / 2f) //3
                {
                    nx++;
                    ny--;
                }
                else if (dy > ((64f - dx) / 2f + 16f)) // 4
                {
                    nx++;
                    ny++;
                }
            }
            return new Vector2(nx, ny);
        }

        public static Vector2 ToTilePosition(Vector2 pixelPositionInWorld, bool boundCheck = true)
        {
            return ToTilePosition((int)pixelPositionInWorld.X, (int)pixelPositionInWorld.Y, boundCheck);
        }

        //Return pixel position of tile center in world
        public static Vector2 ToPixelPosition(int col, int row, bool boundCheck = true)
        {
            if (boundCheck && (col < 0 || row < 0)) return new Vector2(0);

            var basex = (row % 2) * 32 + 64 * col;
            var basey = 16 * row;
            return new Vector2(basex, basey);
        }

        //Return pixel position of tile center in world
        public static Vector2 ToPixelPosition(Vector2 tilePositon, bool boundCheck = true)
        {
            return ToPixelPosition((int)tilePositon.X, (int)tilePositon.Y, boundCheck);
        }
        #endregion public static method

        #region Tiles region in view
        public Vector2 GetStartTileInView()
        {
            return GetStartTileInView(ViewBeginX, ViewBeginY);
        }

        public Vector2 GetEndTileInView()
        {
            return GetEndTileInView(ViewBeginX + ViewWidth,
                ViewBeginY + ViewHeight,
                MapColumnCounts,
                MapRowCounts);
        }

        public static Vector2 GetStartTileInView(int viewBeginX, int viewBeginY)
        {
            var start = ToTilePosition(viewBeginX, viewBeginY);
            start -= new Vector2(20);
            if (start.X < 0) start.X = 0;
            if (start.Y < 0) start.Y = 0;
            return start;
        }

        //viewEndX = ViewBeginX + ViewWidth
        //viewEndY = ViewBeginY + ViewHeight
        public static Vector2 GetEndTileInView(int viewEndX, int viewEndY, int mapColumnCounts, int mapRowCounts)
        {
            var end = ToTilePosition(viewEndX, viewEndY);
            end += new Vector2(20);
            if (end.X > mapColumnCounts) end.X = mapColumnCounts;
            if (end.Y > mapRowCounts) end.Y = mapRowCounts;
            return end;
        }
        #endregion Tiles region in view

        #region Tile
        protected bool IsTileInMapRange(int x, int y)
        {
            return (x >= 0 &&
                    x < _mapColumnCounts &&
                    y >= 0 &&
                    y < _mapRowCounts);
        }

        protected bool IsTileInMapRange(Vector2 tilePosition)
        {
            return IsTileInMapRange((int) tilePosition.X, (int) tilePosition.Y);
        }

        protected bool IsTileInMapViewRange(int col, int row)
        {
            return (col < MapColumnCounts &&
                row < MapRowCounts - 1 &&
                col >= 0 &&
                row > 0);
        }

        public abstract Texture2D GetTileTexture(int x, int y, int layer);

        public abstract Texture2D GetTileTextureAndRegionInWorld(int x, int y, int layer, out Nullable<Rectangle> sourceRegion, ref Rectangle region);

        public abstract bool IsObstacle(int col, int row);

        public bool IsObstacle(Vector2 tilePosition)
        {
            return IsObstacle((int)tilePosition.X, (int)tilePosition.Y);
        }

        public abstract bool IsObstacleForCharacter(int col, int row);

        public bool IsObstacleForCharacter(Vector2 tilePosition)
        {
            return IsObstacleForCharacter((int)tilePosition.X, (int)tilePosition.Y);
        }

        public abstract bool IsObstacleForCharacterJump(int col, int row);

        public bool IsObstacleForCharacterJump(Vector2 tilePosition)
        {
            return IsObstacleForCharacterJump((int)tilePosition.X, (int)tilePosition.Y);
        }

        public abstract bool IsObstacleForMagic(int col, int row);

        public bool IsObstacleForMagic(Vector2 tilePosition)
        {
            return IsObstacleForMagic((int)tilePosition.X, (int)tilePosition.Y);
        }

        /// <summary>
        /// Get tile trap index, return 0 if no trap
        /// </summary>
        /// <param name="col">Column</param>
        /// <param name="row">Row</param>
        /// <returns></returns>
        public abstract int GetTileTrapIndex(int col, int row);

        /// <summary>
        ///  Get tile trap index, return 0 if no trap
        /// </summary>
        /// <param name="tilePosition">Tile column row positon</param>
        /// <returns></returns>
        public int GetTileTrapIndex(Vector2 tilePosition)
        {
            return GetTileTrapIndex((int)tilePosition.X, (int)tilePosition.Y);
        }

        /// <summary>
        /// Get current map tile trap  script.Return null if no trap
        /// </summary>
        /// <param name="col">Column</param>
        /// <param name="row">Row</param>
        /// <param name="trapIndex">[out]The trap's index in map.</param>
        /// <returns></returns>
        public ScriptParser GetTileTrapScriptParser(int col, int row, out int trapIndex)
        {
            trapIndex = GetTileTrapIndex(col, row);
            if (trapIndex == 0) return null;
            return GetMapTrap(trapIndex);
        }

        /// <summary>
        /// Get current map tile trap  script.Return null if no trap
        /// </summary>
        /// <param name="tilePosition">Tile column row positon</param>
        /// <param name="trapIndex">[out]The trap's index in map.</param>
        /// <returns></returns>
        public ScriptParser GetTileTrapScriptParser(Vector2 tilePosition, out int trapIndex)
        {
            return GetTileTrapScriptParser((int)tilePosition.X, (int)tilePosition.Y, out trapIndex);
        }

        public void DrawTile(SpriteBatch spriteBatch, Color drawColor, int layer, Vector2 tilePos, float depth = 1.0f)
        {
            Nullable<Rectangle> sourceRegion;
            Rectangle drawRegion = new Rectangle();
            var texture = GetTileTextureAndRegionInWorld((int) tilePos.X, (int) tilePos.Y, layer, out sourceRegion, ref drawRegion);
            if (texture == null) return;
            drawRegion.X -= ViewBeginX;
            drawRegion.Y -= ViewBeginY;
            spriteBatch.Draw(texture,
                        drawRegion,
                        sourceRegion,
                        drawColor,
                        0f,
                        new Vector2(0f),
                        SpriteEffects.None,
                        depth);
        }
        #endregion Tile

        #region Trap
        /// <summary>
        /// Load trap from file
        /// </summary>
        /// <param name="filePath">File path</param>
        public static void LoadTrap(string filePath)
        {
            //Clear ingnored traps list
            _ingnoredTrapsIndex.Clear();

            _traps.Clear();
            try
            {
                var parser = new FileIniDataParser();
                var data = parser.ReadFile(filePath, Globals.LocalEncoding);
                foreach (var section in data.Sections)
                {
                    var list = new Dictionary<int, string>();
                    foreach (var key in section.Keys)
                    {
                        list[int.Parse(key.KeyName)] = key.Value;
                    }
                    _traps[section.SectionName] = list;
                }
            }
            catch (Exception exception)
            {
                Log.LogFileLoadError("Trap", filePath, exception);
            }
        }

        /// <summary>
        /// Svae trap to file
        /// </summary>
        /// <param name="path">File path</param>
        public static void SaveTrap(string path)
        {
            try
            {
                var data = new IniData();
                foreach (var key in _traps.Keys)
                {
                    data.Sections.AddSection(key);
                    var list = _traps[key];
                    if(list == null) continue;
                    foreach (var k in list.Keys)
                    {
                        data[key].AddKey(k.ToString(), list[k]);
                    }
                }
                File.WriteAllText(path, data.ToString(), Globals.LocalEncoding);
            }
            catch (Exception exception)
            {
                Log.LogFileSaveError("Trap", path, exception);
            }
        }

        public static void LoadTrapIndexIgnoreList(string filePath)
        {
            _ingnoredTrapsIndex.Clear();
            try
            {
                if(!File.Exists(filePath)) return;
                var data = new FileIniDataParser().ReadFile(filePath, Globals.LocalEncoding);
                var section = Utils.GetFirstSection(data);
                foreach (var keyData in section)
                {
                    _ingnoredTrapsIndex.Add(int.Parse(keyData.Value));
                }
            }
            catch (Exception exception)
            {
                Log.LogFileLoadError("Trap index ignore list", filePath, exception);
            }
        }

        public static void SaveTrapIndexIgnoreList(string filePath)
        {
            try
            {
                var data = new IniData();
                data.Sections.AddSection("Init");
                var section = data["Init"];
                var count = _ingnoredTrapsIndex.Count;
                for (var i = 0; i < count; i++)
                {
                    section.AddKey(i.ToString(), _ingnoredTrapsIndex[i].ToString());
                }

                File.WriteAllText(filePath, data.ToString(), Globals.LocalEncoding);
            }
            catch (Exception exception)
            {
                Log.LogFileSaveError("Trap index ignore list", filePath, exception);
            }
        }

        /// <summary>
        /// Set map trap
        /// </summary>
        /// <param name="mapName">If null or empty, use current map name</param>
        /// <param name="index">Trap index</param>
        /// <param name="trapFileName">Trap file name</param>
        public void SetMapTrap(int index, string trapFileName, string mapName = null)
        {
            if (string.IsNullOrEmpty(mapName) ||
                mapName == _mapFileNameWithoutExtension)
            {
                //Remove index in ingnored traps list activating tarp on this index.
                _ingnoredTrapsIndex.Remove(index);
            }

            if (string.IsNullOrEmpty(mapName))
                mapName = _mapFileNameWithoutExtension;
            if (string.IsNullOrEmpty(mapName)) return;//no map name

            if (!_traps.ContainsKey(mapName))
            {
                _traps[mapName] = new Dictionary<int, string>();//add key
            }
            var list = _traps[mapName];
            if (!list.ContainsKey(index))
            {
                list[index] = "";//add key
            }
            if (string.IsNullOrEmpty(trapFileName))
                list.Remove(index); //remove trap
            else
                list[index] = trapFileName; // change trap
        }

        /// <summary>
        /// Get map trap, if failed retrun null
        /// </summary>
        /// <param name="mapName">If null or empty, use current map name</param>
        /// <param name="index">Trap index</param>
        /// <returns></returns>
        public ScriptParser GetMapTrap(int index, string mapName = null)
        {
            if (string.IsNullOrEmpty(mapName))
                mapName = _mapFileNameWithoutExtension;
            if (string.IsNullOrEmpty(mapName)) return null;//no map name

            var trapFileName = GetMapTrapFileName(index, mapName);
            if (!string.IsNullOrEmpty(trapFileName))
            {
                return Utils.GetScriptParser(trapFileName, mapName);
            }

            return null;
        }

        public string GetMapTrapFileName(int index, string mapName = null)
        {
            if (string.IsNullOrEmpty(mapName))
                mapName = _mapFileNameWithoutExtension;
            if (string.IsNullOrEmpty(mapName)) return null;//no map name

            if (_traps.ContainsKey(mapName))
            {
                var list = _traps[mapName];
                if (list.ContainsKey(index))
                {
                    return list[index];
                }
            }
            return null;
        }

        public bool HasTrapScript(Vector2 tilePosition)
        {
            int index = GetTileTrapIndex(tilePosition);
            if (index == 0 || string.IsNullOrEmpty(GetMapTrapFileName(index))) return false;
            if (_ingnoredTrapsIndex.Any(i => index == i))
            {
                //Script is ignored
                return false;
            }
            return true;
        }

        /// <summary>
        /// Run trap script in current tile position
        /// </summary>
        /// <param name="tilePosition"></param>
        public void RunTileTrapScript(Vector2 tilePosition)
        {
            int index;
            var script = GetTileTrapScriptParser(tilePosition, out index);
            if (script != null)
            {
                if (_ingnoredTrapsIndex.Any(i => index == i))
                {
                    //Script is ignored, don't run it.
                    return;
                }

                //Player standing when run map trap script
                Globals.ThePlayer.StandingImmediately();

                _isInRunMapTrap = true;
                // disable input prevet from unexpected result
                Globals.IsInputDisabled = true;

                _currentRunTrapScript = ScriptManager.RunScript(script);
                

                //Script is run, add to ignore list
                _ingnoredTrapsIndex.Add(index);
            }
        }
        #endregion Trap

        #region Layer

        public virtual void DrawTrapLayer(SpriteBatch spriteBatch)
        {
            //Do nothing, override this to draw trap info layer.
        }

        public virtual void DrawObstacleLayer(SpriteBatch spriteBatch)
        {
            //Do nothing, override this to draw obstacle info.
        }

        public void DrawLayer(SpriteBatch spriteBatch, Color drawColor, int layer)
        {
            if (!IsOk) return;
            if ((layer < 0 || layer > (MaxLayer - 1)) || !IsLayerDraw(layer)) return;
            var start = GetStartTileInView();
            var end = GetEndTileInView();
            for (var y = (int)start.Y; y < (int)end.Y; y++)
            {
                for (var x = (int)start.X; x < (int)end.X; x++)
                {
                    DrawTile(spriteBatch, drawColor, layer, new Vector2(x, y), 1f);
                }
            }
        }

        public static void SetLayerDraw(int layer, bool isDraw)
        {
            if (layer < 0 || layer > (MaxLayer - 1)) return;
            _isLayerDraw[layer] = isDraw;
        }

        public static bool IsLayerDraw(int layer)
        {
            if (layer < 0 || layer > (MaxLayer - 1)) return false;
            return _isLayerDraw[layer];
        }

        public static void SwitchLayerDraw(int layer)
        {
            SetLayerDraw(layer, !IsLayerDraw(layer));
        }
        #endregion Layer

        public static void OpenMap(string mapFileName)
        {
            var ext = Path.GetExtension(mapFileName);
            switch (ext)
            {
                case ".map":
                    Instance = new JxqyMap(mapFileName);
                    break;
                case ".tmx":
                    Instance = new TiledMap(mapFileName);
                    break;
            }
        }

        public void LoadMap(string mapFileName)
        {
            var path = @"map\" + mapFileName;
            try
            {
                IsOk = LoadMapInternal(path);
            }
            catch (Exception e)
            {
                Log.LogFileLoadError("Map", path, e);
            }
            Globals.TheCarmera.WorldWidth = MapPixelWidth;
            Globals.TheCarmera.WorldHeight = MapPixelHeight;
            _mapFileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
            _mapFileName = Path.GetFileName(path);
            LittelMapTexture = Utils.LoadTexture2DFromFile(@"map\littlemap\" + _mapFileNameWithoutExtension + ".png");
        }

        protected abstract bool LoadMapInternal(string mapFilePath);

        /// <summary>
        /// Free map resource, make map to empty map
        /// </summary>
        public static void Free()
        {
            if (Instance != null)
            {
                Instance.FreeInternal();
            }
        }

        protected virtual void FreeInternal()
        {
            IsOk = false;
            _mapColumnCounts =
                _mapRowCounts =
                _mapPixelWidth =
                _mapPixelHeight = 0;
        }

        /// <summary>
        /// Get rand position in map.
        /// </summary>
        /// <param name="tilePostion">Begin tile positon.</param>
        /// <param name="max">Max tile distance to the begin tile position.</param>
        /// <returns>The rand tile positon.<see cref="Vector2.Zero"/> if can't find the tile in map range.</returns>
        public Vector2 GetRandPositon(Vector2 tilePostion, int max)
        {
            var randPosition = Vector2.Zero;
            var maxtry = 10;
            do
            {
                maxtry--;
                randPosition.X = tilePostion.X + Globals.TheRandom.Next(-max, max);
                randPosition.Y = tilePostion.Y + Globals.TheRandom.Next(-max, max);
            } while (!IsTileInMapRange(randPosition) && maxtry >= 0);

            return maxtry< 0 ? Vector2.Zero : randPosition;
        }

        public void Update(GameTime gameTime)
        {
            Globals.TheCarmera.Update(gameTime);
            ViewBeginX = Globals.TheCarmera.ViewBeginX;
            ViewBeginY = Globals.TheCarmera.ViewBeginY;

            //Trap script
            if (_isInRunMapTrap)
            {
                if (_currentRunTrapScript == null || 
                    _currentRunTrapScript.IsEnd)
                {
                    _isInRunMapTrap = false;
                    _currentRunTrapScript = null;
                    Globals.IsInputDisabled = false;
                }
            }
        }

        /// <summary>
        /// Draw map npcs objs magic sprits
        /// </summary>
        /// <param name="spriteBatch"></param>
        public void Draw(SpriteBatch spriteBatch)
        {
            if (DrawColor == Color.Black)
            {
                spriteBatch.End();
                JxqyGame.BeginSpriteBatch(spriteBatch, Globals.TheGame.GrayScaleEffect);

                DrawLayer(spriteBatch, DrawColor, 0);

                spriteBatch.End();
                JxqyGame.BeginSpriteBatch(spriteBatch);
            }
            else
            {
                DrawLayer(spriteBatch, DrawColor, 0);
            }

            var start = GetStartTileInView();
            var end = GetEndTileInView();
            var magicSprites = MagicManager.MagicSpritesInView;
            var npcs = NpcManager.NpcsInView;
            var objs = ObjManager.ObjsInView;

            //Draw body
            foreach (var obj in objs)
            {
                if (obj.Kind == 2)
                {
                    obj.Draw(spriteBatch);
                }
            }

            for (var y = (int)start.Y; y < (int)end.Y; y++)
            {
                for (var x = (int)start.X; x < (int)end.X; x++)
                {
                    if (IsLayerDraw(1) && GetTileTexture(x, y, 1) != null)
                    {
                        if (DrawColor == Color.Black)
                        {
                            spriteBatch.End();
                            JxqyGame.BeginSpriteBatch(spriteBatch, Globals.TheGame.GrayScaleEffect);

                            DrawTile(spriteBatch, DrawColor, 1, new Vector2(x, y), 1f);

                            spriteBatch.End();
                            JxqyGame.BeginSpriteBatch(spriteBatch);
                        }
                        else
                        {
                            DrawTile(spriteBatch, DrawColor, 1, new Vector2(x, y), 1f);
                        }
                    }
                    foreach (var npc in npcs)
                    {
                        if (x == npc.MapX && y == npc.MapY && npc.Kind != 7)
                            npc.Draw(spriteBatch);
                    }
                    foreach (var obj in objs)
                    {
                        if (x == obj.MapX && y == obj.MapY && obj.Kind != 2)
                            obj.Draw(spriteBatch);
                    }
                    foreach (var magicSprite in magicSprites)
                    {
                        if (x == magicSprite.MapX && y == magicSprite.MapY)
                            magicSprite.Draw(spriteBatch);
                    }
                }
            }

            if (DrawColor == Color.Black)
            {
                spriteBatch.End();
                JxqyGame.BeginSpriteBatch(spriteBatch, Globals.TheGame.GrayScaleEffect);

                DrawLayer(spriteBatch, DrawColor, 2);

                spriteBatch.End();
                JxqyGame.BeginSpriteBatch(spriteBatch);
            }
            else
            {
                DrawLayer(spriteBatch, DrawColor, 2);
            }

            //Draw fly npc
            foreach (var npc in npcs)
            {
                if (npc.Kind == 7)
                {
                    npc.Draw(spriteBatch);
                }
            }

            if (IsLayerDraw(3)) DrawTrapLayer(spriteBatch); //Trap
            if (IsLayerDraw(4)) DrawObstacleLayer(spriteBatch); //Obstacle
        }
    }
}
