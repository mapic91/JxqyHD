using Engine.Gui.Base;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Texture = Engine.Gui.Base.Texture;

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
            var cfg = GuiManager.Setttings.Sections["BottomState"];
            BaseTexture = new Texture(Utils.GetAsf(null, cfg["Image"]));
            Width = BaseTexture.Width;
            Height = BaseTexture.Height;
            Position = new Vector2(Globals.WindowWidth/2f + int.Parse(cfg["LeftAdjust"]), 
                Globals.WindowHeight - BaseTexture.Height + int.Parse(cfg["TopAdjust"]));
            InitializeItems();
        }

        private void InitializeItems()
        {
            var cfg = GuiManager.Setttings.Sections["BottomState_Life"];
            _life = new ColumnView(this,
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                new Texture(Utils.GetAsf(null, cfg["Image"])));
            cfg = GuiManager.Setttings.Sections["BottomState_Thew"];
            _thew = new ColumnView(this,
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                new Texture(Utils.GetAsf(null, cfg["Image"])));
            cfg = GuiManager.Setttings.Sections["BottomState_Mana"];
            _mana = new ColumnView(this,
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                new Texture(Utils.GetAsf(null, cfg["Image"])));
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