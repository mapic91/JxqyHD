using System;
using Engine.Gui.Base;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Texture = Engine.Gui.Base.Texture;

namespace Engine.Gui
{
    public sealed class SaveLoadGui : GuiItem
    {
        private ListTextItem _list;

        public bool CanSave { set; get; }

        public SaveLoadGui()
        {
            IsShow = false;
            BaseTexture = new Texture(Utils.GetAsf(@"asf\ui\saveload", "panel.asf"));
            Width = BaseTexture.Width;
            Height = BaseTexture.Height;
            Position = new Vector2(
                (Globals.WindowWidth - Width) / 2f,
                (Globals.WindowHeight - Height) / 2f);
            var itemText = new String[]
            {
                "进度一",
                "进度二",
                "进度三",
                "进度四",
                "进度五",
                "进度六",
                "进度七"
            };
            _list = new ListTextItem(this,
                new Vector2(141, 118),
                80,
                189,
                null,
                7,
                itemText,
                3,
                0,
                25,
                new Color(91, 31, 27) * 0.8f,
                new Color(102, 73, 212) * 0.8f);
        }

        public override void Update(GameTime gameTime)
        {
            if(!IsShow) return;
            base.Update(gameTime);
            _list.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if(!IsShow) return;
            base.Draw(spriteBatch);
            _list.Draw(spriteBatch);
        }
    }
}