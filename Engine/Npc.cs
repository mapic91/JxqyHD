using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine
{
    public class Npc : Character
    {
        #region public properties
        public override PathFinder.PathType PathType
        {
            get
            {
                if (base.PathFinder == 1)
                    return Engine.PathFinder.PathType.PerfectMaxTry100;
                else
                    return Engine.PathFinder.PathType.PathOneStep;
            }
        }

        public bool IsEnemy
        {
            get { return Kind == 1 && Relation == 1; }
        }

        public bool IsFriend
        {
            get { return ((Kind == 1 && Relation == 0) || Kind == 3); }
        }

        public bool IsFollower
        {
            get { return Kind == 3; }
        }

        public bool  IsInteractive
        {
            get { return (!string.IsNullOrEmpty(ScriptFile) || IsEnemy || IsFriend); }
        }
        #endregion

        public Npc() { }

        public Npc(string filePath) : base(filePath) { }

        protected override bool HasObstacle(Vector2 tilePosition)
        {
            return (NpcManager.IsObstacle(tilePosition) ||
                    ObjManager.IsObstacle(tilePosition) ||
                    Globals.ThePlayer.TilePosition == tilePosition);
        }

        public override void Update(GameTime gameTime)
        {
            var isExist = IsExist;
            IsExist = false;//Temporary make self disappear because of obstacle check 

            if (IsEnemy)
            {
                var tileDistance = Engine.PathFinder.GetTileDistance(TilePosition,
                Globals.ThePlayer.TilePosition);

                if(tileDistance <= VisionRadius)
                    Attacking(Globals.ThePlayer.TilePosition);
                else Standing();
            }
            base.Update(gameTime);

            IsExist = isExist;//restore
        }
    }
}
