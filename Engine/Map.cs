using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Engine.Script;
using IniParser;
using IniParser.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine
{
    public class Map
    {
        struct MapMpcIndex
        {
            public byte Frame;
            public byte MpcIndex;
        }

        struct MapTileInfo
        {
            public byte TrapIndex;
            public byte BarrierType;
        }

        private const byte None = 0; //无
        private const byte Obstacle = 0x80; //障
        private const byte CanOverObstacle = 0xA0; //跳障，可跳过
        private const byte Trans = 0x40; //透，武功可以透过，人不能透过
        private const byte CanOverTrans = 0x60; //跳透，可跳过
        private const byte CanOver = 0x20; //可跳过

        private readonly List<Mpc> _mpcList = new List<Mpc>(255);
        private readonly List<Mpc> _loopingList = new List<Mpc>();
        private string _mpcDirPath;
        private int _mapColumnCounts;
        private int _mapRowCounts;
        private int _mapPixelWidth;
        private int _mapPixelHeight;
        private MapMpcIndex[] _layer1, _layer2, _layer3;
        private MapTileInfo[] _tileInfos;
        private readonly bool[] _isLayerDraw = new bool[3] { true, true, true };
        private static Color _drawColor = Color.White;

        //traps
        private readonly Dictionary<string, Dictionary<int, string>> _traps = new Dictionary<string, Dictionary<int, string>>();
        private readonly List<int> _ingnoredTrapsIndex = new List<int>(); 

        private int _viewBeginX;
        private int _viewBeginY;
        private int _viewWidth;
        private int _viewHeight;

        private string _mapFileNameWithoutExtension;

        #region Public Properties

        public bool IsOk { private set; get; }

        public string MapFileNameWithoutExtension { get { return _mapFileNameWithoutExtension; } }

        public static int MapTime;

        public int ViewHeight
        {
            get { return _viewHeight; }
            set { _viewHeight = value < 0 ? 0 : value; }
        }

        public int ViewWidth
        {
            get { return _viewWidth; }
            set { _viewWidth = value < 0 ? 0 : value; }
        }

        public int ViewBeginY
        {
            get { return _viewBeginY; }
            set
            {
                if (value + ViewHeight > _mapPixelHeight) _viewBeginY = _mapPixelHeight - ViewHeight;
                else if (value < 0) _viewBeginY = 0;
                else _viewBeginY = value;
            }
        }

        public int ViewBeginX
        {
            get { return _viewBeginX; }
            set
            {
                if (value + ViewWidth > _mapPixelWidth)
                {
                    _viewBeginX = _mapPixelWidth - ViewWidth;
                    //if map too small, _viewBeginX is negative
                    if (_viewBeginX < 0) _viewBeginX = 0;
                }
                else if (value < 0) _viewBeginX = 0;
                else _viewBeginX = value;
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
            get { return _drawColor; }
            set { _drawColor = value; }
        }

        #endregion Public Properties

        #region public static method
        public static Vector2 ToTilePosition(int pixelX, int pixelY)
        {
            if (pixelX < 0f || pixelY < 0f) return new Vector2(0);

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

        public static Vector2 ToTilePosition(Vector2 pixelPositionInWorld)
        {
            return ToTilePosition((int)pixelPositionInWorld.X, (int)pixelPositionInWorld.Y);
        }

        //Return pixel position of tile center in world
        public static Vector2 ToPixelPosition(int col, int row)
        {
            if (col < 0 || row < 0) return new Vector2(0);

            var basex = (row % 2) * 32 + 64 * col;
            var basey = 16 * row;
            return new Vector2(basex, basey);
        }

        //Return pixel position of tile center in world
        public static Vector2 ToPixelPosition(Vector2 tilePositon)
        {
            return ToPixelPosition((int)tilePositon.X, (int)tilePositon.Y);
        }
        #endregion public static method

        #region Private map load
        private void LoadMapTiles(byte[] buf, ref int offset)
        {
            var totalTile = _mapColumnCounts * _mapRowCounts;
            _layer1 = new MapMpcIndex[totalTile];
            _layer2 = new MapMpcIndex[totalTile];
            _layer3 = new MapMpcIndex[totalTile];
            _tileInfos = new MapTileInfo[totalTile];
            for (var i = 0; i < totalTile; i++)
            {
                _layer1[i].Frame = buf[offset++];
                _layer1[i].MpcIndex = buf[offset++];
                _layer2[i].Frame = buf[offset++];
                _layer2[i].MpcIndex = buf[offset++];
                _layer3[i].Frame = buf[offset++];
                _layer3[i].MpcIndex = buf[offset++];
                _tileInfos[i].BarrierType = buf[offset++];
                _tileInfos[i].TrapIndex = buf[offset++];
                offset += 2;
            }
        }

        private void LoadMpc(byte[] buf, ref int offset)
        {
            offset = 192;
            for (var k = 0; k < 255; k++)
            {
                var mpcFileName = new byte[32];
                var i = 0;
                for (; buf[offset + i] != 0; i++)
                {
                    mpcFileName[i] = buf[offset + i];
                }
                if (i == 0) _mpcList.Add(null);
                else
                {
                    var mpc = new Mpc(
                        _mpcDirPath + "\\" +
                        Globals.SimpleChineseEncoding.GetString(mpcFileName, 0, i));
                    _mpcList.Add(mpc);
                    if (buf[offset + 36] == 1) _loopingList.Add(mpc);
                }
                offset += 64;
            }
            offset = 16512;
        }

        private bool LoadHead(byte[] buf, ref int offset)
        {
            var headInfo = Globals.SimpleChineseEncoding.GetString(buf, 0, "MAP File Ver".Length);
            if (!headInfo.Equals("MAP File Ver")) return false;
            offset = 32;
            var len = 0;
            while (buf[offset + len] != 0) len++;
            if (len > 0) len--;
            _mpcDirPath = Globals.SimpleChineseEncoding.GetString(buf, offset + 1, len);
            offset = 68;
            _mapColumnCounts = Utils.GetLittleEndianIntegerFromByteArray(buf, ref offset);
            _mapRowCounts = Utils.GetLittleEndianIntegerFromByteArray(buf, ref offset);
            _mapPixelWidth = (_mapColumnCounts - 1) * 64;
            _mapPixelHeight = ((_mapRowCounts - 3) / 2 + 1) * 32;
            return true;
        }
        #endregion Private map load

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
            start -= new Vector2(15);
            if (start.X < 0) start.X = 0;
            if (start.Y < 0) start.Y = 0;
            return start;
        }

        //viewEndX = ViewBeginX + ViewWidth
        //viewEndY = ViewBeginY + ViewHeight
        public static Vector2 GetEndTileInView(int viewEndX, int viewEndY, int mapColumnCounts, int mapRowCounts)
        {
            var end = ToTilePosition(viewEndX, viewEndY);
            end += new Vector2(15);
            if (end.X > mapColumnCounts) end.X = mapColumnCounts;
            if (end.Y > mapRowCounts) end.Y = mapRowCounts;
            return end;
        }
        #endregion Tiles region in view

        #region Tile
        private bool IsTileInMapRange(int x, int y)
        {
            return (x >= 0 &&
                    x < _mapColumnCounts &&
                    y >= 0 &&
                    y < _mapRowCounts);
        }

        private bool IsTileInMapRange(Vector2 tilePosition)
        {
            return IsTileInMapRange((int) tilePosition.X, (int) tilePosition.Y);
        }

        private bool IsTileInMapViewRange(int col, int row)
        {
            return (col < MapColumnCounts - 1 &&
                row < MapRowCounts - 3 &&
                col > 0 &&
                row > 0);
        }

        public Texture2D GetTileTexture(int x, int y, int layer)
        {
            if (!IsTileInMapRange(x, y)) return null;
            MapMpcIndex[] idx;
            switch (layer)
            {
                case 0:
                    idx = _layer1;
                    break;
                case 1:
                    idx = _layer2;
                    break;
                case 2:
                    idx = _layer3;
                    break;
                default:
                    return null;
            }
            var pos = x + y * _mapColumnCounts;
            if (idx[pos].MpcIndex == 0) return null;
            return _mpcList[idx[pos].MpcIndex - 1].GetFrame(idx[pos].Frame);
        }

        public Texture2D GetTileTextureAndRegion(int x, int y, int layer, ref Rectangle region)
        {
            if (!IsOk) return null;
            var texture = GetTileTexture(x, y, layer);
            if (texture != null)
            {
                var position = Map.ToPixelPosition(x, y) - new Vector2(texture.Width / 2f, texture.Height - 16f);
                region = new Rectangle((int)position.X, (int)position.Y, texture.Width, texture.Height);
            }
            return texture;
        }

        public static bool HasNpcObjObstacleInMap(Vector2 tilePositon)
        {
            return (NpcManager.IsObstacle(tilePositon) ||
                    ObjManager.IsObstacle(tilePositon) ||
                    Globals.ThePlayer.TilePosition == tilePositon);
        }

        public bool IsObstacle(int col, int row)
        {
            if (IsTileInMapViewRange(col, row))
            {
                var type = _tileInfos[col + row * MapColumnCounts].BarrierType;
                if ((type & Obstacle) == 0)
                    return false;
            }
            return true;
        }

        public bool IsObstacle(Vector2 tilePosition)
        {
            return IsObstacle((int)tilePosition.X, (int)tilePosition.Y);
        }

        public bool IsObstacleForCharacter(int col, int row)
        {
            if (IsTileInMapViewRange(col, row))
            {
                var type = _tileInfos[col + row * MapColumnCounts].BarrierType;
                if (type == None)
                    return false;
            }
            return true;
        }

        public bool IsObstacleForCharacter(Vector2 tilePosition)
        {
            return IsObstacleForCharacter((int)tilePosition.X, (int)tilePosition.Y);
        }

        public bool IsObstacleForCharacterJump(int col, int row)
        {
            if (IsTileInMapViewRange(col, row))
            {
                var type = _tileInfos[col + row * MapColumnCounts].BarrierType;
                if (type == None ||
                    (type & CanOver) != 0)
                    return false;
            }
            return true;
        }

        public bool IsObstacleForCharacterJump(Vector2 tilePosition)
        {
            return IsObstacleForCharacterJump((int)tilePosition.X, (int)tilePosition.Y);
        }

        public bool IsObstacleForMagic(int col, int row)
        {
            if (IsTileInMapViewRange(col, row))
            {
                var type = _tileInfos[col + row * MapColumnCounts].BarrierType;
                if (type == None ||
                    (type & Trans) != 0)
                    return false;
            }
            return true;
        }

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
        public int GetTileTrapIndex(int col, int row)
        {
            if (IsTileInMapViewRange(col, row))
            {
                return _tileInfos[col + row * MapColumnCounts].TrapIndex;
            }
            return 0;
        }

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

        public void DrawTile(SpriteBatch spriteBatch, Texture2D texture, Vector2 tilePos, float depth)
        {
            if (texture == null) return;
            Vector2 tileBegPixelPos = ToPixelPosition(tilePos);
            tileBegPixelPos.X -= texture.Width / 2f;
            tileBegPixelPos.Y -= texture.Height - 16f;
            spriteBatch.Draw(texture,
                        new Rectangle((int)tileBegPixelPos.X - ViewBeginX, (int)tileBegPixelPos.Y - ViewBeginY, texture.Width, texture.Height),
                        new Rectangle(0, 0, texture.Width, texture.Height),
                        DrawColor,
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
        public void LoadTrap(string filePath)
        {
            //Clear ingnored traps list
            _ingnoredTrapsIndex.Clear();

            _traps.Clear();
            try
            {
                var parser = new FileIniDataParser();
                var data = parser.ReadFile(filePath);
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
        public void SaveTrap(string path)
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
                File.WriteAllText(path, data.ToString(), Globals.SimpleChineseEncoding);
            }
            catch (Exception exception)
            {
                Log.LogFileSaveError("Trap", path, exception);
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

            if (_traps.ContainsKey(mapName))
            {
                var list = _traps[mapName];
                if (list.ContainsKey(index))
                {
                    return Utils.GetScriptParser(list[index], null, mapName);
                }
            }

            return null;
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
                ScriptManager.RunScript(script);
                //Script is run, add to ignore list
                _ingnoredTrapsIndex.Add(index);
            }
        }
        #endregion Trap

        #region Layer
        public void DrawLayer(SpriteBatch spriteBatch, int layer)
        {
            if (!IsOk) return;
            if ((layer < 0 || layer > 2) || !IsLayerDraw(layer)) return;
            var start = GetStartTileInView();
            var end = GetEndTileInView();
            for (var y = (int)start.Y; y < (int)end.Y; y++)
            {
                for (var x = (int)start.X; x < (int)end.X; x++)
                {
                    Texture2D texture = GetTileTexture(x, y, layer);
                    DrawTile(spriteBatch, texture, new Vector2(x, y), 1f);
                }
            }
        }

        public void SetLayerDraw(int layer, bool isDraw)
        {
            if (layer < 0 || layer > 2) return;
            _isLayerDraw[layer] = isDraw;
        }

        public bool IsLayerDraw(int layer)
        {
            if (layer < 0 || layer > 2) return false;
            return _isLayerDraw[layer];
        }

        public void SwitchLayerDraw(int layer)
        {
            SetLayerDraw(layer, !IsLayerDraw(layer));
        }
        #endregion Layer

        public void LoadMap(string mapFileName)
        {
            var path = @"map\" + mapFileName;
            try
            {
                var buf = File.ReadAllBytes(path);
                LoadMap(buf);
            }
            catch (Exception e)
            {
                Log.LogFileLoadError("Map", path, e);
            }
            Globals.TheCarmera.WorldWidth = MapPixelWidth;
            Globals.TheCarmera.WorldHeight = MapPixelHeight;
            _mapFileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
        }

        public void LoadMap(byte[] buf)
        {
            //Clear ingnored traps list
            _ingnoredTrapsIndex.Clear();

            Free();
            //Clear asf cache, because normaly npcs objs will be cleared after map load.
            Utils.ClearAsfCache();
            var offset = 0;
            try
            {
                if (!LoadHead(buf, ref offset)) return;
                LoadMpc(buf, ref offset);
                LoadMapTiles(buf, ref offset);
            }
            catch (Exception e)
            {
                Log.LogMessageToFile("Map file is corrupted" + ": " + e);
            }
            IsOk = true;
        }

        /// <summary>
        /// Free map resource, make map to empty map
        /// </summary>
        public void Free()
        {
            IsOk = false;
            _mpcList.Clear();
            _loopingList.Clear();
            _layer1 = _layer2 = _layer3 = null;
            _tileInfos = null;
            _mapColumnCounts =
                _mapRowCounts =
                _mapPixelWidth =
                _mapPixelHeight = 0;
        }

        /// <summary>
        /// Get rand position in map.
        /// </summary>
        /// <param name="tilePostion">Base tile positon.</param>
        /// <param name="max">Max x,y offset to the base tile posiont.</param>
        /// <returns>The rand tile positon.<see cref="Vector2.Zero"/> if can't find the tile in map range.</returns>
        public Vector2 GetRandPositon(Vector2 tilePostion, int max)
        {
            var randPosition = Vector2.Zero;
            var maxtry = 10;
            do
            {
                maxtry--;
                randPosition.X = tilePostion.X + Globals.TheRandom.Next(0, max);
                randPosition.Y = tilePostion.Y + Globals.TheRandom.Next(0, max);
            } while (!IsTileInMapRange(randPosition) && maxtry >= 0);

            return maxtry< 0 ? Vector2.Zero : randPosition;
        }

        public void Update(GameTime gameTime)
        {
            Globals.TheCarmera.Update(gameTime);
            ViewBeginX = Globals.TheCarmera.ViewBeginX;
            ViewBeginY = Globals.TheCarmera.ViewBeginY;
        }

        /// <summary>
        /// Draw map npcs objs magic sprits
        /// </summary>
        /// <param name="spriteBatch"></param>
        public void Draw(SpriteBatch spriteBatch)
        {
            DrawLayer(spriteBatch, 0);

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
                    Texture2D texture = GetTileTexture(x, y, 1);
                    if (IsLayerDraw(1)) DrawTile(spriteBatch, texture, new Vector2(x, y), 1f);
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

            DrawLayer(spriteBatch, 2);

            //Draw fly npc
            foreach (var npc in npcs)
            {
                if (npc.Kind == 7)
                {
                    npc.Draw(spriteBatch);
                }
            }
        }
    }
}
