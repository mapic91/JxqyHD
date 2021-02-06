using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Engine.Map;
using IniParser.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine
{
    static public class NpcManager
    {
        public class DeathInfo
        {
            public Character TheDead;
            public int LeftFrameToKeep;

            public DeathInfo(Character theDead, int leftFrameToKeep)
            {
                TheDead = theDead;
                LeftFrameToKeep = leftFrameToKeep;
            }
        }

        private static LinkedList<Npc> _list = new LinkedList<Npc>();
        private static LinkedList<Npc> _hideList = new LinkedList<Npc>();
        private static List<Npc> _npcInView = new List<Npc>();
        public static readonly LinkedList<DeathInfo> DeathInfos = new LinkedList<DeathInfo>();
        private static string _fileName;

        public static List<Npc> NpcsInView
        {
            get{return _npcInView;}
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
            if (!isSaveParter)
            {
                _fileName = fileName;
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
                        MapBase.MapFileName);
                }

                var index = 0;
                foreach (var npc in _list)
                {
                    if ((isSaveParter && !npc.IsPartner) ||
                        (!isSaveParter && npc.IsPartner) ||
                        npc.SummonedByMagicSprite != null) continue;
                    var sectionName = "NPC" + string.Format("{0:000}", index++);
                    data.Sections.AddSection(sectionName);
                    npc.Save(data[sectionName]);
                    count++;
                }
                foreach (var npc in _hideList)
                {
                    if ((isSaveParter && !npc.IsPartner) ||
                        (!isSaveParter && npc.IsPartner) ||
                        npc.SummonedByMagicSprite != null) continue;
                    var sectionName = "NPC" + string.Format("{0:000}", index++);
                    data.Sections.AddSection(sectionName);
                    npc.Save(data[sectionName]);
                    count++;
                }
                data["Head"].AddKey("Count", count.ToString());
                File.WriteAllText(path, data.ToString(), Globals.LocalEncoding);
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
        /// <param name="ignoreList">Ignore those character when finding</param>
        /// <returns>If not find, return null</returns>
        public static Character GetClosestEnemyTypeCharacter(Vector2 positionInWorld, bool withNetural, bool withInvisible, List<Character> ignoreList = null)
        {
            return GetClosestEnemyTypeCharacter(NpcList, positionInWorld, withNetural, withInvisible, ignoreList);
        }

        /// <summary>
        /// Get enemy closed to target position.
        /// </summary>
        /// <param name="list">Npc list</param>
        /// <param name="positionInWorld">Target position</param>
        /// <param name="ignoreList">Ignore those character when finding</param>
        /// <returns>If not find, return null</returns>
        public static Character GetClosestEnemyTypeCharacter(IEnumerable<Character> list, Vector2 positionInWorld, bool withNetural, bool withInvisible, List<Character> ignoreList = null)
        {
            Character closed = null;
            var closedDistance = 99999999f;
            foreach (var npc in list)
            {
                if ((ignoreList == null || ignoreList.All(item => npc != item)) && (withInvisible || npc.IsVisible) && (npc.IsEnemy ||(withNetural && npc.IsNoneFighter)))
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

        public static Character GetClosestFighter(Vector2 targetPositionInWorld, List<Character> ignoreList = null)
        {
            Character closed = null;
            var closedDistance = 99999999f;
            foreach (var npc in _list)
            {
                if ((ignoreList == null || ignoreList.All(item => npc != item)) && npc.IsFighter)
                {
                    var distance = Vector2.Distance(targetPositionInWorld, npc.PositionInWorld);
                    if (npc.IsDeathInvoked || !(distance < closedDistance)) continue;
                    closed = npc;
                    closedDistance = distance;
                }
            }

            var character = Globals.PlayerKindCharacter;
            if ((ignoreList == null || ignoreList.All(item => character != item)) &&
                !character.IsDeathInvoked &&
                Globals.ThePlayer.Kind == (int)Character.CharacterKind.Player &&
                Vector2.Distance(targetPositionInWorld, character.PositionInWorld) < closedDistance)
            {
                closed = Globals.ThePlayer;
            }
            return closed;
        }

        /// <summary>
        /// Get enemy relate to finder closed to targetPositionInWorld.
        /// </summary>
        /// <param name="finder">The finder.</param>
        /// <param name="targetPositionInWorld">Target position to begin find.</param>
        /// <param name="withNetural"></param>
        /// <param name="ignoreList">Ignore those character when finding</param>
        /// <returns></returns>
        public static Character GetClosestEnemy(Character finder, Vector2 targetPositionInWorld, bool withNetural, bool withInvisible, List<Character> ignoreList = null)
        {
            if (finder == null) return null;

            if (finder.IsEnemy)
            {
                var target = GetLiveClosestPlayerOrFighterFriend(targetPositionInWorld, withNetural, withInvisible, ignoreList);
                if(target == null)
                {
                    target = GetLiveClosestOtherGropEnemy(finder.Group, targetPositionInWorld);
                }
            }

            if (finder.IsPlayer || finder.IsFighterFriend)
            {
                return GetClosestEnemyTypeCharacter(targetPositionInWorld, withNetural, withInvisible, ignoreList);
            }

            return null;
        }

        public static Character GetLiveClosestOtherGropEnemy(int group, Vector2 positionInWorld)
        {
            Character closed = null;
            var closedDistance = 99999999f;
            foreach (var npc in _list)
            {
                if (npc.Group != group && npc.IsVisible && npc.IsEnemy)
                {
                    var distance = Vector2.Distance(positionInWorld, npc.PositionInWorld);
                    if (npc.IsDeathInvoked || !(distance < closedDistance)) continue;
                    closed = npc;
                    closedDistance = distance;
                }
            }
            return closed;
        }

        /// <summary>
        /// Get not death player or fighter friend closed to target position.
        /// </summary>
        /// <param name="positionInWorld">Target position</param>
        /// <param name="ignoreList">Ignore those character when finding</param>
        /// <returns>If not find, return null</returns>
        public static Character GetLiveClosestPlayerOrFighterFriend(Vector2 positionInWorld, bool withNetural, bool withInvisible, List<Character> ignoreList = null)
        {
            Character closed = null;
            var closedDistance = 99999999f;
            foreach (var npc in _list)
            {
                if ((ignoreList == null || ignoreList.All(item => npc != item)) && (withInvisible || npc.IsVisible) && (npc.IsFighterFriend || (withNetural && npc.IsNoneFighter)))
                {
                    var distance = Vector2.Distance(positionInWorld, npc.PositionInWorld);
                    if (npc.IsDeathInvoked || !(distance < closedDistance)) continue;
                    closed = npc;
                    closedDistance = distance;
                }
            }

            var character = Globals.PlayerKindCharacter;
            if ((ignoreList == null || ignoreList.All(item => character != item)) && (withInvisible || character.IsVisible) &&
                !character.IsDeathInvoked &&
                Vector2.Distance(positionInWorld, character.PositionInWorld) < closedDistance)
            {
                closed = Globals.ThePlayer;
            }
            return closed;
        }

        public static Character GetLiveClosestNonneturalFighter(Vector2 positionInWorld, List<Character> ignoreList = null)
        {
            Character closed = null;
            var closedDistance = 99999999f;
            foreach (var npc in _list)
            {
                if ((ignoreList == null || ignoreList.All(item => npc != item)) && (npc.IsFighter && npc.Relation != (int)Character.RelationType.None))
                {
                    var distance = Vector2.Distance(positionInWorld, npc.PositionInWorld);
                    if (npc.IsDeathInvoked || !(distance < closedDistance)) continue;
                    closed = npc;
                    closedDistance = distance;
                }
            }

            var character = Globals.PlayerKindCharacter;
            if ((ignoreList == null || ignoreList.All(item => character != item)) &&
                !character.IsDeathInvoked &&
                Vector2.Distance(positionInWorld, character.PositionInWorld) < closedDistance)
            {
                closed = Globals.ThePlayer;
            }
            return closed;
        }

        public static Character GetClosestCanInteractChracter(Vector2 findBeginTilePosition, int maxTileDistance = int.MaxValue)
        {
            var minDistance = (maxTileDistance == int.MaxValue ? maxTileDistance : maxTileDistance + 1);
            Character minNpc = null;
            foreach (var npc in _list)
            {
                if (npc.HasInteractScript && !npc.IsEnemy && npc.TilePosition != findBeginTilePosition)
                {
                    var tileDistance = PathFinder.GetViewTileDistance(findBeginTilePosition, npc.TilePosition);
                    if (tileDistance < minDistance || 
                        (tileDistance == minDistance && !npc.IsPartner) // find closest nonpartner npc first
                        )
                    {
                        minDistance = tileDistance;
                        minNpc = npc;
                    }
                }
                
            }
            return minNpc;
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
                var enemy = GetEnemy(neighbor, false);
                if (enemy != null)
                {
                    list.Add(enemy);
                }
            }
            return list;
        }

        public static List<Character> GetNeighborNuturalFighter(Character character)
        {
            var list = new List<Character>();
            if (character == null) return list;
            var neighbors = PathFinder.FindAllNeighbors(character.TilePosition);
            foreach (var neighbor in neighbors)
            {
                var fighter = GetNeutralFighter(neighbor);
                if (fighter != null)
                {
                    list.Add(fighter);
                }
            }
            return list;
        }

        /// <summary>
        /// Find enemys
        /// </summary>
        /// <param name="finder"></param>
        /// <param name="beginTilePosition">Range center.</param>
        /// <param name="tileDistance">The max tile distance form finder to enemy</param>
        /// <returns>Finded enemies. If can't find list size is 0.</returns>
        public static List<Character> FindEnemiesInTileDistance(Character finder, Vector2 beginTilePosition, int tileDistance)
        {
            var enemies = new List<Character>();
            if (finder == null || tileDistance < 1) return enemies;

            enemies.AddRange(_list.Where(npc => finder.IsOpposite(npc) && PathFinder.GetViewTileDistance(beginTilePosition, npc.TilePosition) <= tileDistance));

            if (finder.IsOpposite(Globals.ThePlayer) && PathFinder.GetViewTileDistance(beginTilePosition, Globals.ThePlayer.TilePosition) <= tileDistance)
            {
                enemies.Add(Globals.ThePlayer);
            }

            return enemies;
        }

        public static List<Character> FindFightersInTileDistance(Vector2 beginTilePosition, int tileDistance)
        {
            var fighters = new List<Character>();

            fighters.AddRange(_list.Where(npc => npc.IsFighter && PathFinder.GetViewTileDistance(beginTilePosition, npc.TilePosition) <= tileDistance));

            if (Globals.ThePlayer.Kind == (int)Character.CharacterKind.Player && PathFinder.GetViewTileDistance(beginTilePosition, Globals.ThePlayer.TilePosition) <= tileDistance)
            {
                fighters.Add(Globals.ThePlayer);
            }
            return fighters;
        }

        public static List<Character> FindFriendInTileDistance(Character finder, Vector2 beginTilePosition, int tileDistance)
        {
            var friends = new List<Character>();
            if (finder == null) return friends;

            if (tileDistance == 0)
            {
                friends.Add(finder);
                return friends;
            }

            if (finder.IsEnemy)
            {
                friends.AddRange(_list.Where(npc =>  npc.IsEnemy &&
                    PathFinder.GetViewTileDistance(beginTilePosition, npc.TilePosition) <= tileDistance));
            }
            else if (finder.IsPlayer || finder.IsFighterFriend)
            {
                friends.AddRange(_list.Where(npc => npc.IsFighterFriend &&
                    PathFinder.GetViewTileDistance(beginTilePosition, npc.TilePosition) <= tileDistance));
                if (finder == Globals.ThePlayer &&
                    PathFinder.GetViewTileDistance(beginTilePosition, Globals.ThePlayer.TilePosition) <= tileDistance)
                {
                    friends.Add(Globals.ThePlayer);
                }
            }
            else if (finder.IsNoneFighter)
            {
                friends.AddRange(_list.Where(npc => npc.IsNoneFighter &&
                                                    PathFinder.GetViewTileDistance(beginTilePosition, npc.TilePosition) <= tileDistance));
            }
            return friends;
        } 

        public static void RemoveAllPartner()
        {
            for (var node = _list.First; node != null; )
            {
                var next = node.Next;
                if (node.Value.Kind == (int)Character.CharacterKind.Follower)
                {
                    DeleteNpc(node);
                }
                node = next;
            }
            for (var node = _hideList.First; node != null;)
            {
                var next = node.Next;
                if (node.Value.Kind == (int)Character.CharacterKind.Follower)
                {
                    _hideList.Remove(node);
                }
                node = next;
            }
        }

        public static List<Npc> GetAllPartner()
        {
            var list = new List<Npc>();
            foreach (var npc in _list)
            {
                if (npc.IsPartner)
                {
                    list.Add(npc);
                }
            }
            return list;
        }

        /// <summary>
        /// Clear all npcs follow target if equal target.
        /// </summary>
        /// <param name="target">Target be cleared</param>
        public static void CleartFollowTargetIfEqual(Character target)
        {
            foreach (var npc in _list)
            {
                if (npc.FollowTarget == target)
                {
                    npc.ClearFollowTarget();
                }
            }
        }

        public static void PartnersMoveTo(Vector2 destinationTilePosition)
        {
            var partners = GetAllPartner();
            foreach (var partner in partners)
            {
                if (partner.IsStanding())
                {
                    partner.PartnerMoveTo(destinationTilePosition);
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
            var success = true;
            var filePath = Utils.GetNpcObjFilePath(fileName);
            try
            {
                if (clearCurrentNpcs)
                {
                    //keep partners
                    ClearAllNpcAndKeepPartner();
                }
                _fileName = fileName;
                if (!string.IsNullOrEmpty(fileName))
                {
                    var list = Utils.GetAllKeyDataCollection(filePath, "NPC");
                    foreach (var keyDataCollection in list)
                    {
                        AddNpc(keyDataCollection);
                    }
                }
            }
            catch (Exception exception)
            {
                Log.LogFileLoadError("NPC", filePath, exception);
                success = false;
            }
            return success;
        }

        public static void Merge(string fileName)
        {
            Load(fileName, false);
        }

        public static void AddNpc(KeyDataCollection keyDataCollection)
        {
            if (keyDataCollection == null) return;
            var npc = new Npc(keyDataCollection);
            AddNpc(npc);
        }

        public static void AddNpc(Npc npc)
        {
            if (npc != null)
            {
                _list.AddLast(npc);
            }
        }

        public static Npc AddNpc(string fileName, int tileX, int tileY, int direction)
        {
            var path = @"ini\npc\" + fileName;
            var npc = new Npc(path);
            npc.TilePosition = new Vector2(tileX, tileY);
            npc.SetDirection(direction);
            AddNpc(npc);
            return npc;
        }

        public static void ClearAllNpc(bool keepPartner = false)
        {
            _fileName = string.Empty;
            if (keepPartner)
            {
                for (var node = _list.First; node != null;)
                {
                    var npc = node.Value;
                    var next = node.Next;

                    if (!npc.IsPartner)
                    {
                        _list.Remove(node);
                    }
                    node = next;
                }
                for (var node = _hideList.First; node != null;)
                {
                    var npc = node.Value;
                    var next = node.Next;

                    if (!npc.IsPartner)
                    {
                        _hideList.Remove(node);
                    }
                    node = next;
                }
            }
            else
            {
                _list.Clear();
                _hideList.Clear();
            }
            DeathInfos.Clear();
        }

        public static void ClearAllNpcAndKeepPartner()
        {
            ClearAllNpc(true);
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

        public static Npc GetEnemy(int tileX, int tileY, bool withNeutral)
        {
            foreach (var npc in _list)
            {
                if (npc.MapX == tileX && npc.MapY == tileY)
                {
                    if (npc.IsEnemy || (withNeutral && npc.IsNoneFighter))
                    {
                        return npc;
                    }
                }
            }
            return null;
        }

        public static Character GetEventer(Vector2 tilePosition)
        {
            return _list.FirstOrDefault(npc => npc.IsEventCharacter && npc.TilePosition == tilePosition);
        }

        /// <summary>
        /// Get character who can fight.
        /// </summary>
        /// <param name="tilePosition"></param>
        /// <returns></returns>
        public static Character GetFighter(Vector2 tilePosition)
        {
            if (Globals.ThePlayer.Kind == (int)Character.CharacterKind.Player && Globals.ThePlayer.TilePosition == tilePosition) return Globals.ThePlayer;
            foreach (var npc in _list)
            {
                if (npc.IsFighter)
                {
                    if (npc.TilePosition == tilePosition) return npc;
                }
            }

            return null;
        }

        public static Npc GetEnemy(Vector2 tilePosition, bool withNeutral)
        {
            return GetEnemy((int)tilePosition.X, (int)tilePosition.Y, withNeutral);
        }

        public static Npc GetNeutralFighter(Vector2 tilePosotion)
        {
            foreach (var npc in _list)
            {
                if (npc.TilePosition == tilePosotion)
                {
                    if (npc.IsNoneFighter)
                    {
                        return npc;
                    }
                }
            }
            return null;
        }

        public static Character GetPlayerOrFighterFriend(Vector2 tilePosition, bool withNetural)
        {
            if (tilePosition == Globals.PlayerTilePosition) return Globals.PlayerKindCharacter;
            foreach (var npc in _list)
            {
                if (npc.TilePosition == tilePosition)
                {
                    if(npc.IsFighterFriend || (withNetural && npc.IsNoneFighter))
                    return npc;
                }
            }
            return null;
        }

        public static Character GetOtherGropEnemy(int group, Vector2 tilePosition)
        {
            foreach (var npc in _list)
            {
                if (npc.TilePosition == tilePosition)
                {
                    if (npc.Group != group && npc.IsEnemy)
                        return npc;
                }
            }
            return null;
        }

        public static Character GetNonneutralFighter(Vector2 tilePosition)
        {
            if (tilePosition == Globals.PlayerTilePosition) return Globals.PlayerKindCharacter;
            foreach (var npc in _list)
            {
                if (npc.TilePosition == tilePosition && npc.IsFighter && npc.Relation != (int)Character.RelationType.None)
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
            for (var node = _list.First; node != null; )
            {
                var next = node.Next;
                if (node.Value.Name == npcName &&
                    !node.Value.IsPartner)
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
                if (npc.IsEnemy && !(npc.IsInDeathing || npc.IsDeath))
                {
                    npc.Death();
                    Globals.ThePlayer.AddExp(
                        Utils.GetCharacterDeathExp(Globals.ThePlayer, npc), true);
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
            for (var node = _hideList.First; node != null;)
            {
                var npc = node.Value;
                var next = node.Next;
                npc.Update(gameTime);
                if (npc.IsVisibleByVariable)
                {
                    _list.AddLast(npc);
                    _hideList.Remove(node);
                }
                node = next;
            }
            for (var node = _list.First; node != null; )
            {
                var npc = node.Value;
                var next = node.Next;
                npc.Update(gameTime);
                if (npc.IsDeath)
                {
                    if (npc.IsBodyIniOk &&
                        !npc.IsNodAddBody &&
                        npc.SummonedByMagicSprite == null) //Not summoned npc
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
            for (var node = _list.First; node != null;)
            {
                var npc = node.Value;
                var next = node.Next;
                if (!npc.IsVisibleByVariable)
                {
                    _list.Remove(node);
                    _hideList.AddLast(npc);
                }
                node = next;
            }

            for (var node = DeathInfos.First; node != null;)
            {
                var info = node.Value;
                var next = node.Next;
                info.LeftFrameToKeep--;
                if (info.LeftFrameToKeep <= 0)
                {
                    DeathInfos.Remove(node);
                }
                node = next;
            }
        }

        public static void UpdateNpcsInView()
        {
            //Update the list of npcs in view.
            _npcInView = GetNpcsInView();
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
                if (npc.IsFighterKind)
                {
                    npc.CancleAttackTarget();
                }
            }
        }

        public static Character GetPlayerKindCharacter()
        {
            return _list.FirstOrDefault(npc => npc.Kind == (int)Character.CharacterKind.Player);
        }

        public static void AddDead(Character dead)
        {
            DeathInfos.AddLast(new DeathInfo(dead, 2));
        }

        public static Character FindFriendDeadKilledByLiveCharacter(Character finder, int maxTileDistance)
        {
            foreach (var deadInfo in DeathInfos)
            {
                if (Engine.PathFinder.GetViewTileDistance(finder.TilePosition, deadInfo.TheDead.TilePosition) <=
                    maxTileDistance)
                {
                    if (deadInfo.TheDead.LastAttackerMagicSprite != null &&
                        deadInfo.TheDead.LastAttackerMagicSprite.BelongCharacter != null &&
                        !deadInfo.TheDead.LastAttackerMagicSprite.BelongCharacter.IsDeathInvoked &&
                        ((finder.IsEnemy && deadInfo.TheDead.IsEnemy) ||
                        (finder.IsFighterFriend && deadInfo.TheDead.IsFighterFriend)))
                    {
                        return deadInfo.TheDead;
                    }
                }
            }
            return null;
        }
    }
}
