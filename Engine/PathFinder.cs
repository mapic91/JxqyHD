using System;
using System.Collections.Generic;
using System.IO;
using Engine.Benchmark;
using Microsoft.Xna.Framework;

namespace Engine
{
    public static class PathFinder
    {
        public enum PathType
        {
            PathOneStep,
            SimpleMaxNpcTry,
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

        public static bool HasObstacle(Character finder, Vector2 tilePosition)
        {
            return (finder.HasObstacle(tilePosition) ||
                    Globals.TheMap.IsObstacleForCharacter(tilePosition));
        }

        /// <summary>
        /// Temporary disable path find max try restrict.
        /// Used once.
        /// </summary>
        public static bool TemporaryDisableRestrict;

        //Returned path is in pixel position
        public static LinkedList<Vector2> FindPath(Character finder, Vector2 startTile, Vector2 endTile, PathType type)
        {
            LinkedList<Vector2> path = null;
            switch (type)
            {
                case PathType.PathOneStep:
                    path = FindPathStep(finder, startTile, endTile, 10);
                    break;
                case PathType.SimpleMaxNpcTry:
                    path = FindPathSimple(finder, startTile, endTile, 100);
                    break;
                case PathType.PerfectMaxNpcTry:
                    path = FindPathPerfect(finder, startTile, endTile, 100);
                    break;
                case PathType.PerfectMaxPlayerTry:
                    path = FindPathPerfect(finder, startTile, endTile, TemporaryDisableRestrict ? -1 : 500);
                    break;
                case PathType.PathStraightLine:
                    path = GetLinePath(startTile, endTile, 100);
                    break;
            }
            TemporaryDisableRestrict = false;//Used once
            return path;
        }

        //Returned path is in pixel position
        public static LinkedList<Vector2> FindPathStep(Character finder, Vector2 startTile, Vector2 endTile, int stepCount)
        {
            if (finder == null) return null;
            if (startTile == endTile) return null;

            if (Globals.TheMap.IsObstacleForCharacter(endTile))
                return null;

            var path = new LinkedList<Vector2>();
            var visted = new LinkedList<Vector2>();

            var endPositon = Map.ToPixelPosition(endTile);
            path.AddLast(Map.ToPixelPosition(startTile));
            var current = startTile;
            var maxTry = 100;// For performance
            while (maxTry-- > 0)
            {
                var direction = Utils.GetDirectionIndex(endPositon - Map.ToPixelPosition(current), 8);
                var neighbors = FindAllNeighbors(current);
                var removeList = GetObstacleIndexList(neighbors);
                var index = -1;
                var list = new int[]
                {
                    direction,
                    (direction + 1)%8, (direction + 8 - 1)%8,
                    (direction + 2)%8, (direction + 8 - 2)%8,
                    (direction + 3)%8, (direction + 8 - 3)%8,
                    (direction + 4)%8
                };
                for (var i = 0; i < 8; i++)
                {
                    var position = neighbors[list[i]];
                    if (removeList.Contains(list[i]) ||
                        HasObstacle(finder, position) ||
                            visted.Contains(position))
                    {
                        continue;
                    }
                    index = list[i];
                    break;
                }
                if (index == -1)
                {
                    break;
                }
                current = neighbors[index];
                path.AddLast(Map.ToPixelPosition(current));
                visted.AddLast(current);

                if (path.Count > stepCount || current == endTile)
                {
                    break;
                }
            }

            return path.Count < 2 ? null : path;;
        }

