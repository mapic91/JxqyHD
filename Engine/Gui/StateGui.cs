using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Gui
{
    public class StateGui : GuiItem
    {
        private bool _isFemale;
        private TextGui[] _items;
        private bool _isShow;

        public new bool IsShow
        {
            set
            {
                _isShow = value;
                if (value)
                    UpdateItems();
            }
            get { return _isShow; }
        }

        public bool IsFemale
        {
            get { return _isFemale; }
            set
            {
                _isFemale = value;
                var fileName = "panel5.asf";
                if (value)
                    fileName = "panel5b.asf";
                BaseTexture = new Texture(Utils.GetAsf(@"asf\ui\common\", fileName));
            }
        }

        public StateGui()
        {
            BaseTexture = new Texture(Utils.GetAsf(@"asf\ui\common\", "panel5.asf"));
            Width = BaseTexture.Width;
            Height = BaseTexture.Height;
            Position = new Vector2(
                Globals.WindowWidth / 2f - Width,
                0f);
            _items = new TextGui[9];
            const int Left = 144;
            const int ItemWidth = 100;
            const int ItemHeight = 12;
            var color = Color.Black*0.7f;
            _items[0] = new TextGui(this, //Level
                new Vector2(Left, 219),
                ItemWidth,
                ItemHeight,
                Globals.FontSize7,
                0,
                0,
                "",
                color);
            _items[1] = new TextGui(this, //Exp
                new Vector2(Left, 234),
                ItemWidth,
                ItemHeight,
                Globals.FontSize7,
                0,
                0,
                "",
                color);
            _items[2] = new TextGui(this, //LevelUp
                new Vector2(Left, 249),
                ItemWidth,
                ItemHeight,
                Globals.FontSize7,
                0,
                0,
                "",
                color);
            _items[3] = new TextGui(this, //Life
                new Vector2(Left, 264),
                ItemWidth,
                ItemHeight,
                Globals.FontSize7,
                0,
                0,
                "",
                color);
            _items[4] = new TextGui(this, //Thew
                new Vector2(Left, 279),
                ItemWidth,
                ItemHeight,
                Globals.FontSize7,
                0,
                0,
                "",
                color);
            _items[5] = new TextGui(this, //Mana
                new Vector2(Left, 294),
                ItemWidth,
                ItemHeight,
                Globals.FontSize7,
                0,
                0,
                "",
                color);
            _items[6] = new TextGui(this, //Attack
                new Vector2(Left, 309),
                ItemWidth,
                ItemHeight,
                Globals.FontSize7,
                0,
                0,
                "",
                color);
            _items[7] = new TextGui(this, //Defend
                new Vector2(Left, 324),
                ItemWidth,
                ItemHeight,
                Globals.FontSize7,
                0,
                0,
                "",
                color);
            _items[8] = new TextGui(this, // Evade
                new Vector2(Left, 339),
                ItemWidth,
                ItemHeight,
                Globals.FontSize7,
                0,
                0,
                "",
                color);
        }

        public void UpdateItems()
        {
            var player = Globals.ThePlayer;
            if (player == null)
            {
                foreach (var item in _items)
                {
                    item.Text = "";
                }
            }
            else
            {
                _items[0].Text = player.Level.ToString();
                _items[1].Text = player.Exp.ToString();
                _items[2].Text = player.LevelUpExp.ToString();
                _items[3].Text = player.Life + "/" + player.LifeMax;
                _items[4].Text = player.Thew + "/" + player.ThewMax;
                _items[5].Text = player.Mana + "/" + player.ManaMax;
                _items[6].Text = player.Attack.ToString();
                _items[7].Text = player.Defend.ToString();
                _items[8].Text = player.Evade.ToString();
            }
        }

        public override void Update(GameTime gameTime)
        {
            if(!IsShow) return;
            base.Update(gameTime);
            UpdateItems();
            foreach (var item in _items)
            {
                item.Update(gameTime);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if(!IsShow) return;
            base.Draw(spriteBatch);
            foreach (var item in _items)
            {
                item.Draw(spriteBatch);
            }
        }
    }
}