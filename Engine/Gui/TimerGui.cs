using Engine.Gui.Base;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Texture = Engine.Gui.Base.Texture;

namespace Engine.Gui
{
    public sealed class TimerGui : GuiItem
    {
        private TextGui _text;
        private int _seconds;
        private float _elapsedMilliSeconds;
        private bool _isHide;

        public bool IsHide
        {
            get { return _isHide; }
        }

        public TimerGui()
        {
            var cfg = GuiManager.Setttings.Sections["Timer"];
            BaseTexture = new Texture(Utils.GetAsf(null, cfg["Image"]));
            Width = BaseTexture.Width;
            Height = BaseTexture.Height;
            Position = new Vector2(Globals.WindowWidth/ 2f + int.Parse(cfg["LeftAdjust"]), 0 + int.Parse(cfg["TopAdjust"]));

            cfg = GuiManager.Setttings.Sections["Timer_Text"];
            _text = new TextGui(this, 
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

        private void UpdateText()
        {
            _text.Text = string.Format("{0: 00;-00}", _seconds / 60) + 
                "分" +
                string.Format("{0: 00;-00}", _seconds % 60 ) +
                "秒";
        }

        public void StartTimer(int seconds)
        {
            IsShow = true;
            _isHide = false;
            _seconds = seconds;
            _elapsedMilliSeconds = 0;
            UpdateText();
        }

        public void StopTimer()
        {
            IsShow = false;
        }

        public void HideTimerWnd()
        {
            _isHide = true;
        }

        public int GetCurrentSecond()
        {
            return _seconds;
        }

        public override void Update(GameTime gameTime)
        {
            if(!IsShow)return;
            base.Update(gameTime);
            _elapsedMilliSeconds += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
            if (_elapsedMilliSeconds >= 1000f)
            {
                _elapsedMilliSeconds -= 1000f;
                _seconds--;
            }
            UpdateText();
            _text.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsShow || _isHide) return;
            base.Draw(spriteBatch);
            _text.Draw(spriteBatch);
        }
    }
}