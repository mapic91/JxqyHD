using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using TiledSharp;

namespace Engine.Map
{
    class TiledMap : MapBase
    {
        public class TileTextureInfo
        {
            public int Gid;
            public TmxTileset BelongTileset;
            public Texture2D Texture;
            public Nullable<Rectangle> TextureSourceRegion;
        }

        private const byte None = 0; //无
        private const byte Obstacle = 0x80; //障
        private const byte CanOverObstacle = 0xA0; //跳障，可跳过
        private const byte Trans = 0x40; //透，武功可以透过，人不能透过
        private const byte CanOverTrans = 0x60; //跳透，可跳过
        private const byte CanOver = 0x20; //可跳过

        private const string Layer1Name = "1";
        private const string Layer2Name = "2";
        private const string Layer3Name = "3";
        private const string TrapName = "T";
        private const string ObstacleName = "O";

        private TmxMap _tmxMap;
        private Dictionary<string, Texture2D> _allTextures = new Dictionary<string, Texture2D>();
        private int _tileTextureCounts;
        private TileTextureInfo[] _tileTextureInfos;
        private TmxLayer[] _mapLayers = new TmxLayer[MaxLayer];
        private TmxLayer _trapLayer, _obstacleLayer;

        private Dictionary<int, int> _trapGidToIndex = new Dictionary<int, int>();
        private Dictionary<int, byte> _obstacleGidToType = new Dictionary<int, byte>();

        public TiledMap(string mapFileName)
        {
            LoadMap(mapFileName);
        }

        public override Texture2D GetTileTexture(int x, int y, int layer)
        {
            if (!IsTileInMapRange(x, y) || layer < 0 || layer > (MaxLayer - 1) || _mapLayers[layer] == null) return null;
            TmxLayer tmxLayer = _mapLayers[layer];
            var pos = x + y * _mapColumnCounts;
            var gid = tmxLayer.Tiles[pos].Gid;

            if (gid == 0)
            {
                return null;
            }

            return _tileTextureInfos[gid].Texture;
        }

        public TileTextureInfo GetTileTextureInfo(int x, int y, int layer)
        {
            if (!IsOk || !IsTileInMapRange(x, y) || layer < 0 || layer > (MaxLayer - 1) || _mapLayers[layer] == null) return null;
            TmxLayer tmxLayer = _mapLayers[layer];
            var pos = x + y * _mapColumnCounts;
            var gid = tmxLayer.Tiles[pos].Gid;

            if (gid == 0)
            {
                return null;
            }

            return _tileTextureInfos[gid];
        }

        public override Texture2D GetTileTextureAndRegionInWorld(int x, int y, int layer, out Nullable<Rectangle> sourceRegion , ref Rectangle region)
        {
            var info = GetTileTextureInfo(x, y, layer);
            if (info != null)
            {
                var width = info.TextureSourceRegion.HasValue
                    ? info.TextureSourceRegion.Value.Width
                    : info.Texture.Width;
                var height = info.TextureSourceRegion.HasValue
                    ? info.TextureSourceRegion.Value.Height
                    : info.Texture.Height;
                var position = MapBase.ToPixelPosition(x, y) -
                    new Vector2(32, height - 16f) +
                    new Vector2(info.BelongTileset.TileOffset.X, info.BelongTileset.TileOffset.Y);
                region = new Rectangle((int)position.X, (int)position.Y, width, height);
                sourceRegion = info.TextureSourceRegion;
                return info.Texture;
            }
            sourceRegion = null;
            return null;
        }

        public override bool IsObstacle(int col, int row)
        {
            if (_obstacleLayer == null) return false;

            if (IsTileInMapViewRange(col, row))
            {
                var gid = _obstacleLayer.Tiles[col + row * MapColumnCounts].Gid;
                if (gid == 0)
                {
                    return false;
                }
                var type = _obstacleGidToType[gid];
                if ((type & Obstacle) == 0)
                    return false;
            }

            return true;
        }

        public override bool IsObstacleForCharacter(int col, int row)
        {
            if (_obstacleLayer == null) return false;

            if (IsTileInMapViewRange(col, row))
            {
                var gid = _obstacleLayer.Tiles[col + row * MapColumnCounts].Gid;
                if (gid == 0)
                {
                    return false;
                }
                var type = _obstacleGidToType[gid];
                if ((type & (Obstacle + Trans)) == 0)
                    return false;
            }

            return true;
        }

        public override bool IsObstacleForCharacterJump(int col, int row)
        {
            if (_obstacleLayer == null) return false;

            if (IsTileInMapViewRange(col, row))
            {
                var gid = _obstacleLayer.Tiles[col + row * MapColumnCounts].Gid;
                if (gid == 0)
                {
                    return false;
                }
                var type = _obstacleGidToType[gid];
                if ((type & CanOver) != 0)
                    return false;
            }

            return true;
        }

        public override bool IsObstacleForMagic(int col, int row)
        {
            if (_obstacleLayer == null) return false;

            if (IsTileInMapViewRange(col, row))
            {
                var gid = _obstacleLayer.Tiles[col + row * MapColumnCounts].Gid;
                if (gid == 0)
                {
                    return false;
                }
                var type = _obstacleGidToType[gid];
                if ((type & Trans) != 0)
                    return false;
            }

            return true;
        }

        public override int GetTileTrapIndex(int col, int row)
        {
            if (_trapLayer != null && IsTileInMapViewRange(col, row))
            {
                var gid = _trapLayer.Tiles[col + row * MapColumnCounts].Gid;
                if (gid != 0)
                {
                    return _trapGidToIndex[gid];
                }
            }
            return 0;
        }

