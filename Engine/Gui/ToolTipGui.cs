using System;
using Engine.Gui.Base;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Texture = Engine.Gui.Base.Texture;

namespace Engine.Gui
{
    public class ToolTipGui : GuiItem
    {
        private GuiItem _image;
        private TextGui _name, _costOrLevel, _goodEffect, _magicIntro, _goodIntro;
        private GuiItem[] _items;

        public ToolTipGui()
        {
            IsShow = false;
            BaseTexture = new Texture(Utils.GetAsf(@"asf\ui\common\", "tipbox.asf"));
            Width = BaseTexture.Width;
            Height = BaseTexture.Height;
            Position = new Vector2((Globals.WindowWidth - Width)/2f,
                27f);
            _image = new GuiItem(this,
                new Vector2(132, 47),
                60,
                75,
                null);
            _name = new TextGui(this,
                new Vector2(67, 191),
                100,
                20,
                Globals.FontSize10,
                0,
                0,
                "",
                new Color(102, 73, 212)* 0.8f);
            _costOrLevel = new TextGui(this,
                new Vector2(160, 191),
                88,
                20,
                Globals.FontSize10,
                0,
                0,
                "",
                new Color(91, 31, 27)*0.8f);
            _goodEffect = new TextGui(this,
                new Vector2(67,215),
                188,
                20,
                Globals.FontSize10,
                0,
                0,
                "",
                Color.Blue*0.8f);
            _magicIntro = new TextGui(this,
                new Vector2(67, 210),
                196, 
                120,
                Globals.FontSize10,
                1,
                0,
                "",
                new Color(52, 21, 14)*0.8f);
            _goodIntro = new TextGui(this,
                new Vector2(67, 245),
                196,
                100,
                Globals.FontSize10,
                1,
                0,
                "",
                new Color(52, 21, 14) * 0.8f);
            _items = new GuiItem[6];
            _items[0] = _image;
            _items[1] = _name;
            _items[2] = _costOrLevel;
            _items[3] = _goodEffect;
            _items[4] = _goodIntro;
            _items[5] = _magicIntro;
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

        public void ShowMagic(Magic magic)
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

        public void ShowGood(Good good)
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
                if (good.Attack != 0)
                    effect += ("攻 " + good.Attack.ToString("+#;-#") + "  ");
                if (good.Defend != 0)
                    effect += ("防 " + good.Defend.ToString("+#;-#") + "  ");
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