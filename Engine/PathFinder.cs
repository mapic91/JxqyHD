using System;
using System.Collections.Generic;
using Engine.Benchmark;
using Microsoft.Xna.Framework;

namespace Engine
{
    public static class PathFinder
    {
        private static Vector2 _lastStartTile = Vector2.Zero;
        private static Vector2 _lastEndTile = Vector2.Zero;
        private static LinkedList<Vector2> _lastPath = null; 

        public enum PathType
        {
            PathOneStep,
            PerfectMaxNpcTry,
            PerfectMaxPlayerTry,
            PathStraightLine
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
        public static LinkedList<Vector2> FindPath(Character finder, Vector2 startTile, Vector2 endTile, PathType type)
        {
            switch (type)
            {
                case PathType.PathOneStep:
                    return FindPathStep(finder, startTile, endTile, 10);
                case PathType.PerfectMaxNpcTry:
                    return FindPathPerfect(finder, startTile, endTile, 100);
                case PathType.PerfectMaxPlayerTry:
                    return FindPathPerfect(finder, startTile, endTile, 2000);
                case PathType.PathStraightLine:
                    return GetLinePath(startTile, endTile);
            }
            return null;
        }

        //Returned path is in pixel position
        public static LinkedList<Vector2> FindPathStep(Character finder, Vector2 startTile, Vector2 endTile, int stepCount)
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
                if (finder.HasObstacle(current) && current != startTile) continue;
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
        public static LinkedList<Vector2> FindPathSimple(Character finder, Vector2 startTile, Vector2 endTile, int maxTry)
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
                if (finder.HasObstacle(current) && current != startTile) continue;
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

        public static LinkedList<Vector2> GetLinePath(Vector2 startTile, Vector2 endTile)
        {
            if (startTile == endTile) return null;

            var path = new LinkedList<Vector2>();
            var frontier = new C5.IntervalHeap<Node>();
            frontier.Add(new Node(startTile, 0f));
            while (!frontier.IsEmpty)
            {
                var current = frontier.DeleteMin().Location;
                path.AddLast(Map.ToPixelPosition(current));
                if (current == endTile) break;
                foreach (var neighbor in FindAllNeighbors(current))
                {
                    frontier.Add(new Node(neighbor, GetCost(neighbor, endTile)));
                }
            }
            return path;
        }

        public static int GetTileDistance(Vector2 startTile, Vector2 endTile)
        {
            if (startTile == endTile) return 0;

            var startX = (int) startTile.X;
            var startY = (int) startTile.Y;
            var endX = (int) endTile.X;
            var endY = (int) endTile.Y;

            if (endY%2 != startY%2)
            {
                //Start tile and end tile is not both at even row or odd row.
                //Move start tile position to make it at even row or odd row which is same as the end tile row.

                //Change row
                startY += ((endY < startY) ? 1 : -1);

                //Add some adjust to start tile column value
                if (endY%2 == 0)
                {
                    startX += ((endX > startX) ? 1 : 0);
                }
                else
                {
                    startX += ((endX < startX) ? -1 : 0);
                }
            }

            var offX = Math.Abs(startX - endX);
            var offY = Math.Abs(startY - endY)/2;


            return offX + offY;
        }

        /// <summary>
        /// Test whether can view target within vision radius.
        /// </summary>
        /// <param name="startTile">Viewer tile position.</param>
        /// <param name="endTile">Target tile position.</param>
        /// <param name="visionRadius">Viewr vision radius</param>
        /// <returns>True if can view target without map obstacle.Otherwise false.</returns>
        public static bool CanViewTarget(Vector2 startTile, Vector2 endTile, int visionRadius)
        {
            const int maxVisionRadious = 80;
            if (visionRadius > maxVisionRadious)
            {
                //Vision radius is too big, for performace reason return false.
                return false;
            }

            if (startTile != endTile)
            {
                if (Globals.TheMap.IsObstacleForMagic(endTile)) return false;

                var path = new LinkedList<Vector2>();
                var frontier = new C5.IntervalHeap<Node>();
                frontier.Add(new Node(startTile, 0f));
                while (!frontier.IsEmpty)
                {
                    var current = frontier.DeleteMin().Location;
                    if (current == endTile) return true;

                    if (Globals.TheMap.IsObstacle(current) || visionRadius < 0) return false;

                    path.AddLast(current);
                    foreach (var neighbor in FindAllNeighbors(current))
                    {
                        frontier.Add(new Node(neighbor, GetCost(neighbor, endTile)));
                    }
                    visionRadius--;
                }
                return false;
            }
            
            return true;
        }

