using System.Collections.Generic;
using Engine.Gui.Base;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Gui
{
    public sealed class LittleHeadGui : GuiItem
    {
        private Dictionary<string, Asf> _headIco = new Dictionary<string, Asf>(); 
        public LittleHeadGui()
        {
            IsShow = true;
        }

        public override void Update(GameTime gameTime)
        {
            //do nothing
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            var partners = NpcManager.GetAllPartner();
            const int x = 5;
            var y = 5;
            foreach (var partner in partners)
            {
                var name = partner.Name;
                if(string.IsNullOrEmpty(name))continue;
                if (!_headIco.ContainsKey(name))
                {
                    _headIco[name] = Utils.GetAsf(@"asf\ui\littlehead\", name + ".asf");
                }
                if(_headIco[name] == null)continue;
                var texture = _headIco[name].GetFrame(0);
                if(texture == null) continue;

                spriteBatch.Draw(texture, new Vector2(x, y), Color.White);
                y += texture.Width + 2;
            }
        }
    }
}