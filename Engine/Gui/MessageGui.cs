using Engine.Gui.Base;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using Texture = Engine.Gui.Base.Texture;

namespace Engine.Gui
{
    public class MessageGui : GuiItem
    {
        private TextGui _message;
        private float _elepsedMilliseconds;
        private const float MaxShowMilliseconds = 2000;
        private LinkedList<string> _msgs = new LinkedList<string>();
        public override sealed bool IsShow
        {
            get { return base.IsShow; }
            set
            {
                if(base.IsShow == value)
                {
                    return;
                }
                base.IsShow = value;
                _elepsedMilliseconds = 0;
                if(value && _msgs.First != null)
                {
                    var msg = _msgs.First.Value;
                    _msgs.RemoveFirst();
                    _message.Text = msg;
                }
            }
        }

        public MessageGui()
        {
            var cfg = GuiManager.Setttings.Sections["Message"];
            BaseTexture = new Texture(Utils.GetAsf(null, cfg["Image"]));
            Width = BaseTexture.Width;
            Height = BaseTexture.Height;
            Position = new Vector2((Globals.WindowWidth - Width) / 2 + int.Parse(cfg["LeftAdjust"]),
                Globals.WindowHeight - Height + int.Parse(cfg["TopAdjust"]));

            cfg = GuiManager.Setttings.Sections["Message_Text"];
            _message = new TextGui(this,
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                Globals.FontSize12,
                int.Parse(cfg["CharSpace"]),
                int.Parse(cfg["LineSpace"]),
                "",
                Utils.GetColor(cfg["Color"]));

            IsShow = false;
        }

        public void ShowMessage(string message)
        {
            //_msgs.AddLast(message);
            _message.Text = message;
            IsShow = true;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            _message.Update(gameTime);
            _elepsedMilliseconds += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if(IsShow)
            {
                if (_elepsedMilliseconds >= MaxShowMilliseconds)
                    IsShow = false;
            } else
            {
                if (_elepsedMilliseconds >= 500 && _msgs.First != null)
                    IsShow = true;
            }
            
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsShow) return;
            base.Draw(spriteBatch);
            _message.Draw(spriteBatch);
        }
    }
}