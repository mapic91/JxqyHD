using Engine.Gui.Base;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Texture = Engine.Gui.Base.Texture;

namespace Engine.Gui
{
    public class StateGui : GuiItem
    {
        private int _index;
        private TextGui[] _items;
        private bool _isShow;

        public override bool IsShow
        {
            set
            {
                _isShow = value;
                if (value)
                    UpdateItems();
            }
            get { return _isShow; }
        }

        public int Index
        {
            get { return _index; }
            set
            {
                _index = value;
                var fileName = "panel5.asf";
                if (value > 0)
                    fileName = "panel5" + (char)('a' + value) + ".asf"; ;
                BaseTexture = new Texture(Utils.GetAsf(@"asf\ui\common\", fileName));
            }
        }

        public StateGui()
        {
            var cfg = GuiManager.Setttings.Sections["State"];
            BaseTexture = new Texture(Utils.GetAsf(@"asf\ui\common\", "panel5.asf"));
            Width = BaseTexture.Width;
            Height = BaseTexture.Height;
            Position = new Vector2(
                Globals.WindowWidth / 2f - Width + int.Parse(cfg["LeftAdjust"]),
                0f + int.Parse(cfg["TopAdjust"]));
            _items = new TextGui[9];

            cfg = GuiManager.Setttings.Sections["State_Level"];
            _items[0] = new TextGui(this, //Level
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                Globals.FontSize7,
                0,
                0,
                "",
                Utils.GetColor(cfg["Color"]));
            cfg = GuiManager.Setttings.Sections["State_Exp"];
            _items[1] = new TextGui(this, //Exp
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                Globals.FontSize7,
                0,
                0,
                "",
                Utils.GetColor(cfg["Color"]));
            cfg = GuiManager.Setttings.Sections["State_LevelUp"];
            _items[2] = new TextGui(this, //LevelUp
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                Globals.FontSize7,
                0,
                0,
                "",
                Utils.GetColor(cfg["Color"]));
            cfg = GuiManager.Setttings.Sections["State_Life"];
            _items[3] = new TextGui(this, //Life
               new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                Globals.FontSize7,
                0,
                0,
                "",
                Utils.GetColor(cfg["Color"]));
            cfg = GuiManager.Setttings.Sections["State_Thew"];
            _items[4] = new TextGui(this, //Thew
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                Globals.FontSize7,
                0,
                0,
                "",
                Utils.GetColor(cfg["Color"]));
            cfg = GuiManager.Setttings.Sections["State_Mana"];
            _items[5] = new TextGui(this, //Mana
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                Globals.FontSize7,
                0,
                0,
                "",
                Utils.GetColor(cfg["Color"]));
            cfg = GuiManager.Setttings.Sections["State_Attack"];
            _items[6] = new TextGui(this, //Attack
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                Globals.FontSize7,
                0,
                0,
                "",
                Utils.GetColor(cfg["Color"]));
            cfg = GuiManager.Setttings.Sections["State_Defend"];
            _items[7] = new TextGui(this, //Defend
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                Globals.FontSize7,
                0,
                0,
                "",
                Utils.GetColor(cfg["Color"]));
            cfg = GuiManager.Setttings.Sections["State_Evade"];
            _items[8] = new TextGui(this, // Evade
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                Globals.FontSize7,
                0,
                0,
                "",
                Utils.GetColor(cfg["Color"]));
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
                _items[5].Text = player.ManaLimit
                    ? "1/1"
                    : player.Mana + "/" + player.ManaMax;
                _items[6].Text = player.Attack.ToString();
                if (player.Attack2 != 0 || player.Attack3 != 0)
                {
                    _items[6].Text += string.Format("({0})({1})", player.Attack2, player.Attack3);
                }
                _items[7].Text = player.Defend.ToString();
                if (player.Defend2 != 0 || player.Defend3 != 0)
                {
                    _items[7].Text += string.Format("({0})({1})", player.Defend2, player.Defend3);
                }
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