using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Engine
{
    public static class PathFinder
    {
        public enum PathType
        {
            PathOneStep,
            PerfectMaxTry100,
            PerfectMaxTry2000
        }

        private static LinkedList<Vector2> GetPath(Dictionary<Vector2, Vector2> cameFrom, Vector2 startTile,
            Vector2 endTile)
        {
            if (cameFrom.ContainsKey(endTile))
            {
                var path = new LinkedList<Vector2>();
                var current = endTile;
                path.AddFirst(Map.ToPixelPosition(current));
                while (current != startTile)
                {
                    current = cameFrom[current];
                    path.AddFirst(Map.ToPixelPosition(current));
                }
                return path;
            }
            return null;
        }

        //Returned path is in pixel position
        public static LinkedList<Vector2> FindPath(Vector2 startTile, Vector2 endTile, PathType type)
        {
            switch (type)
            {
                case PathType.PathOneStep:
                    return FindPathStep(startTile, endTile, 10);
                case PathType.PerfectMaxTry100:
                    return FindPathPerfect(startTile, endTile, 100);
                case PathType.PerfectMaxTry2000:
                    return FindPathPerfect(startTile, endTile, 2000);
            }
            return null;
        }

        //Returned path is in pixel position
        public static LinkedList<Vector2> FindPathStep(Vector2 startTile, Vector2 endTile, int stepCount)
        {
            if (startTile == endTile) return null;

            if (Globals.TheMap.IsObstacleForCharacter(endTile))
                return null;

            var cameFrom = new Dictionary<Vector2, Vector2>();
            var frontier = new C5.IntervalHeap<Node>();
            var path = new LinkedList<Vector2>();

            frontier.Add(new Node(startTile, 0f));
            var step = 0;
            Vector2 current = Vector2.Zero;
            while (!frontier.IsEmpty)
            {
                current = frontier.DeleteMin().Location;
                if (Map.HasNpcObjObstacleInMap(current) && current != startTile) continue;
                if (step++ > stepCount) break;
                if (current == endTile) break;
                foreach (var neighbor in FindNeighbors(current))
                {
                    if (!cameFrom.ContainsKey(neighbor))
                    {
                        var priority = GetCost(neighbor, endTile);
                        frontier.Add(new Node(neighbor, priority));
                        cameFrom[neighbor] = current;
                    }
                }
            }

            while (current != startTile)
            {
                path.AddFirst(Map.ToPixelPosition(current));
                current = cameFrom[current];
            }
            path.AddFirst(Map.ToPixelPosition(startTile));

            return path;
        }

        //Returned path is in pixel position
        public static LinkedList<Vector2> FindPathSimple(Vector2 startTile, Vector2 endTile, int maxTry)
        {
            if (startTile == endTile) return null;

            if (Globals.TheMap.IsObstacleForCharacter(endTile))
                return null;

            var cameFrom = new Dictionary<Vector2, Vector2>();
            var frontier = new C5.IntervalHeap<Node>();

            frontier.Add(new Node(startTile, 0f));
            var tryCount = 0;
            while (!frontier.IsEmpty)
            {
                if (tryCount++ > maxTry) break;
                var current = frontier.DeleteMin().Location;
                if (current == endTile) break;
                if (Map.HasNpcObjObstacleInMap(current) && current != startTile) continue;
                foreach (var neighbor in FindNeighbors(current))
                {
                    if (!cameFrom.ContainsKey(neighbor))
                    {
                        var priority = GetCost(neighbor, endTile);
                        frontier.Add(new Node(neighbor, priority));
                        cameFrom[neighbor] = current;
                    }
                }
            }
            return GetPath(cameFrom, startTile, endTile);
        }

        public static int GetTileDistance(Vector2 startTile, Vector2 endTile)
        {
            if (startTile == endTile) return 0;

            var frontier = new C5.IntervalHeap<Node>();
            frontier.Add(new Node(startTile, 0f));
            var count = -1;
            while (!frontier.IsEmpty)
            {
                var current = frontier.DeleteMin().Location;
                count++;
                if (current == endTile) return count;
                foreach (var neighbor in FindAllNeighbors(current))
                {
                    frontier.Add(new Node(neighbor, GetCost(neighbor, endTile)));
                }
            }
            return count;
        }

        //lineLength: the tile distance from startTile to endTile
        public static bool CanMagicReach(Vector2 startTile, Vector2 endTile, out int lineLength)
        {
            var finded = true;
            if (startTile != endTile)
            {
                var path = new LinkedList<Vector2>();
                var frontier = new C5.IntervalHeap<Node>();
                frontier.Add(new Node(startTile, 0f));
                while (!frontier.IsEmpty)
                {
                    var current = frontier.DeleteMin().Location;
                    if (Globals.TheMap.IsObstacle(current)) finded = false;
                    path.AddLast(current);
                    if (current == endTile) break;
                    foreach (var neighbor in FindAllNeighbors(current))
                    {
                        frontier.Add(new Node(neighbor, GetCost(neighbor, endTile)));
                    }
                }
                lineLength = path.Count - 1;
            }
            lineLength = 0;
            return finded;
        }

        //Returned path is in pixel position
        public static LinkedList<Vector2> FindPathPerfect(Vector2 startTile, Vector2 endTile, int maxTryCount)
        {
            if (startTile == endTile) return null;

            if (Globals.TheMap.IsObstacleForCharacter(endTile))
                return null;

            var cameFrom = new Dictionary<Vector2, Vector2>();
            var costSoFar = new Dictionary<Vector2, float>();
            var frontier = new C5.IntervalHeap<Node>();

            frontier.Add(new Node(startTile, 0f));
            costSoFar[startTile] = 0f;

            var tryCount = 0; //For performance
            while (!frontier.IsEmpty)
            {
                if (tryCount++ > maxTryCount) break;
                var current = frontier.DeleteMin().Location;
                if (current.Equals(endTile)) break;
                if (Map.HasNpcObjObstacleInMap(current) && current != startTile) continue;
                foreach (var next in FindNeighbors(current))
                {
                    var newCost = costSoFar[current] + GetCost(current, next);
                    if (!costSoFar.ContainsKey(next) ||
                        newCost < costSoFar[next])
                    {
                        costSoFar[next] = newCost;
                        var priority = newCost + GetCost(endTile, next);
                        frontier.Add(new Node(next, priority));
                        cameFrom[next] = current;
                    }
                }

            }

            return GetPath(cameFrom, startTile, endTile);
        }

        //Return in tile postiion
        public static Vector2 FindNeighborInDirection(Vector2 tilePosition, Vector2 direction)
        {
            var neighbor = Vector2.Zero;
            if (direction != Vector2.Zero)
            {
                neighbor = FindAllNeighbors(tilePosition)[Utils.GetDirectionIndex(direction, 8)];
            }
            return neighbor;
        }

        private static float GetCost(Vector2 fromTile, Vector2 toTile)
        {
            var fromPosition = Map.ToPixelPosition(fromTile);
            var toPosition = Map.ToPixelPosition(toTile);

            return Vector2.Distance(fromPosition, toPosition);
        }

        private static List<Vector2> FindNeighbors(Vector2 location)
        {
            var listAll = FindAllNeighbors(location);

            var list = new List<Vector2>();
            var removeList = new List<int>();
            var count = listAll.Count;
            for (var i = 0; i < count; i++)
            {
                if (Globals.TheMap.IsObstacleForCharacter(listAll[i]))
                {
                    removeList.Add(i);
                    if (Globals.TheMap.IsObstacle(listAll[i]))
                    {
                        switch (i)
                        {
                            case 1:
                                removeList.Add(0);
                                removeList.Add(2);
                                break;
                            case 3:
                                removeList.Add(2);
                                removeList.Add(4);
                                break;
                            case 5:
                                removeList.Add(4);
                                removeList.Add(6);
                                break;
                            case 7:
                                removeList.Add(0);
                                removeList.Add(6);
                                break;
                        }
                    }
                }
            }

            for (var j = 0; j < count; j++)
            {
                if (!removeList.Contains(j))
                    list.Add(listAll[j]);
            }

            return list;
        }

        private static List<Vector2> FindAllNeighbors(Vector2 tilePosition)
        {
            var list = new List<Vector2>();
            var x = tilePosition.X;
            var y = tilePosition.Y;
            // 3  4  5
            // 2     6
            // 1  0  7
            if ((int)y % 2 == 0)
            {
                list.Add(new Vector2(x, y + 2f));
                list.Add(new Vector2(x - 1f, y + 1f));
                list.Add(new Vector2(x - 1f, y));
                list.Add(new Vector2(x - 1f, y - 1f));
                list.Add(new Vector2(x, y - 2f));
                list.Add(new Vector2(x, y - 1f));
                list.Add(new Vector2(x + 1f, y));
                list.Add(new Vector2(x, y + 1f));
            }
            else
            {
                list.Add(new Vector2(x, y + 2f));
                list.Add(new Vector2(x, y + 1f));
                list.Add(new Vector2(x - 1f, y));
                list.Add(new Vector2(x, y - 1f));
                list.Add(new Vector2(x, y - 2f));
                list.Add(new Vector2(x + 1f, y - 1f));
                list.Add(new Vector2(x + 1f, y));
                list.Add(new Vector2(x + 1f, y + 1f));
            }

            return list;
        }
    }

    public struct Node : IComparable<Node>
    {
        public Vector2 Location;
        public float Priority;

        public Node(Vector2 location, float priority)
        {
            Location = location;
            Priority = priority;
        }

        public int CompareTo(Node other)
        {
            return this.Priority.CompareTo(other.Priority);
        }
    }
}
