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
        public TimerGui()
        {
            IsShow = false;
            BaseTexture = new Texture(Utils.GetAsf(@"asf\ui\timer\", "window.asf"));
            Width = BaseTexture.Width;
            Height = BaseTexture.Height;
            Position = new Vector2(Globals.WindowWidth/2f + 103, 0);
            _text = new TextGui(this, 
                new Vector2(74, 44),
                120, 
                22,
                Globals.FontSize12,
                1,
                0,
                "",
                Color.Red*0.8f);
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