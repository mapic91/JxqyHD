using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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

        private List<Mpc> _mpcList;
        private Game _game;
        private string _mpcDirPath;
        private int _mapColumnCounts;
        private int _mapRowCounts;
        private int _mapPixelWidth;
        private int _mapPixelHeight;
        private MapMpcIndex[] _layer1, _layer2, _layer3;
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

        public static Vector2 ToTilePosition(Vector2 pixelPositionInWorld, Asf asf)
        {
            //Add back offset
            var positionAtTileCenter = pixelPositionInWorld + new Vector2(asf.Left, asf.Bottom);
            return ToTilePosition(positionAtTileCenter);
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

        private void LoadMapLayer(byte[] buf, ref int offset)
        {
            var totalTile = _mapColumnCounts * _mapRowCounts;
            _layer1 = new MapMpcIndex[totalTile];
            _layer2 = new MapMpcIndex[totalTile];
            _layer3 = new MapMpcIndex[totalTile];
            for (var i = 0; i < totalTile; i++)
            {
                _layer1[i].Frame = buf[offset++];
                _layer1[i].MpcIndex = buf[offset++];
                _layer2[i].Frame = buf[offset++];
                _layer2[i].MpcIndex = buf[offset++];
                _layer3[i].Frame = buf[offset++];
                _layer3[i].MpcIndex = buf[offset++];
                offset += 4;
            }
        }

        private void LoadMpc(byte[] buf, ref int offset)
        {
            _mpcList = new List<Mpc>();
            offset = 192;
            while (true)
            {
                var mpcFileName = new byte[32];
                var i = 0;
                for (; buf[offset + i] != 0; i++)
                {
                    mpcFileName[i] = buf[offset + i];
                }
                if (i == 0) break;
                _mpcList.Add(new Mpc(_game,
                            _mpcDirPath + "\\" + Encoding.GetEncoding(936).GetString(mpcFileName, 0, i)));
                offset += 64;
            }
            offset = 16512;
        }

        private bool LoadHead(byte[] buf, ref int offset)
        {
            var headInfo = Encoding.GetEncoding(936).GetString(buf, 0, "MAP File Ver".Length);
            if (!headInfo.Equals("MAP File Ver")) return false;
            offset = 32;
            var len = 0;
            while (buf[offset + len] != 0) len++;
            if (len > 0) len--;
            _mpcDirPath = Encoding.GetEncoding(936).GetString(buf, offset + 1, len);
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

        public void LoadMap(Game game, string path)
        {
            try
            {
                var buf = File.ReadAllBytes(path);
                LoadMap(game, buf);
            }
            catch (Exception e)
            {
                Log.LogMessageToFile("Map load failed" + path + ":" + e);
            }
        }

        public void LoadMap(Game game, byte[] buf)
        {
            _game = game;
            var offset = 0;
            try
            {
                if (!LoadHead(buf, ref offset)) return;
                LoadMpc(buf, ref offset);
                LoadMapLayer(buf, ref offset);
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

        public void Draw(SpriteBatch spriteBatch)
        {
            DrawLayer(spriteBatch, 0);
            DrawLayer(spriteBatch, 1);
            DrawLayer(spriteBatch, 2);
        }
    }
}
