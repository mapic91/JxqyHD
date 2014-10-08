using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using C5;
using Microsoft.Xna.Framework;

namespace Engine
{
    public static class PathFinder
    {
        public static List<Vector2> FindPath(Vector2 startTile, Vector2 endTile)
        {
            if (Globals.TheMap.IsObstacleForCharacter((int)endTile.X, (int)endTile.Y))
                return null;

            var path = new List<Vector2>();
            if (startTile == endTile) return path;

            var cameFrom = new Dictionary<Vector2, Vector2>();
            var costSoFar = new Dictionary<Vector2, float>();
            var frontier = new IntervalHeap<Node>();

            frontier.Add(new Node(startTile, 0f));
            costSoFar[startTile] = 0f;

            while (!frontier.IsEmpty)
            {
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
                path.Add(current);
                while (current != startTile)
                {
                    current = cameFrom[current];
                    path.Insert(0, current);
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
            var list = new List<Vector2>();
            var x = location.X;
            var y = location.Y;
            // 7  0  1
            // 6     2
            // 5  4  3
            if ((int) y%2 == 0)
            {
                list.Add(new Vector2(x, y - 2f));
                list.Add(new Vector2(x, y - 1f));
                list.Add(new Vector2(x + 1f, y));
                list.Add(new Vector2(x, y + 1f));
                list.Add(new Vector2(x, y + 2f));
                list.Add(new Vector2(x - 1f, y + 1f));
                list.Add(new Vector2(x - 1f, y));
                list.Add(new Vector2(x - 1f, y - 1f));
            }
            else
            {
                list.Add(new Vector2(x, y - 2f));
                list.Add(new Vector2(x + 1f, y - 1f));
                list.Add(new Vector2(x + 1f, y));
                list.Add(new Vector2(x + 1f, y + 1f));
                list.Add(new Vector2(x, y + 2f));
                list.Add(new Vector2(x, y + 1f));
                list.Add(new Vector2(x - 1f, y));
                list.Add(new Vector2(x, y - 1f));
            }

            var count = list.Count;
            for (var i = count - 1; i >= 0; i--)
            {
                if (Globals.TheMap.IsObstacleForCharacter(list[i]))
                {
                    list.RemoveAt(i);
                }
            }

            return list;
        }

        private static void AddIfNotExist(List<int> removeList, int value)
        {
            if(!removeList.Contains(value)) removeList.Add(value);
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
