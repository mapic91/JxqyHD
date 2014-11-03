using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine
{
    public static class ObjManager
    {
        private static LinkedList<Obj> _list = new LinkedList<Obj>();
        private static List<Obj> _objInView = new List<Obj>();
        private static Rectangle _lastViewRegion;
        private static bool _objListChanged = true;

        public static List<Obj> ObjsInView
        {
            get
            {
                if (!_lastViewRegion.Equals(Globals.TheCarmera.CarmerRegionInWorld) ||
                    _objListChanged)
                {
                    _objInView = GetObjsInView();
                    _lastViewRegion = Globals.TheCarmera.CarmerRegionInWorld;
                }
                return _objInView;
            }
        }

        public static LinkedList<Obj> ObjList
        {
            get { return _list; }
        }

        private static List<Obj> GetObjsInView()
        {
            var viewRegion = Globals.TheCarmera.CarmerRegionInWorld;
            var list = new List<Obj>(_list.Count);
            foreach (var obj in _list)
            {
                if (viewRegion.Intersects(obj.RegionInWorld))
                    list.Add(obj);
            }
            return list;
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
            ClearAllObj();

            var count = lines.Count();
            for (var i = 0; i < count; )
            {
                var groups = Regex.Match(lines[i++], @"\[OBJ([0-9]+)\]").Groups;
                if (groups[0].Success)
                {
                    var contents = new List<string>();
                    while (i < count && !string.IsNullOrEmpty(lines[i]))
                    {
                        contents.Add(lines[i]);
                        i++;
                    }
                    AddObj(contents.ToArray());
                    i++;
                }
            }
            return true;
        }

        public static void AddObj(string[] lines)
        {
            var obj = new Obj();
            obj.Load(lines);
            AddObj(obj);
        }

        public static void AddObj(Obj obj)
        {
            if (obj != null)
            {
                _list.AddLast(obj);
                _objListChanged = true;
            }
        }

        public static void ClearAllObj()
        {
            _list.Clear();
            _objListChanged = true;
        }

        public static bool IsObstacle(int tileX, int tileY)
        {
            foreach (var obj in _list)
            {
                if (obj.MapX == tileX && obj.MapY == tileY && obj.IsObstacle)
                    return true;
            }
            return false;
        }

        public static bool IsObstacle(Vector2 tilePosition)
        {
            return IsObstacle((int)tilePosition.X, (int)tilePosition.Y);
        }

        //just check objs in view
        public static bool IsObstacleInView(int tileX, int tileY)
        {
            foreach (var obj in ObjsInView)
            {
                if (obj.MapX == tileX && obj.MapY == tileY && obj.IsObstacle)
                    return true;
            }
            return false;
        }

        //just check objs in view
        public static bool IsObstacleInView(Vector2 tilePosition)
        {
            return IsObstacleInView((int)tilePosition.X, (int)tilePosition.Y);
        }

        public static Obj GetObstacle(int tileX, int tileY)
        {
            foreach (var obj in _list)
            {
                if (obj.MapX == tileX && obj.MapY == tileY && obj.IsObstacle)
                    return obj;
            }
            return null;
        }

        public static Obj GetObstacle(Vector2 tilePosition)
        {
            return GetObstacle((int)tilePosition.X, (int)tilePosition.Y);
        }

        public static void Update(GameTime gameTime)
        {
            foreach (var obj in _list)
            {
                obj.Update(gameTime);
            }
        }

        public static void Draw(SpriteBatch spriteBatch)
        {
            foreach (var obj in _list)
                obj.Draw(spriteBatch);
        }
    }
}
