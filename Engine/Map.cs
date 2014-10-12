using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        private List<Mpc> _mpcList;
        private List<Mpc> _loopingList;
        private string _mpcDirPath;
        private int _mapColumnCounts;
        private int _mapRowCounts;
        private int _mapPixelWidth;
        private int _mapPixelHeight;
        private MapMpcIndex[] _layer1, _layer2, _layer3;
        private MapTileInfo[] _tileInfos;
        private readonly bool[] _isLayerDraw = new bool[3]{true, true, true};

        private int _viewBeginX;
        private int _viewBeginY;
        private int _viewWidth;
        private int _viewHeight;

        #region Public Properties
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
                if (value + ViewWidth > _mapPixelWidth) _viewBeginX = _mapPixelWidth - ViewWidth;
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

        #endregion Public Properties

        #region public static method
        public static Vector2 ToTilePosition(int pixelX, int pixelY)
        {
            if (pixelX < 0 || pixelY < 0) return new Vector2(0);

            var nx = pixelX / 64;
            var ny = 1 + (pixelY / 32) * 2;

            //now calculate real position, please see 获取地图坐标.jpg
            var dx = pixelX - nx * 64;
            var dy = pixelY - (ny / 2) * 32;
            if (dx < 32)
            {
                if (dy < (32 - dx) / 2) // 1
                {
                    ny--;
                }
                else if (dy > (dx / 2 + 16)) // 2
                {
                    ny++;
                }
            }
            if (dx > 32)
            {
                if (dy < (dx - 32) / 2) //3
                {
                    nx++;
                    ny--;
                }
                else if (dy > ((64 - dx) / 2 + 16)) // 4
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
            _mpcList = new List<Mpc>(255);
            _loopingList = new List<Mpc>();
            offset = 192;
            for(var k = 0; k < 255; k++)
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
                        Encoding.GetEncoding(Globals.SimpleChinaeseCode).GetString(mpcFileName, 0, i));
                    _mpcList.Add(mpc);
                    if(buf[offset + 36] ==  1) _loopingList.Add(mpc);
                }
                offset += 64;
            }
            offset = 16512;
        }

        private bool LoadHead(byte[] buf, ref int offset)
        {
            var headInfo = Encoding.GetEncoding(Globals.SimpleChinaeseCode).GetString(buf, 0, "MAP File Ver".Length);
            if (!headInfo.Equals("MAP File Ver")) return false;
            offset = 32;
            var len = 0;
            while (buf[offset + len] != 0) len++;
            if (len > 0) len--;
            _mpcDirPath = Encoding.GetEncoding(Globals.SimpleChinaeseCode).GetString(buf, offset + 1, len);
            offset = 68;
            _mapColumnCounts = Utils.GetLittleEndianIntegerFromByteArray(buf, ref offset);
            _mapRowCounts = Utils.GetLittleEndianIntegerFromByteArray(buf, ref offset);
            _mapPixelWidth = (_mapColumnCounts - 1) * 64;
            _mapPixelHeight = ((_mapRowCounts - 3) / 2 + 1) * 32;
            return true;
        }

        

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

        public Texture2D GetTileTexture(int x, int y, int layer)
        {
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
            var texture = GetTileTexture(x, y, layer);
            if (texture != null)
            {
                var position = Map.ToPixelPosition(x, y) - new Vector2(texture.Width/2f, texture.Height - 16f);
                region = new Rectangle((int)position.X, (int)position.Y, texture.Width, texture.Height);
            }
            return texture;
        }

        public bool IsTileInMapViewRange(int col, int row)
        {
            return (col < MapColumnCounts - 1 && 
                row < MapRowCounts - 3 && 
                col >= 0 && 
                row >= 0);
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
            return IsObstacle((int) tilePosition.X, (int) tilePosition.Y);
        }

        public bool IsObstacleForCharacter(int col, int row)
        {
            if (IsTileInMapViewRange(col, row))
            {
                var type = _tileInfos[col + row*MapColumnCounts].BarrierType;
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
            return IsObstacleForCharacterJump((int) tilePosition.X, (int) tilePosition.Y);
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
            return IsObstacleForMagic((int) tilePosition.X, (int) tilePosition.Y);
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
                        Color.White,
                        0f,
                        new Vector2(0f),
                        SpriteEffects.None,
                        depth);
        }

        public void DrawLayer(SpriteBatch spriteBatch, int layer)
        {
            if ((layer < 0 || layer > 2) || !_isLayerDraw[layer]) return;
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

        public void LoadMap(string path)
        {
            try
            {
                var buf = File.ReadAllBytes(path);
                LoadMap(buf);
            }
            catch (Exception e)
            {
                Log.LogMessageToFile("Map load failed" + path + ":" + e);
            }
        }

        public void LoadMap(byte[] buf)
        {
            var offset = 0;
            try
            {
                if (!LoadHead(buf, ref offset)) return;
                LoadMpc(buf, ref offset);
                LoadMapTiles(buf, ref offset);
            }
            catch (Exception e)
            {
                Log.LogMessageToFile("Map file is corrupted" + ":" + e);
            }
        }

        public void SetLayerDraw(int layer, bool isDraw)
        {
            if(layer < 0 || layer > 2) return;
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

        public void Update(GameTime gameTime)
        {
            //在月影传说中，禁用了地图的循环功能
            foreach (var mpc in _loopingList)
            {
                mpc.Update(gameTime);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            DrawLayer(spriteBatch, 0);
            DrawLayer(spriteBatch, 1);
            DrawLayer(spriteBatch, 2);
        }
    }
}
