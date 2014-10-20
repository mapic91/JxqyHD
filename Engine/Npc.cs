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

        public Npc() { }

        public Npc(string filePath) : base(filePath)
        {
            
        }

        public void Draw(SpriteBatch spriteBatch, Point mousePositionInWorld)
        {
            var texture = GetCurrentTexture();
            if (Collider.IsPixelCollideForNpcObj(mousePositionInWorld,
                RegionInWorld,
                texture))
            {
                Globals.OutEdgeSprite = this;
                Globals.OutEdgeTexture = TextureGenerator.GetOuterEdge(texture, Globals.NpcEdgeColor);
            }
            Draw(spriteBatch);
        }
    }
}
