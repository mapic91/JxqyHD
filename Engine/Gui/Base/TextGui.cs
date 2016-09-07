using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Gui.Base
{
    public class TextGui : GuiItem
    {
        private int _startIndex;
        private int _endIndex;
        private readonly LinkedList<Info> _drawInfo = new LinkedList<Info>();
        public SpriteFont Font { set; get; }
        public int ExtureCharacterSpace { set; get; }
        public int ExtureLineSpace { set; get; }

        public string Text
        {
            set
            {
                CurrentColor = DefaultColor;
                TextStream = new StringBuilder(value);
                _startIndex = _endIndex = 0;
                Caculate();
            }
            get { return TextStream.ToString(); }
        }
        private StringBuilder TextStream { set; get; }
        public Color DefaultColor { get; set; }
        protected Color CurrentColor { get; set; }

        public TextGui(GuiItem parent,
            Vector2 position,
            int width,
            int height,
            SpriteFont font,
            string text)
        {
            Init(parent, position, width, height, font, 0, 0, text, Color.Black);
        }

        public TextGui(GuiItem parent,
            Vector2 position,
            int width,
            int height,
            SpriteFont font,
            int extraCharecterSpace,
            int extraLineSpace,
            string text,
            Color defaultColor)
        {
            Init(parent,
                position,
                width,
                height,
                font,
                extraCharecterSpace,
                extraLineSpace,
                text,
                defaultColor);
        }

        public void Init(GuiItem parent,
            Vector2 position,
            int width,
            int height,
            SpriteFont font,
            int extureCharecterSpace,
            int extureLineSpace,
            string text,
            Color defaultColor)
        {
            Parent = parent;
            Position = position;
            Width = width;
            Height = height;
            Font = font;
            ExtureCharacterSpace = extureCharecterSpace;
            ExtureLineSpace = extureLineSpace;
            DefaultColor = defaultColor;
            Text = text;// must be the last, because it invoke Caculate()
        }

        private bool IsReachRight(float x, float characterWidth)
        {
            return x + characterWidth + ExtureCharacterSpace > Width;
        }

        private bool IsReachBottom(float y)
        {
            return y + Font.LineSpacing + ExtureLineSpace > Height;
        }

        private void AddWdith(ref float x, float charecterWidth)
        {
            x += (charecterWidth + ExtureCharacterSpace);
        }

        private void AddLinespace(ref float y)
        {
            y += (Font.LineSpacing + ExtureLineSpace);
        }

        public bool NextPage()
        {
            return Caculate();
        }

        private static readonly Regex RegColor = new Regex(@"^color=([0-9]*),([0-9]*),([0-9]*)");
        public bool Caculate()
        {
            _drawInfo.Clear();
            try
            {
                if (TextStream == null ||
                    _endIndex >= TextStream.Length)
                    return false;

                _startIndex = _endIndex;
                var x = 0f;
                var y = 0f;
                var endIndex = TextStream.Length;
                while (_endIndex < endIndex)
                {
                    var drawText = TextStream[_endIndex].ToString();
                    if (drawText == "<")
                    {
                        var text = new StringBuilder();
                        while (TextStream[++_endIndex] != '>')
                        {
                            text.Append(TextStream[_endIndex]);
                        }

                        var textStr = text.ToString();
                        switch (textStr)
                        {
                            case "color=Red":
                                CurrentColor = Color.Red * (DefaultColor.A / 255f);
                                break;
                            case "color=Black":
                                CurrentColor = Color.Black * (DefaultColor.A / 255f);
                                break;
                            case "color=Default":
                                CurrentColor = DefaultColor;
                                break;
                            case "enter":
                                AddLinespace(ref y);
                                x = 0;
                                if (IsReachBottom(y))
                                {
                                    _endIndex++;
                                    return true;
                                }
                                break;
                            default:
                                if (RegColor.IsMatch(textStr))
                                {
                                    var matchs = RegColor.Match(textStr);
                                    var r = matchs.Groups[1].Value;
                                    var g = matchs.Groups[2].Value;
                                    var b = matchs.Groups[3].Value;
                                    int rv = 0, gv = 0, bv = 0;
                                    int.TryParse(r, out rv);
                                    int.TryParse(g, out gv);
                                    int.TryParse(b, out bv);
                                    CurrentColor = new Color(rv,gv,bv) * (DefaultColor.A / 255f);
                                }
                                break;
                        }
                    }
                    else if (drawText == "\n")
                    {
                        AddLinespace(ref y);
                        x = 0;
                        if (IsReachBottom(y))
                        {
                            _endIndex++;
                            return true;
                        }
                    }
                    else
                    {
                        var stringWidth = Font.MeasureString(drawText).X;
                        //Make space width correct
                        if (drawText == " ")
                        {
                            if (_endIndex + 1 < endIndex &&
                            TextStream[_endIndex + 1] == ' ')
                            {
                                stringWidth = 2 * Font.MeasureString("0").X;
                                _endIndex++;
                            }
                            else
                            {
                                stringWidth = Font.MeasureString("0").X;
                            }
                        }
                        if (IsReachRight(x, stringWidth))
                        {
                            AddLinespace(ref y);
                            if (IsReachBottom(y))
                                return true;
                            x = 0f;
                        }
                        _drawInfo.AddLast(new Info(
                            drawText,
                            ScreenPosition + new Vector2(x, y),
                            CurrentColor));
                        AddWdith(ref x, stringWidth);
                    }
                    _endIndex++;
                }
            }
            catch (Exception)
            {
                _drawInfo.Clear();
                _endIndex = TextStream.Length;
                Log.LogMessageToFile("String [" + TextStream + "] format is bad!");
                return false;
            }
            return true;
        }

        /// <summary>
        /// Set default color and current text draw color
        /// </summary>
        /// <param name="color">Color</param>
        public void SetDrawColor(Color color)
        {
            DefaultColor = color;
            CurrentColor = color;
            foreach (var info in _drawInfo)
            {
                info.DrawColor = color;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsShow) return;
            base.Draw(spriteBatch);
            foreach (var info in _drawInfo)
            {
                spriteBatch.DrawString(Font,
                    info.Text,
                    info.Position,
                    info.DrawColor);
            }
        }

        private class Info
        {
            public string Text;
            public Vector2 Position;
            public Color DrawColor;

            public Info(string text, Vector2 position, Color drawColor)
            {
                Text = text;
                Position = position;
                DrawColor = drawColor;
            }
        }
    }
}