using System.Text;
using Engine.Gui.Base;
using Engine.ListManager;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Texture = Engine.Gui.Base.Texture;

namespace Engine.Gui
{
    public sealed class MemoGui : GuiItem
    {
        private ScrollBar _scrollBar;
        private TextGui _text;

        public MemoGui()
        {
            var cfg = GuiManager.Setttings.Sections["Memo"];
            BaseTexture = new Texture(Utils.GetAsf(null, cfg["Image"]));
            Width = BaseTexture.Width;
            Height = BaseTexture.Height;
            Position = new Vector2(
                Globals.WindowWidth / 2f + int.Parse(cfg["LeftAdjust"]),
                0f + int.Parse(cfg["TopAdjust"]));

            cfg = GuiManager.Setttings.Sections["Memo_Text"];
            _text = new TextGui(this, 
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                Globals.FontSize10,
                int.Parse(cfg["CharSpace"]),
                int.Parse(cfg["LineSpace"]),
                "",
                Utils.GetColor(cfg["Color"]));

            cfg = GuiManager.Setttings.Sections["Memo_Slider"];
            var slideTexture = Utils.GetAsf(null, cfg["Image_Btn"]);
            var slideBaseTexture = new Texture(slideTexture);
            var slideClikedTexture = new Texture(slideTexture, 0, 1);
            var slideButton = new GuiItem(this,
                Vector2.Zero,
                slideBaseTexture.Width,
                slideBaseTexture.Height,
                slideBaseTexture,
                null,
                slideClikedTexture,
                null,
                Utils.GetSoundEffect(cfg["Sound_Btn"]));
            _scrollBar = new ScrollBar(this,
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                null,
                ScrollBar.ScrollBarType.Vertical,
                slideButton,
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                0,
                1,
                0);
            _scrollBar.Scrolled += (arg1, arg2) => SetTextShow(arg2.Value);

            MouseScrollUp += (arg1, arg2) => _scrollBar.Value -= 1;
            MouseScrollDown += (arg1, arg2) => _scrollBar.Value += 1;

            IsShow = false;
        }

        public void UpdateTextShow()
        {
            var count = MemoListManager.GetCount();
            _scrollBar.MaxValue = count - 1;
            SetTextShow(_scrollBar.Value);
        }

        public void SetTextShow(int indexBegin)
        {
            var text = new StringBuilder();
            for (var i = 0; i < 10; i++)
            {
                text.Append(MemoListManager.GetString(indexBegin + i) + "\n");
            }
            _text.Text = text.ToString();
        }

        public override void Update(GameTime gameTime)
        {
            if(!IsShow)return;
            base.Update(gameTime);
            _text.Update(gameTime);
            _scrollBar.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsShow) return;
            base.Draw(spriteBatch);
            _text.Draw(spriteBatch);
            _scrollBar.Draw(spriteBatch);
        }
    }
}