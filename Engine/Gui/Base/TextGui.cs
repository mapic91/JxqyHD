using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Engine.Gui.Base
{
    public class TextGui : GuiItem
    {
        private bool _autoHeight;
        private bool _inRange;
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

        /// <summary>
        /// event fired when mouse is in real text rectangle area
        /// </summary>
        public event Action<object, MouseEvent> MouseEnterText;
        public event Action<object, MouseEvent> MouseLeaveText;
        public event Action<object, MouseLeftDownEvent> MouseLeftDownText;

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
        /// <summary>
        /// color to override text color
        /// </summary>
        public Color OverrideColor { get; set; }
        /// <summary>
        /// If true, text color while use OverrideColor
        /// </summary>
        public bool UseOverrideColor { get; set; }

        private Color ActiveDefaultColor
        {
            get { return _isInRangeDefaultColor ? _rangeDefaultColor : DefaultColor; }
        }
        private bool _isInRangeDefaultColor;
        private Color _rangeDefaultColor;
        protected Rectangle _textRect;

        /// <summary>
        /// rectangle in screen which compress all text
        /// </summary>
        public Rectangle RealScreenRectangle
        {
            get
            {
                return new Rectangle((int)ScreenPosition.X + _textRect.X, (int)ScreenPosition.Y + _textRect.Y, _textRect.Width,
                    _textRect.Height);
            }
        }

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
            _autoHeight = height <= 0;
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

            MouseMove += (arg1, arg2) =>
            {
                if (RealScreenRectangle.Contains(new Point((int)arg2.MouseScreenPosition.X, (int)arg2.MouseScreenPosition.Y)))
                {
                    _inRange = true;
                    MouseEnterText?.Invoke(arg1, arg2);
                }
                else if (_inRange && !RealScreenRectangle.Contains(new Point((int)arg2.MouseScreenPosition.X, (int)arg2.MouseScreenPosition.Y)))
                {
                    _inRange = false;
                    MouseLeaveText?.Invoke(arg1, arg2);
                }
            };
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
                            new Vector2(stringWidth, Font.LineSpacing + ExtureLineSpace),
                            CurrentColor,
                            CurrentColor == ActiveDefaultColor));
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
                _textRect = new Rectangle(0, 0, Width, Height);
                Log.LogMessage("String [" + TextStream + "] format is bad!");
                return false;
            }

            if (_drawInfo.Count == 0)
            {
                _textRect = new Rectangle(0, 0, Width, Height);
            }
            else
            {
                var minX = _drawInfo.Min(info => info.Position.X);
                var minY = _drawInfo.Min(info => info.Position.Y);
                var maxX = _drawInfo.Max(info => info.Position.X + info.Size.X);
                var maxY = _drawInfo.Max(info => info.Position.Y + info.Size.Y);
                _textRect.X = (int)minX;
                _textRect.Y = (int)minY;
                _textRect.Width = (int)Math.Ceiling(maxX - minX);
                _textRect.Height = (int)Math.Ceiling(maxY - minY);
            }

            if (_autoHeight)
            {
                Height = RealHeight;
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
            var screenPosition = ScreenPosition;
            foreach (var info in _drawInfo)
            {
                spriteBatch.DrawString(Font,
                    info.Text,
                    info.Position + screenPosition,
                    UseOverrideColor ? OverrideColor : info.DrawColor);
            }
        }

        public override void Update(GameTime gameTime)
        {
            var mouseState = Mouse.GetState();
            if (mouseState.LeftButton == ButtonState.Pressed &&
                _lastMouseState.LeftButton == ButtonState.Released)
            {
                var screenPosition = new Vector2(mouseState.X, mouseState.Y);
                if (RealScreenRectangle.Contains(new Point((int)screenPosition.X, (int)screenPosition.Y)))
                {
                    MouseLeftDownText?.Invoke(this, new MouseLeftDownEvent(screenPosition - ScreenPosition, screenPosition));
                }
            }
            base.Update(gameTime);
        }

        private class Info
        {
            public string Text;
            public Vector2 Position;
            public Vector2 Size;
            public Color DrawColor;
            public bool IsDefaultColor;

            public Info(string text, Vector2 position, Vector2 size, Color drawColor, bool isDefaultColor)
            {
                Text = text;
                Position = position;
                Size = size;
                DrawColor = drawColor;
                IsDefaultColor = isDefaultColor;
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