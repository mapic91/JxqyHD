using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Gui
{
    public class ColumnGui : GuiItem
    {
        private ColumnView _life;
        private ColumnView _thew;
        private ColumnView _mana;
        private ColumnView[] _items = new ColumnView[3];
        public ColumnGui()
        {
            BaseTexture = new Texture(Utils.GetAsf(@"asf\ui\column\panel9.asf"));
            Width = BaseTexture.Width;
            Height = BaseTexture.Height;
            Position = new Vector2(Globals.WindowWidth/2f - 320, 
                Globals.WindowHeight - BaseTexture.Height);
            InitializeItems();
        }

        private void InitializeItems()
        {
            _life = new ColumnView(this,
                new Vector2(11, 22),
                48, 
                46,
                new Texture(Utils.GetAsf(@"asf\ui\column\ColLife.asf")));
            _thew = new ColumnView(this,
                new Vector2(59, 22),
                48,
                46,
                new Texture(Utils.GetAsf(@"asf\ui\column\ColThew.asf")));
            _mana = new ColumnView(this,
                new Vector2(113, 22),
                48,
                46,
                new Texture(Utils.GetAsf(@"asf\ui\column\ColMana.asf")));
            _items[0] = _life;
            _items[1] = _thew;
            _items[2] = _mana;
        }

        private void Update(Player player)
        {
            if(player == null) return;
            _life.Percent = player.Life/(float) player.LifeMax;
            _thew.Percent = player.Thew/(float) player.ThewMax;
            _mana.Percent = player.Mana/(float) player.ManaMax;
        }

        public override void Update(GameTime gameTime)
        {
            if(!IsShow) return;
            base.Update(gameTime);
            foreach (var item in _items)
            {
                item.Update(gameTime);
            }
            Update(Globals.ThePlayer);
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