        //Returned path is in pixel position
        public static LinkedList<Vector2> FindPathPerfect(Character finder, Vector2 startTile, Vector2 endTile, int maxTryCount)
        {
            if (startTile == endTile) return null;

            if (Globals.TheMap.IsObstacleForCharacter(endTile))
                return null;

            //last try can't find path, return null
            if (_lastPath == null &&
                _lastStartTile == startTile &&
                _lastEndTile == endTile)
            {
                return null;
            }
            //Use chached for performance
            _lastStartTile = startTile;
            _lastEndTile = endTile;

            var cameFrom = new Dictionary<Vector2, Vector2>();
            var costSoFar = new Dictionary<Vector2, float>();
            var frontier = new C5.IntervalHeap<Node>();

            frontier.Add(new Node(startTile, 0f));
            costSoFar[startTile] = 0f;

            var tryCount = 0; //For performance

            //Decrease max try count when fps low
            switch ((Fps.FpsValue+5)/10)
            {
                case 5:
                    maxTryCount /= 4;
                    break;
                case 4:
                    maxTryCount /= 8;
                    break;
                case 3:
                    maxTryCount /= 16;
                    break;
                case 2:
                    maxTryCount /= 32;
                    break;
                case 1:
                    maxTryCount = 10;
                    break;
                case 0:
                    maxTryCount = 0;
                    break;
            }

            while (!frontier.IsEmpty)
            {
                if (tryCount++ > maxTryCount) break;
                var current = frontier.DeleteMin().Location;
                if (current.Equals(endTile)) break;
                if (finder.HasObstacle(current) && current != startTile) continue;
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

            _lastPath = GetPath(cameFrom, startTile, endTile);
            return _lastPath;
        }

        /// <summary>
        /// Find neighbor in direction
        /// </summary>
        /// <param name="tilePosition">Tile position to find</param>
        /// <param name="direction">Vector direction</param>
        /// <returns>Return in tile postiion</returns>
        public static Vector2 FindNeighborInDirection(Vector2 tilePosition, Vector2 direction)
        {
            var neighbor = Vector2.Zero;
            if (direction != Vector2.Zero)
            {
                neighbor = FindAllNeighbors(tilePosition)[Utils.GetDirectionIndex(direction, 8)];
            }
            return neighbor;
        }

        /// <summary>
        /// Find tile in direction with tile distance from begin tile.
        /// </summary>
        /// <param name="tilePosition">Begin tile positon.</param>
        /// <param name="direction">Vector direction</param>
        /// <param name="tileDistance">Tile distance from begin tile.</param>
        /// <returns></returns>
        public static Vector2 FindDistanceTileInDirection(Vector2 tilePosition, Vector2 direction, int tileDistance)
        {
            if (direction == Vector2.Zero || tileDistance < 1)
            {
                return tilePosition;
            }

            var neighbor = tilePosition;
            for (var i = 0; i < tileDistance; i++)
            {
                neighbor = FindNeighborInDirection(neighbor,
                    direction);
            }

            return neighbor;
        }

        /// <summary>
        /// Find neighbor in direction(0-7)
        /// </summary>
        /// <param name="tilePosition">Tile position to find</param>
        /// <param name="direction">Direction: 0-7</param>
        /// <returns>Return in tile postiion</returns>
        public static Vector2 FindNeighborInDirection(Vector2 tilePosition, int direction)
        {
            if (direction < 0 || direction > 7) return Vector2.Zero;
            return FindAllNeighbors(tilePosition)[direction];
        }

        public static float GetCost(Vector2 fromTile, Vector2 toTile)
        {
            var fromPosition = Map.ToPixelPosition(fromTile);
            var toPosition = Map.ToPixelPosition(toTile);

            return Vector2.Distance(fromPosition, toPosition);
        }

        public static List<Vector2> FindNeighbors(Vector2 location)
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

        public static List<Vector2> FindAllNeighbors(Vector2 tilePosition)
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
