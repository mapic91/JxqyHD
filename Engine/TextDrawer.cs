using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Engine
{
    public class TextDrawer
    {
        private SpriteFont _font;
        private string _strToDraw = null;
        private string _drawedStrBuffer = null;
        private int _strBeginPos = 0;
        private Rectangle _region;
        private KeyboardState _lastState;
        private Color _drawColor;

        public Color DrawColor
        {
            get { return _drawColor; }
            set { _drawColor = value; }
        }

        public Rectangle Region
        {
            get { return _region; }
            set { _region = value; }
        }

        public bool IsInDraw
        {
            get { return _drawedStrBuffer != null; }
            set { if (!value) _drawedStrBuffer = null; }
        }

        public string StrToDraw
        {
            get { return _strToDraw; }
        }

        private SpriteFont Font
        {
            get { return _font; }
            set { _font = value; }
        }

        private void Caculate()
        {
            float widthsum = 0f, heightsum = 0f;
            var str = StrToDraw.Substring(_strBeginPos);
            var strLen = str.Length;
            var drawStr = new StringBuilder(strLen);
            int drawStrLen = 0;
            for (var i = 0; i < strLen; i++)
            {
                Vector2 measure = Font.MeasureString(str.Substring(i, 1));
                if (str[i] == '\n')
                {
                    heightsum += Font.LineSpacing;
                    widthsum = 0;
                }
                else
                {
                    widthsum += measure.X;
                }
                if ((int)widthsum > Region.Width)
                {
                    widthsum = 0;
                    drawStr.Append('\n');
                    heightsum += Font.LineSpacing;
                }
                if ((int)heightsum > Region.Height)
                {
                    if (str[i] == '\n') drawStrLen++;
                    break;
                }
                drawStr.Append(str[i]);
                drawStrLen++;
            }
            _strBeginPos += drawStrLen;
            _drawedStrBuffer = drawStr.ToString();
        }
        public void DrawString(SpriteFont font, string str, Rectangle region)
        {
            DrawString(font, str, region, Color.Black);
        }
        public void DrawString(SpriteFont font, string str, Rectangle region, Color color)
        {
            Font = font;
            _strToDraw = str;
            _strBeginPos = 0;
            Region = region;
            DrawColor = color;
            Caculate();
        }



        public void Update(GameTime gameTime)
        {
            var state = Keyboard.GetState();
            if (state.IsKeyDown(Keys.Space) &&
                _lastState.IsKeyUp(Keys.Space))
            {
                if (_strBeginPos >= StrToDraw.Length)
                {
                    IsInDraw = false;
                }
                else
                {
                    Caculate();
                }
            }
            _lastState = state;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if(IsInDraw)
                spriteBatch.DrawString(Font, _drawedStrBuffer, new Vector2(Region.X, Region.Y), DrawColor);
        }


    }
}