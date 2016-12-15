using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Map
{
    public class JxqyMap : MapBase
    {
        public JxqyMap(string mapFileName)
        {
            LoadMap(mapFileName);
        }

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
        private MapMpcIndex[] _layer1, _layer2, _layer3;
        private MapTileInfo[] _tileInfos;

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
                        Globals.LocalEncoding.GetString(mpcFileName, 0, i));
                    _mpcList.Add(mpc);
                    if (buf[offset + 36] == 1) _loopingList.Add(mpc);
                }
                offset += 64;
            }
            offset = 16512;
        }

        private bool LoadHead(byte[] buf, ref int offset)
        {
            var headInfo = Globals.LocalEncoding.GetString(buf, 0, "MAP File Ver".Length);
            if (!headInfo.Equals("MAP File Ver")) return false;
            offset = 32;
            var len = 0;
            while (buf[offset + len] != 0) len++;
            if (len > 0) len--;
            _mpcDirPath = Globals.LocalEncoding.GetString(buf, offset + 1, len);
            offset = 68;
            _mapColumnCounts = Utils.GetLittleEndianIntegerFromByteArray(buf, ref offset);
            _mapRowCounts = Utils.GetLittleEndianIntegerFromByteArray(buf, ref offset);
            _mapPixelWidth = (_mapColumnCounts - 1) * 64;
            _mapPixelHeight = ((_mapRowCounts - 3) / 2 + 1) * 32;
            return true;
        }
        #endregion Private map load

        #region Tile
        public override Texture2D GetTileTexture(int x, int y, int layer)
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
            var ts = _mpcList[idx[pos].MpcIndex - 1];
            return (ts == null ? null : ts.GetFrame(idx[pos].Frame));
        }

        public override Texture2D GetTileTextureAndRegionInWorld(int x, int y, int layer, out Nullable<Rectangle> sourceRegion, ref Rectangle region)
        {
            sourceRegion = null;
            if (!IsOk) return null;
            var texture = GetTileTexture(x, y, layer);
            if (texture != null)
            {
                var position = MapBase.ToPixelPosition(x, y) - new Vector2(texture.Width / 2f, texture.Height - 16f);
                region = new Rectangle((int)position.X, (int)position.Y, texture.Width, texture.Height);
            }
            return texture;
        }

        public override bool IsObstacle(int col, int row)
        {
            if (IsTileInMapViewRange(col, row))
            {
                var type = _tileInfos[col + row * MapColumnCounts].BarrierType;
                if ((type & Obstacle) == 0)
                    return false;
            }
            return true;
        }

        public override bool IsObstacleForCharacter(int col, int row)
        {
            if (IsTileInMapViewRange(col, row))
            {
                var type = _tileInfos[col + row * MapColumnCounts].BarrierType;
                if ((type & (Obstacle + Trans)) == 0)
                    return false;
            }
            return true;
        }

        public override bool IsObstacleForCharacterJump(int col, int row)
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

        public override bool IsObstacleForMagic(int col, int row)
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

        /// <summary>
        /// Get tile trap index, return 0 if no trap
        /// </summary>
        /// <param name="col">Column</param>
        /// <param name="row">Row</param>
        /// <returns></returns>
        public override int GetTileTrapIndex(int col, int row)
        {
            if (IsTileInMapViewRange(col, row))
            {
                return _tileInfos[col + row * MapColumnCounts].TrapIndex;
            }
            return 0;
        }
        #endregion Tile

        protected override bool LoadMapInternal(string mapFilePath)
        {
            var buf = File.ReadAllBytes(mapFilePath);
            return LoadMapFromBuffer(buf);
        }

        private bool LoadMapFromBuffer(byte[] buf)
        {
            //Clear ingnored traps list
            _ingnoredTrapsIndex.Clear();

            FreeInternal();
            //Clear asf cache, because normaly npcs objs will be cleared after map load.
            Utils.ClearTextureCache();

            //Destory magic sprite in current map
            MagicManager.Renew();

            var offset = 0;
            try
            {
                if (!LoadHead(buf, ref offset)) return false;
                LoadMpc(buf, ref offset);
                LoadMapTiles(buf, ref offset);
            }
            catch (Exception e)
            {
                Log.LogMessage("Map file is corrupted" + ": " + e);
            }
            return true;
        }

        protected override void FreeInternal()
        {
            _mpcList.Clear();
            _loopingList.Clear();
            _layer1 = _layer2 = _layer3 = null;
            _tileInfos = null;

            base.FreeInternal();
        }
    }
}