        //Returned path is in pixel position
        public static LinkedList<Vector2> FindPathSimple(Character finder, Vector2 startTile, Vector2 endTile, int maxTry)
        {
            if (startTile == endTile) return null;

            if (Globals.TheMap.IsObstacleForCharacter(endTile))
                return null;

            var cameFrom = new Dictionary<Vector2, Vector2>();
            var frontier = new C5.IntervalHeap<Node<Vector2>>();

            frontier.Add(new Node<Vector2>(startTile, 0f));
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
                        var priority = GetTilePositionCost(neighbor, endTile);
                        frontier.Add(new Node<Vector2>(neighbor, priority));
                        cameFrom[neighbor] = current;
                    }
                }
            }
            return GetPath(cameFrom, startTile, endTile);
        }

        /// <summary>
        /// Get line path without care obstacle.
        /// </summary>
        /// <param name="startTile">Start tile positon</param>
        /// <param name="endTile">End tile positon</param>
        /// <param name="maxTry">Max strep</param>
        /// <param name="tilePositon">If true, path position in path list is tile position.Otherwise is postion in world.</param>
        /// <returns>Path positition in world list.</returns>
        public static LinkedList<Vector2> GetLinePath(Vector2 startTile, Vector2 endTile, int maxTry, bool tilePositon = false)
        {
            if (startTile == endTile) return null;

            var path = new LinkedList<Vector2>();
            var frontier = new C5.IntervalHeap<Node<Vector2>>();
            frontier.Add(new Node<Vector2>(startTile, 0f));
            while (!frontier.IsEmpty)
            {
                if(maxTry-- < 0) break;
                var current = frontier.DeleteMin().Location;
                path.AddLast(tilePositon? current : Map.ToPixelPosition(current));
                if (current == endTile) break;
                foreach (var neighbor in FindAllNeighbors(current))
                {
                    frontier.Add(new Node<Vector2>(neighbor, GetTilePositionCost(neighbor, endTile)));
                }
            }
            return path;
        }

        public static bool HasMapObstacalInTilePositionList(LinkedList<Vector2> tilePositionList)
        {
            if (tilePositionList == null) return true;
            foreach (var tilePostion in tilePositionList)
            {
                if (Globals.TheMap.IsObstacleForCharacter(tilePostion))
                {
                    return true;
                }
            }
            return false;
        }

        private static void GetTileDistanceOff(Vector2 startTile, Vector2 endTile, out int offX, out int offY)
        {
            offX = 0;
            offY = 0;
            if (startTile == endTile) return;

            var startX = (int)startTile.X;
            var startY = (int)startTile.Y;
            var endX = (int)endTile.X;
            var endY = (int)endTile.Y;

            if (endY % 2 != startY % 2)
            {
                //Start tile and end tile is not both at even row or odd row.
                //Move start tile position to make it at even row or odd row which is same as the end tile row.

                //Change row
                startY += ((endY < startY) ? 1 : -1);

                //Add some adjust to start tile column value
                if (endY % 2 == 0)
                {
                    startX += ((endX > startX) ? 1 : 0);
                }
                else
                {
                    startX += ((endX < startX) ? -1 : 0);
                }
            }

            offX = Math.Abs(startX - endX);
            offY = Math.Abs(startY - endY) / 2;
        }

        private static int GetPathTileDistance(Vector2 startTile, Vector2 endTile)
        {
            int offX, offY;
            GetTileDistanceOff(startTile, endTile, out offX, out offY);

            return offX + offY;
        }

        public static int GetViewTileDistance(Vector2 startTile, Vector2 endTile)
        {
            int offX, offY;
            GetTileDistanceOff(startTile, endTile, out offX, out offY);

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
                var frontier = new C5.IntervalHeap<Node<Vector2>>();
                frontier.Add(new Node<Vector2>(startTile, 0f));
                while (!frontier.IsEmpty)
                {
                    var current = frontier.DeleteMin().Location;
                    if (current == endTile) return true;

                    if (Globals.TheMap.IsObstacle(current) || visionRadius < 0) return false;

                    path.AddLast(current);
                    foreach (var neighbor in FindAllNeighbors(current))
                    {
                        frontier.Add(new Node<Vector2>(neighbor, GetTilePositionCost(neighbor, endTile)));
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

            var cameFrom = new Dictionary<Vector2, Vector2>();
            var costSoFar = new Dictionary<Vector2, float>();
            var frontier = new C5.IntervalHeap<Node<Vector2>>();

            frontier.Add(new Node<Vector2>(startTile, 0f));
            costSoFar[startTile] = 0f;

            var tryCount = 0; //For performance

            //Decrease max try count when fps low
            //switch ((Fps.FpsValue+5)/10)
            //{
            //    case 5:
            //        maxTryCount = 30;
            //        break;
            //    case 4:
            //    case 2:
            //    case 1:
            //    case 0:
            //        maxTryCount = 15;
            //        break;
            //}

            while (!frontier.IsEmpty)
            {
                if (maxTryCount != -1 && tryCount++ > maxTryCount) break;
                var current = frontier.DeleteMin().Location;
                if (current.Equals(endTile)) break;
                if (finder.HasObstacle(current) && current != startTile) continue;
                foreach (var next in FindNeighbors(current))
                {
                    var newCost = costSoFar[current] + 1;
                    if (!costSoFar.ContainsKey(next) ||
                        newCost < costSoFar[next])
                    {
                        costSoFar[next] = newCost;
                        var priority = newCost + GetTilePositionCost(endTile, next);
                        frontier.Add(new Node<Vector2>(next, priority));
                        cameFrom[next] = current;
                    }
                }

            }

            return GetPath(cameFrom, startTile, endTile);;
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

        public static float GetPixelPostionCost(Vector2 fromPosition, Vector2 toPosition)
        {
            return Vector2.Distance(fromPosition, toPosition);
        }

        public static int GetTilePositionCost(Vector2 fromTile, Vector2 toTile)
        {
            return GetPathTileDistance(fromTile, toTile);
        }

        /// <summary>
        /// Find not obstacle tile neighbors at location.
        /// </summary>
        /// <param name="location">Tile postion.</param>
        /// <returns>Neighbors list.</returns>
        public static List<Vector2> FindNeighbors(Vector2 location)
        {
            List<int> list;
            return FindNeighbors(location, out list);
        }

        /// <summary>
        /// Find not obstacle tile neighbors at location.
        /// </summary>
        /// <param name="location">Tile postion.</param>
        /// <param name="removeList">Obstacle neighbor index list.</param>
        /// <returns>Neighbors list.</returns>
        public static List<Vector2> FindNeighbors(Vector2 location, out List<int> removeList)
        {
            var listAll = FindAllNeighbors(location);
            removeList = GetObstacleIndexList(listAll);

            var list = new List<Vector2>();
            var count = listAll.Count;
            for (var j = 0; j < count; j++)
            {
                if (!removeList.Contains(j))
                    list.Add(listAll[j]);
            }

            return list;
        }

        private static List<int> GetObstacleIndexList(List<Vector2> neighborList)
        {
            var removeList = new List<int>();
            var count = neighborList.Count;
            for (var i = 0; i < count; i++)
            {
                if (Globals.TheMap.IsObstacleForCharacter(neighborList[i]))
                {
                    removeList.Add(i);
                    if (Globals.TheMap.IsObstacle(neighborList[i]))
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
            return removeList;
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

        /// <summary>
        /// Find one nonobstacle neighbor at destinationTilePosition. 
        /// If destinationTilePosition is not obstacle, destinationTilePosition is not changed, return ture.
        /// Otherwise return ture if finded one nonobstacle neighbor and destinationTilePosition is assigned to that neighbor's tile position.
        /// Return false if not finded.
        /// </summary>
        /// <param name="finder">The finder</param>
        /// <param name="destinationTilePosition"></param>
        /// <returns></returns>
        public static bool FindNonobstacleNeighborOrItself(Character finder, ref Vector2 destinationTilePosition)
        {
            if (finder.HasObstacle(destinationTilePosition) ||
                Globals.TheMap.IsObstacleForCharacter(destinationTilePosition))
            {
                var neighbors = FindAllNeighbors(destinationTilePosition);
                foreach (var neighbor in neighbors)
                {
                    if (!finder.HasObstacle(neighbor) &&
                        !Globals.TheMap.IsObstacleForCharacter(neighbor))
                    {
                        destinationTilePosition = neighbor;
                        return true;
                    }
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// Test finder can move linearly.
        /// </summary>
        /// <param name="finder"></param>
        /// <param name="fromTilePosition"></param>
        /// <param name="toTilePosition"></param>
        /// <returns></returns>
        public static bool CanLinearlyMove(Character finder, Vector2 fromTilePosition, Vector2 toTilePosition)
        {
            if(fromTilePosition == toTilePosition) return true;
            if (finder.HasObstacle(toTilePosition)) return false;
            var tileDistance = GetPathTileDistance(fromTilePosition, toTilePosition);
            var path = FindPathPerfect(finder, fromTilePosition, toTilePosition, tileDistance*16);
            if (path != null && (path.Count - 1) == tileDistance)
            {
                return true;
            }
            return false;
        }

    }



    public struct Node<T> : IComparable<Node<T>>
    {
        public T Location;
        public float Priority;

        public Node(T location, float priority)
        {
            Location = location;
            Priority = priority;
        }

        public int CompareTo(Node<T> other)
        {
            return this.Priority.CompareTo(other.Priority);
        }
    }
}