        protected override bool LoadMapInternal(string mapFilePath)
        {
            FreeInternal();

            _tmxMap = new TmxMap(mapFilePath);

            _mapColumnCounts = _tmxMap.Width;
            _mapRowCounts = _tmxMap.Height;
            _mapPixelWidth = (_mapColumnCounts - 1) * 64;
            _mapPixelHeight = ((_mapRowCounts - 3) / 2 + 1) * 32;

            LoadAllTexturesAndComputeTileTextureCount();
            LoadAllTileTextureInfos();
            LoadMapLayers();

            return true;
        }

        private void LoadAllTexturesAndComputeTileTextureCount()
        {
            _tileTextureCounts = 0;
            foreach (var tileset in _tmxMap.Tilesets)
            {
                if (tileset.TileCount != null) _tileTextureCounts += tileset.TileCount.Value;
                if (!string.IsNullOrEmpty(tileset.Image.Source))
                {
                    _allTextures[tileset.Image.Source] = Utils.LoadTexture2DFromFile(tileset.Image.Source);
                }
                foreach (var tile in tileset.Tiles)
                {
                    if (!string.IsNullOrEmpty(tile.Image.Source))
                    {
                        _allTextures[tile.Image.Source] = Utils.LoadTexture2DFromFile(tile.Image.Source);
                    }
                }
            }
        }

        private void LoadAllTileTextureInfos()
        {
            _tileTextureInfos = new TileTextureInfo[_tileTextureCounts + 1]; //tile texture gid start at 1
            foreach (var tileset in _tmxMap.Tilesets)
            {
                var gid = tileset.FirstGid;
                switch (tileset.Name)
                {
                    case TrapName:
                        {
                            foreach (var tile in tileset.Tiles)
                            {
                                _trapGidToIndex[gid + tile.Id] = int.Parse(Path.GetFileNameWithoutExtension(tile.Image.Source));
                            }

                            break;
                        }
                    case ObstacleName:
                        {
                            foreach (var tile in tileset.Tiles)
                            {
                                switch (Path.GetFileNameWithoutExtension(tile.Image.Source))
                                {
                                    case "跳透":
                                        _obstacleGidToType[gid + tile.Id] = CanOverTrans;
                                        break;
                                    case "跳障":
                                        _obstacleGidToType[gid + tile.Id] = CanOverObstacle;
                                        break;
                                    case "透":
                                        _obstacleGidToType[gid + tile.Id] = Trans;
                                        break;
                                    case "障":
                                        _obstacleGidToType[gid + tile.Id] = Obstacle;
                                        break;
                                }
                            }
                            break;
                        }
                }

                if (string.IsNullOrEmpty(tileset.Image.Source))
                {
                    foreach (var tile in tileset.Tiles)
                    {
                        var info = new TileTextureInfo();
                        _tileTextureInfos[gid] = info;
                        info.Gid = gid;
                        info.BelongTileset = tileset;
                        info.Texture = _allTextures[tile.Image.Source];
                        gid++;
                    }
                }
                else
                {
                    var columns = tileset.Columns.Value;
                    var count = 0;
                    var row = 0;
                    while (true)
                    {
                        for (var i = 0; i < columns; i++)
                        {
                            var info = new TileTextureInfo();
                            _tileTextureInfos[gid] = info;
                            info.Gid = gid;
                            info.BelongTileset = tileset;
                            info.Texture = _allTextures[tileset.Image.Source];
                            info.TextureSourceRegion = new Rectangle(
                                tileset.Margin + i * (tileset.Spacing + tileset.TileWidth),
                                tileset.Margin + row * (tileset.Spacing + tileset.TileHeight),
                                tileset.TileWidth,
                                tileset.TileHeight
                            );
                            gid++;
                            count++;
                        }
                        if (count == tileset.TileCount.Value) break;
                        row++;
                    }
                }
            }
        }

        private void LoadMapLayers()
        {
            foreach (var layer in _tmxMap.Layers)
            {
                switch (layer.Name)
                {
                    case Layer1Name:
                        _mapLayers[0] = layer;
                        break;
                    case Layer2Name:
                        _mapLayers[1] = layer;
                        break;
                    case Layer3Name:
                        _mapLayers[2] = layer;
                        break;
                    case TrapName:
                        _mapLayers[3] = layer;
                        _trapLayer = layer;
                        break;
                    case ObstacleName:
                        _mapLayers[4] = layer;
                        _obstacleLayer = layer;
                        break;
                    default:
                        Log.LogMessageToFile(string.Format("Tmx map layer [{0}] ingnored.", layer.Name));
                        break;
                }
            }
        }

        protected override void FreeInternal()
        {
            _allTextures.Clear();
            _trapGidToIndex.Clear();
            _obstacleGidToType.Clear();
            _tileTextureInfos = null;
            _mapLayers[0] = _mapLayers[1] = _mapLayers[2] = _mapLayers[3] = _mapLayers[4] = _trapLayer = _obstacleLayer = null;
            base.FreeInternal();
        }

        public override void DrawTrapLayer(SpriteBatch spriteBatch)
        {
            DrawLayer(spriteBatch, Color.White, 3);
        }

        public override void DrawObstacleLayer(SpriteBatch spriteBatch)
        {
            DrawLayer(spriteBatch, Color.White, 4);
        }
    }
}
