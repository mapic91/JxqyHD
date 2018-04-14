using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Engine.Gui.Base;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Gui
{
    public sealed class ToolTipGuiType2: ToolTipGuiBase
    {
        private BackgroundColorLayer _background;
        private TextGui _text;
        private String _magicNameColor;
        private String _magicLevelColor;
        private String _magicIntroColor;
        private String _goodNameColor;
        private String _goodPriceColor;
        private String _goodUserColor;
        private String _goodPropertyColor;
        private String _goodIntroColor;

        private const string FormatTemplate =
            "<color={0}><color=BeginRangeDefault>{1}<color=EndRangeDefault><color=Default>";

        private const string FormatAlignLeft = "<AlignLeft>{0}<EndAlign>";
        private int _textHorizontalPadding ;
        private int _textVerticalPadding;

        private void RePose()
        {
            _background.Height = _text.RealHeight + 2 * _textVerticalPadding;
            var pos = GuiManager.GetMouseScreenPosition();
            if (pos.X + _background.Width > Globals.WindowWidth)
            {
                pos.X = Globals.WindowWidth - _background.Width;
            }

            if (pos.Y + _background.Height > Globals.WindowHeight)
            {
                pos.Y = Globals.WindowHeight - _background.Height;
            }

            _background.Position = pos;
        }

        public ToolTipGuiType2()
        {
            var cfg = GuiManager.Setttings.Sections["ToolTip_Type2"];
            _textHorizontalPadding = int.Parse(cfg["TextHorizontalPadding"]);
            _textVerticalPadding = int.Parse(cfg["TextVerticalPadding"]);
            _background = new BackgroundColorLayer(null, new Vector2(0,0), Vector2.Zero, int.Parse(cfg["Width"]), 1, Utils.GetColor(cfg["BackgroundColor"]));
            _text = new TextGui(_background, 
                new Vector2(_textHorizontalPadding, _textVerticalPadding), 
                _background.Width - 2*_textHorizontalPadding, 0, 
                Globals.FontSize12, 0, 0, "", Color.Black, TextGui.Align.Center);

            _magicNameColor = cfg["MagicNameColor"];
            _magicLevelColor = cfg["MagicLevelColor"];
            _magicIntroColor = cfg["MagicIntroColor"];
            _goodNameColor = cfg["GoodNameColor"];
            _goodPriceColor = cfg["GoodPriceColor"];
            _goodUserColor = cfg["GoodUserColor"];
            _goodPropertyColor = cfg["GoodPropertyColor"];
            _goodIntroColor = cfg["GoodIntroColor"];
        }

        public override void ShowMagic(Magic magic)
        {
            IsShow = true;
            var showText = new StringBuilder();
            var name = "无名称";
            var level = "等级： 0";
            var intro = "无简介";
            if (magic != null)
            {
                if (!string.IsNullOrEmpty(magic.Name))
                    name = magic.Name;
                level = "等级： " + magic.CurrentLevel;
                if (!string.IsNullOrEmpty(magic.Intro))
                    intro = magic.Intro;
            }

            showText.AppendLine(String.Format(FormatTemplate, _magicNameColor, name));
            showText.AppendLine(String.Format(FormatTemplate, _magicLevelColor, level));
            showText.AppendLine();
            showText.AppendFormat(FormatAlignLeft, String.Format(FormatTemplate, _magicIntroColor, intro));

            _text.Text = showText.ToString();
            RePose();
        }

        public override void ShowGood(Good good)
        {
            IsShow = true;
            var name = "无名称";
            var cost = "价格： 0";
            var user = "";
            var effect = new StringBuilder();
            var intro = "无简介";
            if (good != null)
            {
                if (!string.IsNullOrEmpty(good.Name))
                    name = good.Name;
                cost = "价格： " + good.Cost;
                if (good.IsSellPriceSetted)
                    cost += "\n" + "卖出价： " + good.SellPrice;

                if (good.User != null && good.User.Length > 0)
                    user = ("使用者：" + string.Join("，", good.User));
                if (good.MinUserLevel > 0)
                    user += (string.IsNullOrEmpty(user) ? "" : Environment.NewLine) + "等级需求：" + good.MinUserLevel;

                if (good.Life != 0)
                    effect.AppendLine("命" + good.Life.ToString("+#;-#"));
                if (good.Thew != 0)
                    effect.AppendLine("体" + good.Thew.ToString("+#;-#"));
                if (good.Mana != 0)
                    effect.AppendLine("气" + good.Mana.ToString("+#;-#"));
                if (good.Attack != 0)
                    effect.AppendLine("攻" + good.Attack.ToString("+#;-#"));
                if (good.Attack2 != 0)
                    effect.AppendLine("攻2 " + good.Attack2.ToString("+#;-#"));
                if (good.Defend != 0)
                    effect.AppendLine("防" + good.Defend.ToString("+#;-#"));
                if (good.Defend2 != 0)
                    effect.AppendLine("防2" + good.Defend2.ToString("+#;-#"));
                if (good.Evade != 0)
                    effect.AppendLine("捷" + good.Evade.ToString("+#;-#"));
                if (good.LifeMax != 0)
                    effect.AppendLine("命" + good.LifeMax.ToString("+#;-#"));
                if (good.ThewMax != 0)
                    effect.AppendLine("体" + good.ThewMax.ToString("+#;-#"));
                if (good.ManaMax != 0)
                    effect.AppendLine("气" + good.ManaMax.ToString("+#;-#"));
                if (good.SpecialEffect == 1)
                    effect.AppendLine(string.Format("不断恢复生命 {0}%/秒", good.SpecialEffectValue));
                if (good.AddMagicEffectPercent > 0 || good.AddMagicEffectAmount > 0)
                {
                    effect.AppendLine(string.Format("[{0}] 攻击{1}{2}",
                        string.IsNullOrEmpty(good.AddMagicEffectName) ? "所有武功" : good.AddMagicEffectName,
                        good.AddMagicEffectPercent > 0 ? (" +" + good.AddMagicEffectPercent/100.0f + "%") : "",
                        good.AddMagicEffectAmount > 0 ? (" +" + good.AddMagicEffectAmount) : ""));
                }
                if (!string.IsNullOrEmpty(good.Intro))
                    intro = good.Intro;
            }

            var showText = new StringBuilder();
            showText.AppendLine(String.Format(FormatTemplate, _goodNameColor, name));
            showText.AppendLine(String.Format(FormatTemplate, _goodPriceColor, cost));
            if (!string.IsNullOrEmpty(user))
            {
                showText.AppendLine(String.Format(FormatTemplate, _goodUserColor, user));
            }

            if (effect.Length > 0)
            {
                showText.AppendLine(String.Format(FormatTemplate, _goodPropertyColor, effect.ToString().TrimEnd()));
            }
            showText.AppendFormat(FormatAlignLeft, String.Format(FormatTemplate, _goodIntroColor, intro));
            _text.Text = showText.ToString();
            RePose();
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsShow) return;
            base.Update(gameTime);
            _background.Update(gameTime);
            _text.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsShow) return;
            _background.Draw(spriteBatch);
            _text.Draw(spriteBatch);
        }
    }
}
