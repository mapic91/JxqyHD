using Engine.Gui.Base;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Texture = Engine.Gui.Base.Texture;

namespace Engine.Gui
{
    public class MessageGui : GuiItem
    {
        private TextGui _message;
        private float _elepsedMilliseconds;
        private const float MaxShowMilliseconds = 2000;
        public override sealed bool IsShow
        {
            get { return base.IsShow; }
            set
            {
                base.IsShow = value;
                _elepsedMilliseconds = 0;
            }
        }

        public MessageGui()
        {
            BaseTexture = new Texture(Utils.GetAsf(@"asf\ui\message\", "msgbox.asf"));
            Width = BaseTexture.Width;
            Height = BaseTexture.Height;
            Position = new Vector2((Globals.WindowWidth - Width) / 2 - 10,
                Globals.WindowHeight - 47 - Height);
            _message = new TextGui(this,
                new Vector2(46, 32),
                145,
                50,
                Globals.FontSize12,
                0,
                1,
                "",
                new Color(155, 34, 22) * 0.8f);

            IsShow = false;
        }

        public void ShowMessage(string message)
        {
            _message.Text = message;
            IsShow = true;
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsShow) return;
            base.Update(gameTime);
            _message.Update(gameTime);
            _elepsedMilliseconds += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (_elepsedMilliseconds >= MaxShowMilliseconds)
                IsShow = false;
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsShow) return;
            base.Draw(spriteBatch);
            _message.Draw(spriteBatch);
        }
    }
}