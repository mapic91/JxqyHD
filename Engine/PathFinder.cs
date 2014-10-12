using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using C5;
using Microsoft.Xna.Framework;

namespace Engine
{
    public enum PathType
    {
        WalkRun,
        Jump
    }
    public static class PathFinder
    {
        //Returned path is in pixel position
        public static List<Vector2> FindPath(Vector2 startTile, Vector2 endTile, PathType type)
        {
            var path = new List<Vector2>();
            if (startTile == endTile) return path;

            if (type == PathType.Jump)
            {
                path.Add(Map.ToPixelPosition(startTile));
                path.Add(Map.ToPixelPosition(endTile));
                return path;
            }

            if (Globals.TheMap.IsObstacleForCharacter((int)endTile.X, (int)endTile.Y))
                return null;

            var cameFrom = new Dictionary<Vector2, Vector2>();
            var costSoFar = new Dictionary<Vector2, float>();
            var frontier = new IntervalHeap<Node>();

            frontier.Add(new Node(startTile, 0f));
            costSoFar[startTile] = 0f;

            var tryCount = 0; //For performance
            while (!frontier.IsEmpty)
            {
                if (tryCount++ > 2000) return null;
                var current = frontier.DeleteMin().Location;
                if (current.Equals(endTile)) break;
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

            if (cameFrom.ContainsKey(endTile))
            {
                var current = endTile;
                path.Add(Map.ToPixelPosition(current));
                while (current != startTile)
                {
                    current = cameFrom[current];
                    path.Insert(0, Map.ToPixelPosition(current));
                }
                return path;
            }

            return null;
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

        private static List<Vector2> FindAllNeighbors(Vector2 location)
        {
            var list = new List<Vector2>();
            var x = location.X;
            var y = location.Y;
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
