using Engine.Gui.Base;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Texture = Engine.Gui.Base.Texture;

namespace Engine.Gui
{
    public sealed class ToolTipGuiType1 : ToolTipGuiBase
    {
        private GuiItem _image;
        private TextGui _name, _costOrLevel, _goodEffect, _magicIntro, _goodIntro;
        private GuiItem[] _items;

        public ToolTipGuiType1()
        {
            var cfg = GuiManager.Setttings.Sections["ToolTip_Type1"];
            BaseTexture = new Texture(Utils.GetAsf(null, cfg["Image"]));
            Width = BaseTexture.Width;
            Height = BaseTexture.Height;
            Position = new Vector2((Globals.WindowWidth - Width)/ 2f + int.Parse(cfg["LeftAdjust"]),
                0 + int.Parse(cfg["TopAdjust"]));

            cfg = GuiManager.Setttings.Sections["ToolTip_Type1_Item_Image"];
            _image = new GuiItem(this,
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                null);
            cfg = GuiManager.Setttings.Sections["ToolTip_Type1_Item_Name"];
            _name = new TextGui(this,
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                Globals.FontSize10,
                int.Parse(cfg["CharSpace"]),
                int.Parse(cfg["LineSpace"]),
                "",
                Utils.GetColor(cfg["Color"]));
            cfg = GuiManager.Setttings.Sections["ToolTip_Type1_Item_PriceOrLevel"];
            _costOrLevel = new TextGui(this,
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                Globals.FontSize10,
                int.Parse(cfg["CharSpace"]),
                int.Parse(cfg["LineSpace"]),
                "",
                Utils.GetColor(cfg["Color"]));
            cfg = GuiManager.Setttings.Sections["ToolTip_Type1_Item_Effect"];
            _goodEffect = new TextGui(this,
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                Globals.FontSize10,
                int.Parse(cfg["CharSpace"]),
                int.Parse(cfg["LineSpace"]),
                "",
                Utils.GetColor(cfg["Color"]));
            cfg = GuiManager.Setttings.Sections["ToolTip_Type1_Item_Magic_Intro"];
            _magicIntro = new TextGui(this,
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                Globals.FontSize10,
                int.Parse(cfg["CharSpace"]),
                int.Parse(cfg["LineSpace"]),
                "",
                Utils.GetColor(cfg["Color"]));
            cfg = GuiManager.Setttings.Sections["ToolTip_Type1_Item_Good_Intro"];
            _goodIntro = new TextGui(this,
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                Globals.FontSize10,
                int.Parse(cfg["CharSpace"]),
                int.Parse(cfg["LineSpace"]),
                "",
                Utils.GetColor(cfg["Color"]));
            _items = new GuiItem[6];
            _items[0] = _image;
            _items[1] = _name;
            _items[2] = _costOrLevel;
            _items[3] = _goodEffect;
            _items[4] = _goodIntro;
            _items[5] = _magicIntro;

            IsShow = false;
        }

        public void Clear()
        {
            _image.BaseTexture = null;
            _name.Text = 
                _costOrLevel.Text =
                _goodEffect.Text =
                _goodIntro.Text =
                _magicIntro.Text = "";
        }

        public override void ShowMagic(Magic magic)
        {
            Clear();
            IsShow = true;
            Texture texture = null;
            var name = "无名称";
            var level = "等级： 0";
            var intro = "无简介";
            if (magic != null)
            {
                texture = new Texture(magic.Image);
                if (!string.IsNullOrEmpty(magic.Name))
                    name = magic.Name;
                level = "等级： " + magic.CurrentLevel;
                if (!string.IsNullOrEmpty(magic.Intro))
                    intro = magic.Intro;
            }
            _image.BaseTexture = texture;
            _name.Text = name;
            _costOrLevel.Text = level;
            _magicIntro.Text = intro;
        }

        public override void ShowGood(Good good)
        {
            Clear();
            IsShow = true;
            Texture texture = null;
            var name = "无名称";
            var cost = "价格： 0";
            var effect = "";
            var intro = "无简介";
            if (good != null)
            {
                texture = new Texture(good.Image);
                if (!string.IsNullOrEmpty(good.Name))
                    name = good.Name;
                cost = "价格： " + good.Cost;
                if (good.Life != 0)
                    effect += ("命 " + good.Life.ToString("+#;-#") + "  ");
                if (good.Thew != 0)
                    effect += ("体 " + good.Thew.ToString("+#;-#") + "  ");
                if (good.Mana != 0)
                    effect += ("气 " + good.Mana.ToString("+#;-#") + "  ");
                if (good.Attack != 0 || good.Attack2 != 0)
                {
                    var attack1 = "";
                    if (good.Attack != 0)
                    {
                        attack1 = good.Attack.ToString("+#;-#");
                    }
                    var attack2 = "";
                    if (good.Attack2 != 0)
                    {
                        attack2 = string.Format("({0:+#;-#})", good.Attack2);
                    }
                    effect += ("攻 " + attack1 + attack2 + "  ");
                }
                if (good.Defend != 0 || good.Defend2 != 0)
                {
                    var defend1 = "";
                    if (good.Defend != 0)
                    {
                        defend1 = good.Defend.ToString("+#;-#");
                    }
                    var defend2 = "";
                    if (good.Defend2 != 0)
                    {
                        defend2 = string.Format("({0:+#;-#})", good.Defend2);
                    }
                    effect += ("防 " + defend1 + defend2 + "  ");
                }
                if (good.Evade != 0)
                    effect += ("捷 " + good.Evade.ToString("+#;-#") + "  ");
                if(good.LifeMax != 0)
                    effect += ("命 " + good.LifeMax.ToString("+#;-#") + "  ");
                if(good.ThewMax != 0)
                    effect += ("体 " + good.ThewMax.ToString("+#;-#") + "  ");
                if(good.ManaMax != 0)
                    effect += ("气 " + good.ManaMax.ToString("+#;-#") + "  ");
                if (!string.IsNullOrEmpty(good.Intro))
                    intro = good.Intro;
            }
            _image.BaseTexture = texture;
            _name.Text = name;
            _costOrLevel.Text = cost;
            _goodEffect.Text = effect;
            _goodIntro.Text = intro;
        }

        public override void Update(GameTime gameTime)
        {
            if(!IsShow) return;
            base.Update(gameTime);
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