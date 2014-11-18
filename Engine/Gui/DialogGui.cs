using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Gui
{
    public sealed class DialogGui : GuiItem
    {
        private TextGui _text;
        private TextGui _selectA;
        private TextGui _selectB;
        public DialogGui()
        {
            IsShow = false;
            BaseTexture = new Texture(Utils.GetAsf(@"asf\ui\dialog\", "panel.asf"));
            Width = BaseTexture.Width;
            Height = BaseTexture.Height;
            Position = new Vector2((Globals.WindowWidth - BaseTexture.Width) / 2f, 
                Globals.WindowHeight - 208f);
            _text = new TextGui(this,
                new Vector2(65, 30),
                310,
                70,
                Globals.FontSize12,
                1,
                2,
                "",
                Color.Black*0.8f);
        }

        public void ShowText(string text)
        {
            _text.Text = text;
            IsShow = true;
        }

        public bool NextPage()
        {
            return _text.NextPage();
        }

        public override void Update(GameTime gameTime)
        {
            if(!IsShow) return;
            base.Update(gameTime);
            _text.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsShow) return;
            base.Draw(spriteBatch);
            _text.Draw(spriteBatch);
        }
    }
}