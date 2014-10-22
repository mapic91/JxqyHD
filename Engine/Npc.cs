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

        public Npc(string filePath) : base(filePath)
        {
            
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
        }
    }
}
