using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IniParser.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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

        /// <summary>
        /// Npc file name.
        /// </summary>
        public static string FileName
        {
            get { return _fileName; }
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

        private static void Save(string fileName = null, bool isSaveParter = false)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                if (isSaveParter || string.IsNullOrEmpty(_fileName))
                {
                    //Can't save without file name.
                    return;
                }
                fileName = _fileName;
            }
            var path = @"save\game\" + fileName;
            try
            {
                var count = 0;
                var data = new IniData();
                data.Sections.AddSection("Head");
                if (!isSaveParter)
                {
                    data["Head"].AddKey("Map",
                        Globals.TheMap.MapFileNameWithoutExtension + ".map");
                }

                var index = 0;
                foreach (var npc in _list)
                {
                    if ((isSaveParter && !npc.IsPartner) ||
                        (!isSaveParter && npc.IsPartner)) continue;
                    var sectionName = "NPC" + string.Format("{0:000}", index++);
                    data.Sections.AddSection(sectionName);
                    npc.Save(data[sectionName]);
                    count++;
                }
                data["Head"].AddKey("Count", count.ToString());
                File.WriteAllText(path, data.ToString(), Globals.SimpleChineseEncoding);
            }
            catch (Exception exception)
            {
                Log.LogFileSaveError("Npc", path, exception);
            }
        }

        /// <summary>
        /// Get enemy closed to target position.
        /// </summary>
        /// <param name="positionInWorld">Target position</param>
        /// <returns>If not find, return null</returns>
        public static Npc GetClosedEnemy(Vector2 positionInWorld)
        {
            Npc closed = null;
            var closedDistance = 99999999f;
            foreach (var npc in NpcManager.NpcList)
            {
                if (npc.IsEnemy)
                {
                    var distance = Vector2.Distance(positionInWorld, npc.PositionInWorld);
                    if (distance < closedDistance)
                    {
                        closed = npc;
                        closedDistance = distance;
                    }
                }
            }
            return closed;
        }

        /// <summary>
        /// Get player or fighter friend closed to target position.
        /// </summary>
        /// <param name="positionInWorld">Target position</param>
        /// <returns>If not find, return null</returns>
        public static Character GetClosedPlayerOrFighterFriend(Vector2 positionInWorld)
        {
            Character closed = null;
            var closedDistance = 99999999f;
            foreach (var npc in _list)
            {
                if (npc.IsFighterFriend)
                {
                    var distance = Vector2.Distance(positionInWorld, npc.PositionInWorld);
                    if (distance < closedDistance)
                    {
                        closed = npc;
                        closedDistance = distance;
                    }
                }
            }

            if (Vector2.Distance(positionInWorld, Globals.ThePlayer.PositionInWorld) <
                closedDistance)
            {
                closed = Globals.ThePlayer;
            }
            return closed;
        }

        /// <summary>
        /// Find neighbors of character.If not find returned list count is 0.
        /// </summary>
        /// <param name="character">The character to find</param>
        /// <returns>Finded neighbors.If not find list count is 0.</returns>
        public static List<Character> GetNeighborEnemy(Character character)
        {
            var list = new List<Character>();
            if (character == null) return list;
            var neighbors = PathFinder.FindAllNeighbors(character.TilePosition);
            foreach (var neighbor in neighbors)
            {
                var enemy = GetEnemy(neighbor);
                if (enemy != null)
                {
                    list.Add(enemy);
                }
            }
            return list;
        }

        public static void RemoveAllPartner()
        {
            for (var node = _list.First; node != null;)
            {
                var next = node.Next;
                if (node.Value.Kind == (int) Character.CharacterKind.Follower)
                {
                    DeleteNpc(node);
                }
                node = next;
            }
        }

        public static List<Npc> GetAllPartner()
        {
            var list = new List<Npc>();
            foreach (var npc in _list)
            {
                if (npc.Kind == (int) Character.CharacterKind.Follower)
                {
                    list.Add(npc);
                }
            }
            return list;
        }

        public static void PartnersMoveTo(Vector2 destinationTilePosition)
        {
            var partners = GetAllPartner();
            foreach (var partner in partners)
            {
                if (partner.IsStanding())
                {
                    partner.MoveTo(destinationTilePosition);
                }
            }
        }

        public static void LoadPartner(string filePath)
        {
            try
            {
                RemoveAllPartner();
                var list = Utils.GetAllKeyDataCollection(filePath, "NPC");
                foreach (var keyDataCollection in list)
                {
                    AddNpc(new Npc(keyDataCollection));
                }
            }
            catch (Exception exception)
            {
                Log.LogFileLoadError("Partner", filePath, exception);
            }
        }

        public static bool Load(string fileName, bool clearCurrentNpcs = true)
        {
            if (string.IsNullOrEmpty(fileName)) return false;
            var success = true;
            var filePath = Utils.GetNpcObjFilePath(fileName);
            var partners = new List<Npc>();
            try
            {
                if (clearCurrentNpcs)
                {
                    //get partners for restore later
                    partners = GetAllPartner();
                    ClearAllNpc();
                }
                _fileName = fileName;
                var list = Utils.GetAllKeyDataCollection(filePath, "NPC");
                foreach (var keyDataCollection in list)
                {
                    AddNpc(keyDataCollection);
                }
            }
            catch (Exception exception)
            {
                Log.LogFileLoadError("NPC", filePath, exception);
                success = false;
            }
            //restore partners
            foreach (var npc in partners)
            {
                AddNpc(npc);
            }
            return success;
        }

        public static void Merge(string fileName)
        {
            Load(fileName, false);
        }

        public static void AddNpc(KeyDataCollection keyDataCollection)
        {
            if(keyDataCollection == null) return;
            var npc = new Npc(keyDataCollection);
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

        public static Character GetPlayerOrFighterFriend(Vector2 tilePosition)
        {
            if (tilePosition == Globals.ThePlayer.TilePosition) return Globals.ThePlayer;
            foreach (var npc in _list)
            {
                if (npc.IsFighterFriend && npc.TilePosition == tilePosition)
                {
                    return npc;
                }
            }
            return null;
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

        public static List<Character> GetAllNpcs(string name)
        {
            return _list.Where(npc => npc.Name == name).Cast<Character>().ToList();
        }

        public static LinkedListNode<Npc> GetNpcNode(string name)
        {
            for (var node = _list.First; node != null; node = node.Next)
            {
                if (node.Value.Name == name) return node;
            }
            return null;
        }

        public static void DeleteNpc(string npcName)
        {
            for (var node = _list.First; node != null;)
            {
                var next = node.Next;
                if (node.Value.Name == npcName)
                {
                    DeleteNpc(node);
                }
                node = next;
            }
        }

        public static int GetNpcCount(int kind, int relation)
        {
            if (Globals.ThePlayer.Kind == kind &&
                Globals.ThePlayer.Relation == relation)
            {
                return 1;
            }
            return _list.Count(npc => npc.Kind == kind && npc.Relation == relation);
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

        public static void AllEnemyDie()
        {
            foreach (var npc in _list)
            {
                if (npc.IsEnemy)
                {
                    npc.Death();
                }
            }
        }

        public static void SaveNpc(string fileName = null)
        {
            Save(fileName);
        }

        public static void SavePartner(string fileName)
        {
            Save(fileName, true);
        }

        public static void Update(GameTime gameTime)
        {
            for (var node = _list.First; node != null; )
            {
                var npc = node.Value;
                var next = node.Next;
                npc.Update(gameTime);
                if (npc.IsDeath && npc.IsDeathScriptEnd)
                {
                    if (npc.IsBodyIniOk &&
                        !npc.IsNodAddBody)
                    {
                        npc.BodyIni.PositionInWorld = npc.PositionInWorld;
                        npc.BodyIni.CurrentDirection = npc.CurrentDirection;
                        ObjManager.AddObj(npc.BodyIni);
                    }
                    ObjManager.AddObj(GoodDrop.GetDropObj(npc));
                    DeleteNpc(node);
                }
                node = next;
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            foreach (var npc in _list)
                npc.Draw(spriteBatch);
        }

        /// <summary>
        /// All fighters cancle attack to attacking target.
        /// </summary>
        public static void CancleFighterAttacking()
        {
            foreach (var npc in _list)
            {
                if (npc.IsFighter)
                {
                    npc.CancleAttackTarget();
                }
            }
        }
    }
}
