using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

        private static void RemoveNpc(LinkedListNode<Npc> node)
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

        public static bool Load(string filePath)
        {
            try
            {
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
            _list.AddLast(npc);
            _npcListChanged = true;
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
                    RemoveNpc(node);
                    break;
                }
            }
        }

        public static void Update(GameTime gameTime)
        {
            for (var node = _list.First; node != null; )
            {
                var npc = node.Value;
                var next = node.Next;
                npc.Update(gameTime);
                if (npc.IsDeath) RemoveNpc(node);
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
