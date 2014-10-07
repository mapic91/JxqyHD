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
                var path = new List<Vector2>();
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
            var listAll = new Dictionary<int, Vector2>();
            var x = location.X;
            var y = location.Y;
            // 7  0  1
            // 6     2
            // 5  4  3
            if ((int) y%2 == 0)
            {
                listAll[0] = (new Vector2(x, y - 2f));
                listAll[1] = (new Vector2(x, y - 1f));
                listAll[2] = (new Vector2(x + 1f, y));
                listAll[3] = (new Vector2(x, y + 1f));
                listAll[4] = (new Vector2(x, y + 2f));
                listAll[5] = (new Vector2(x - 1f, y + 1f));
                listAll[6] = (new Vector2(x - 1f, y));
                listAll[7] = (new Vector2(x - 1f, y - 1f));
            }
            else
            {
                listAll[0] = (new Vector2(x, y - 2f));
                listAll[1] = (new Vector2(x + 1f, y - 1f));
                listAll[2] = (new Vector2(x + 1f, y));
                listAll[3] = (new Vector2(x + 1f, y + 1f));
                listAll[4] = (new Vector2(x, y + 2f));
                listAll[5] = (new Vector2(x, y + 1f));
                listAll[6] = (new Vector2(x - 1f, y));
                listAll[7] = (new Vector2(x, y - 1f));
            }

            var list = new List<Vector2>();
            var removeList = new List<int>();
            var count = listAll.Count;
            for (var i = 0; i < count; i++)
            {
                if (Globals.TheMap.IsObstacleForCharacter(listAll[i]))
                {
                    AddIfNotExist(removeList, i);
                    switch (i)
                    {
                        case 1:
                            AddIfNotExist(removeList, 0);
                            AddIfNotExist(removeList, 2);
                            break;
                        case 3:
                            AddIfNotExist(removeList, 2);
                            AddIfNotExist(removeList, 4);
                            break;
                        case 5:
                            AddIfNotExist(removeList, 4);
                            AddIfNotExist(removeList, 6);
                            break;
                        case 7:
                            AddIfNotExist(removeList, 0);
                            AddIfNotExist(removeList, 6);
                            break;
                    }

                }
            }

            for (var j = 0; j < count; j++)
            {
                if(!removeList.Contains(j))
                    list.Add(listAll[j]);
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
