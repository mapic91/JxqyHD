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
        private int _drawInfoLineBegin;
        private readonly List<Info> _drawInfo = new List<Info>();
        public SpriteFont Font { set; get; }
        public int ExtureCharacterSpace { set; get; }
        public int ExtureLineSpace { set; get; }
        public Align TextAlign = Align.Left;
        /// <summary>
        /// Real text width height drawed.
        /// </summary>
        public int RealWidth;
        public int RealHeight;

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

        private Color ActiveDefaultColor
        {
            get { return _isInRangeDefaultColor ? _rangeDefaultColor : DefaultColor; }
        }
        private bool _isInRangeDefaultColor;
        private Color _rangeDefaultColor;

        private Align ActiveAlign
        {
            get { return _isInRangeAlign ? _rangeAlign : TextAlign; }
        }
        private bool _isInRangeAlign;
        private Align _rangeAlign;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="position"></param>
        /// <param name="width">If less equal then 0, width is not restricted.</param>
        /// <param name="height">If less equal then 0, height is not restricted.</param>
        /// <param name="font"></param>
        /// <param name="text"></param>
        /// <param name="align"></param>
        public TextGui(GuiItem parent,
            Vector2 position,
            int width,
            int height,
            SpriteFont font,
            string text,
            Align align = Align.Left)
        {
            Init(parent, position, width, height, font, 0, 0, text, align, Color.Black);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="position"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="font"></param>
        /// <param name="extraCharecterSpace"></param>
        /// <param name="extraLineSpace"></param>
        /// <param name="text"></param>
        /// <param name="defaultColor"></param>
        /// <param name="align"></param>
        public TextGui(GuiItem parent,
            Vector2 position,
            int width,
            int height,
            SpriteFont font,
            int extraCharecterSpace,
            int extraLineSpace,
            string text,
            Color defaultColor,
            Align align = Align.Left)
        {
            Init(parent,
                position,
                width,
                height,
                font,
                extraCharecterSpace,
                extraLineSpace,
                text,
                align,
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
            Align align,
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
            TextAlign = align;
            Text = text;// must be the last, because it invoke Caculate()
        }

        private bool IsReachRight(float x, float characterWidth)
        {
            return Width > 0 && x + characterWidth + ExtureCharacterSpace > Width;
        }

        private bool IsReachBottom(float y)
        {
            return Height > 0 && y + Font.LineSpacing + ExtureLineSpace > Height;
        }

        private void AddWdith(ref float x, float charecterWidth)
        {
            x += (charecterWidth + ExtureCharacterSpace);
        }

        private void AddLinespace(ref float y)
        {
            y += (Font.LineSpacing + ExtureLineSpace);
        }

        private void AlignLineText(float x)
        {
            var drawInfoLineEnd = _drawInfo.Count;
            if (_drawInfoLineBegin < drawInfoLineEnd)
            {
                var offset = Vector2.Zero;
                if (ActiveAlign == Align.Center)
                {
                    offset.X = (int)(Width - x) / 2;
                }
                else if (ActiveAlign == Align.Right)
                {
                    offset.X = (int)(Width - x);
                }

                if (offset != Vector2.Zero)
                {
                    for (var i = _drawInfoLineBegin; i < drawInfoLineEnd; i++)
                    {
                        _drawInfo[i].Position += offset;
                    }
                }
            }

            _drawInfoLineBegin = drawInfoLineEnd;
        }

        public bool NextPage()
        {
            return Caculate();
        }

        private static readonly Regex RegColor = new Regex(@"^color=([0-9]*),([0-9]*),([0-9]*)");
        private static readonly Regex RegColorWithAlpha = new Regex(@"^color=([0-9]*),([0-9]*),([0-9]*),([0-9]*)");
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
                _drawInfoLineBegin = _drawInfo.Count;
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
                                CurrentColor = Color.Red * (ActiveDefaultColor.A / 255f);
                                break;
                            case "color=Black":
                                CurrentColor = Color.Black * (ActiveDefaultColor.A / 255f);
                                break;
                            case "color=Default":
                                CurrentColor = ActiveDefaultColor;
                                break;
                            case "color=BeginRangeDefault":
                                //Use current color as default color in this range.
                                _isInRangeDefaultColor = true;
                                _rangeDefaultColor = CurrentColor;
                                break;
                            case "color=EndRangeDefault":
                                //Range end
                                _isInRangeDefaultColor = false;
                                break;
                            case "AlignLeft":
                                _isInRangeAlign = true;
                                _rangeAlign = Align.Left;
                                break;
                            case "AlignCenter":
                                _isInRangeAlign = true;
                                _rangeAlign = Align.Center;
                                break;
                            case "AlignRight":
                                _isInRangeAlign = true;
                                _rangeAlign = Align.Right;
                                break;
                            case "EndAlign":
                                AlignLineText(x);
                                _isInRangeAlign = false;
                                break;
                            case "enter":
                                AlignLineText(x);
                                AddLinespace(ref y);
                                x = 0;
                                if (IsReachBottom(y))
                                {
                                    RealHeight = (int)y;
                                    _endIndex++;
                                    return true;
                                }
                                break;
                            default:
                                if (RegColorWithAlpha.IsMatch(textStr))
                                {
                                    var matchs = RegColorWithAlpha.Match(textStr);
                                    var r = matchs.Groups[1].Value;
                                    var g = matchs.Groups[2].Value;
                                    var b = matchs.Groups[3].Value;
                                    var a = matchs.Groups[4].Value;
                                    int rv = 0, gv = 0, bv = 0, av = 0;
                                    int.TryParse(r, out rv);
                                    int.TryParse(g, out gv);
                                    int.TryParse(b, out bv);
                                    int.TryParse(a, out av);
                                    CurrentColor = new Color(rv, gv, bv) * (av/255f);
                                }
                                else if (RegColor.IsMatch(textStr))
                                {
                                    var matchs = RegColor.Match(textStr);
                                    var r = matchs.Groups[1].Value;
                                    var g = matchs.Groups[2].Value;
                                    var b = matchs.Groups[3].Value;
                                    int rv = 0, gv = 0, bv = 0;
                                    int.TryParse(r, out rv);
                                    int.TryParse(g, out gv);
                                    int.TryParse(b, out bv);
                                    CurrentColor = new Color(rv,gv,bv) * (ActiveDefaultColor.A / 255f);
                                }
                                break;
                        }
                    }
                    else if (drawText == "\n")
                    {
                        AlignLineText(x);
                        AddLinespace(ref y);
                        x = 0;
                        if (IsReachBottom(y))
                        {
                            RealHeight = (int)y;
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
                            RealWidth = Math.Max(RealWidth, (int)x);
                            AlignLineText(x);
                            AddLinespace(ref y);
                            if (IsReachBottom(y))
                            {
                                RealHeight = (int)y;
                                return true;
                            }
                            x = 0f;
                        }
                        _drawInfo.Add(new Info(
                            drawText,
                            new Vector2(x, y),
                            CurrentColor));
                        AddWdith(ref x, stringWidth);
                    }
                    _endIndex++;
                }
                AlignLineText(x);
                if(x > 0) AddLinespace(ref y);
                RealHeight = (int)y;
            }
            catch (Exception)
            {
                _drawInfo.Clear();
                _endIndex = TextStream.Length;
                Log.LogMessage("String [" + TextStream + "] format is bad!");
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
                    info.Position + ScreenPosition,
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

        public enum Align
        {
            Left,
            Center,
            Right
        }
    }
}