
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Engine.Map;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Engine
{
    public class MagicRegionFileReader
    {
        public class Item
        {
            public Vector2 Offset;
            public int Delay;

            public Item(Vector2 offset, int delay)
            {
                Offset = offset;
                Delay = delay;
            }
        }
        public class Layer
        {
            public int Width;
            public int Height;
            public int[] Data;

            public Layer(int width, int height, int[] data)
            {
                Width = width;
                Height = height;
                Data = data;
            }
        }

        public static Dictionary<int,List<Item>> Load(string filePath)
        {
            try
            {
                using (var reader = File.OpenText(filePath))
                {
                    return Load((JObject)JToken.ReadFrom(new JsonTextReader(reader)));
                }
            }
            catch (Exception e)
            {
                Log.LogFileLoadError("MagicRegionFile", filePath, e);
                return null;
            }
        }

        static Dictionary<int, List<Item>> Load(JObject o)
        {
            var regionInfo = new Dictionary<int, List<Item>>();
            var delayInfo = new Dictionary<int, List<int>>();
            var layers = new Dictionary<int, Layer>();

            var mapWidth = o.Value<int>("width");
            var mapHeight = o.Value<int>("height");

            foreach (JObject ts in o["tilesets"])
            {
                ReadDelayInfo(ts, delayInfo);
            }

            ReadLayers((JArray)o["layers"], layers);
            var beginPos = GetBeginTile(layers[8]);

            foreach (var key in layers.Keys)
            {
                if (key == 8)
                {
                    //At begin position setting layer
                    continue;
                }
                var layer = layers[key];
                var items = new List<Item>();
                FillItems(items, beginPos, layer, delayInfo);
                regionInfo.Add(key, items);
            }

            return regionInfo;
        }

        private static void FillItems(List<Item> items, Vector2 beginPos, Layer layer, Dictionary<int, List<int>> delayInfo)
        {
            var beginPixelPosition = MapBase.ToPixelPosition(beginPos, false);
            for (var h = 0; h < layer.Height; h++)
            {
                for (var w = 0; w < layer.Width; w++)
                {
                    var tileIndex = layer.Data[w + h*layer.Width];
                    if(tileIndex == 0) continue;
                    var offset = MapBase.ToPixelPosition(w, h, false) - beginPixelPosition;
                    if (delayInfo.ContainsKey(tileIndex))
                    {
                        items.AddRange(delayInfo[tileIndex].Select(delay => new Item(offset, delay)));
                    }
                    else
                    {
                        items.Add(new Item(offset, 0));
                    }
                }
            }
        }

        private static void ReadLayers(JArray layers,Dictionary<int, Layer> d)
        {
            foreach (var layer in layers)
            {
                var width = layer.Value<int>("width");
                var height = layer.Value<int>("height");
                var data = new int[width*height];
                var index = 0;
                foreach (var i in (JArray)layer["data"])
                {
                    data[index++] = i.Value<int>();
                }
                d.Add(layer["name"].Value<int>(), new Layer(width, height, data));
            }
        }

        static void ReadDelayInfo(JObject tilesets, Dictionary<int, List<int>> d)
        {
            var firstgid = tilesets.Value<int>("firstgid");
            JToken tiles;
            if (tilesets.TryGetValue("tiles", out tiles))
            {
                foreach (var tile in ((JObject)tiles).Properties())
                {
                    var indexName = tile.Name;
                    JToken value;
                    if (((JObject) tile.Value).TryGetValue("animation", out value))
                    {
                        var durations = new List<int>();
                        foreach (var animation in value)
                        {
                            durations.Add(animation.Value<int>("duration"));
                        }
                        d.Add(firstgid + int.Parse(indexName), durations);
                    }
                }
            }
        }

        static Vector2 GetBeginTile(Layer layer)
        {
            for (int h = 0; h < layer.Height; h++)
            {
                for (int w = 0; w < layer.Width; w++)
                {
                    if (layer.Data[w + h*layer.Width] > 0)
                    {
                        return new Vector2(w,h);
                    }
                }
            }
            return new Vector2(0, 0);
        }
    }
}
