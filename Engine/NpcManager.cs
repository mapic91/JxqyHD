using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using IniParser.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Engine
{
    static public class NpcManager
    {
        private static LinkedList<Npc> _list = new LinkedList<Npc>();
        private static List<Npc> _npcInView = new List<Npc>();
        private static Rectangle _lastViewRegion;
        private static bool _npcListChanged = true;
        private static string _fileName;

        public static List<Npc> NpcsInView
        {
            get
            {
                if (!_lastViewRegion.Equals(Globals.TheCarmera.CarmerRegionInWorld) ||
                    _npcListChanged)
                {
                    _npcInView = GetNpcsInView();
                    _lastViewRegion = Globals.TheCarmera.CarmerRegionInWorld;
                }
                return _npcInView;
            }
        }

        public static LinkedList<Npc> NpcList
        {
            get { return _list; }
        }

        private static List<Npc> GetNpcsInView()
        {
            var viewRegion = Globals.TheCarmera.CarmerRegionInWorld;
            var list = new List<Npc>(_list.Count);
            foreach (var npc in _list)
            {
                if (viewRegion.Intersects(npc.RegionInWorld))
                    list.Add(npc);
            }
            return list;
        }

        private static void DeleteNpc(LinkedListNode<Npc> node)
        {
            _list.Remove(node);
            _npcListChanged = true;
        }

        public static Npc GetClosedEnemy(Vector2 position)
        {
            Npc closedNpc = null;
            var closedDistance = 99999999f;
            foreach (var npc in NpcManager.NpcList)
            {
                if (npc.IsEnemy)
                {
                    var distance = Vector2.Distance(position, npc.PositionInWorld);
                    if (distance < closedDistance)
                    {
                        closedNpc = npc;
                        closedDistance = distance;
                    }
                }
            }
            return closedNpc;
        }

        public static bool Load(string fileName)
        {
            try
            {
                _fileName = fileName;
                var filePath = Utils.GetNpcObjFilePath(fileName);
                var lines = File.ReadAllLines(filePath, Globals.SimpleChinaeseEncoding);
                Load(lines);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        public static bool Load(string[] lines)
        {
            ClearAllNpc();

            var count = lines.Count();
            for (var i = 0; i < count; )
            {
                var groups = Regex.Match(lines[i++], @"\[NPC([0-9]+)\]").Groups;
                if (groups[0].Success)
                {
                    var contents = new List<string>();
                    while (i < count && !string.IsNullOrEmpty(lines[i]))
                    {
                        contents.Add(lines[i]);
                        i++;
                    }
                    AddNpc(contents.ToArray());
                    i++;
                }
            }
            return true;
        }

        public static void AddNpc(string[] lines)
        {
            var npc = new Npc();
            npc.Load(lines);
            AddNpc(npc);
        }

        public static void AddNpc(Npc npc)
        {
            if (npc != null)
            {
                _list.AddLast(npc);
                _npcListChanged = true;
            }
        }

        public static void AddNpc(string fileName, int tileX, int tileY, int direction)
        {
            var path = @"ini\npc\" + fileName;
            var npc = new Npc(path);
            npc.TilePosition = new Vector2(tileX, tileY);
            npc.SetDirection(direction);
            AddNpc(npc);
        }

        public static void ClearAllNpc()
        {
            _list.Clear();
            _npcListChanged = true;
        }

        public static bool IsEnemy(int tileX, int tileY)
        {
            foreach (var npc in _list)
            {
                if (npc.MapX == tileX && npc.MapY == tileY && npc.IsEnemy)
                    return true;
            }
            return false;
        }

        public static bool IsEnemy(Vector2 tilePosition)
        {
            return IsEnemy((int)tilePosition.X, (int)tilePosition.Y);
        }

        public static Npc GetEnemy(int tileX, int tileY)
        {
            foreach (var npc in _list)
            {
                if (npc.MapX == tileX && npc.MapY == tileY && npc.IsEnemy)
                    return npc;
            }
            return null;
        }

        public static Npc GetEnemy(Vector2 tilePosition)
        {
            return GetEnemy((int)tilePosition.X, (int)tilePosition.Y);
        }

        public static bool IsObstacle(int tileX, int tileY)
        {
            foreach (var npc in _list)
            {
                if (npc.MapX == tileX && npc.MapY == tileY && npc.IsObstacle)
                    return true;
            }
            return false;
        }

        public static bool IsObstacle(Vector2 tilePosition)
        {
            return IsObstacle((int)tilePosition.X, (int)tilePosition.Y);
        }

        //just check npcs in view
        public static bool IsObstacleInView(int tileX, int tileY)
        {
            foreach (var npc in NpcsInView)
            {
                if (npc.MapX == tileX && npc.MapY == tileY && npc.IsObstacle)
                    return true;
            }
            return false;
        }

        //just check npcs in view
        public static bool IsObstacleInView(Vector2 tilePosition)
        {
            return IsObstacleInView((int)tilePosition.X, (int)tilePosition.Y);
        }

        public static Npc GetObstacle(int tileX, int tileY)
        {
            foreach (var npc in _list)
            {
                if (npc.MapX == tileX && npc.MapY == tileY && npc.IsObstacle)
                    return npc;
            }
            return null;
        }

        public static Npc GetObstacle(Vector2 tilePosition)
        {
            return GetObstacle((int)tilePosition.X, (int)tilePosition.Y);
        }

        public static Npc GetNpc(string name)
        {
            return _list.FirstOrDefault(npc => npc.Name == name);
        }

        public static void DeleteNpc(string npcName)
        {
            for (var node = _list.First; node != null; node = node.Next)
            {
                if (node.Value.Name == npcName)
                {
                    DeleteNpc(node);
                    break;
                }
            }
        }

        public static void ShowNpc(string npcName, bool isShow = true)
        {
            Character character = null;
            if (Globals.ThePlayer != null &&
                Globals.ThePlayer.Name == npcName)
            {
                character = Globals.ThePlayer;
            }
            else
            {
                foreach (var npc in _list)
                {
                    if (npc.Name == npcName)
                    {
                        character = npc;
                    }
                }
            }
            if (character != null)
            {
                character.IsHide = !isShow;
            }
        }

        public static void Save(string fileName = null)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                fileName = _fileName;
            }
            var path = @"save\game\" + fileName;
            try
            {
                var count = _list.Count;
                var data = new IniData();
                data.Sections.AddSection("Head");
                data["Head"].AddKey("Map", 
                    Globals.TheMap.MapFileNameWithoutExtension + ".map");
                data["Head"].AddKey("Count", count.ToString());

                var node = _list.First;
                for (var i = 0; i < count; i++, node = node.Next)
                {
                    var sectionName = "NPC" + string.Format("{0:000}", i);
                    data.Sections.AddSection(sectionName);
                    var npc = node.Value;
                    npc.Save(data[sectionName]);
                }
                File.WriteAllText(path, data.ToString(), Globals.SimpleChinaeseEncoding);
            }
            catch (Exception exception)
            {
                Log.LogFileSaveError("Npc", path, exception);
            }
        }

        public static void Update(GameTime gameTime)
        {
            for (var node = _list.First; node != null; )
            {
                var npc = node.Value;
                var next = node.Next;
                npc.Update(gameTime);
                if (npc.IsDeath) DeleteNpc(node);
                node = next;
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            foreach (var npc in _list)
                npc.Draw(spriteBatch);
        }
    }
}
