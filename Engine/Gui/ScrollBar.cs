using System;
using Engine.Gui.Base;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Texture = Engine.Gui.Base.Texture;

namespace Engine.Gui
{
    public class ScrollBar : GuiItem
    {
        private GuiItem _slider;
        private float _value;
        private int _minValue;
        private int _maxValue;
        private bool _inDragging;
        private bool _isSliderClicked;
        private float _lastValue;
        private Vector2 _lastMouseScreenPosition;

        public event Action<object, ScrolledEvent> Scrolled; 

        public GuiItem Slider
        {
            protected get { return _slider; }
            set
            {
                _slider = value;
                if (_slider != null)
                {
                    _slider.Parent = this;
                    _slider.Position = Vector2.Zero;
                }
            }
        }

        public float Value
        {
            get { return _value; }
            set
            {
                var lastValue = _value;
                if (value > MaxValue)
                    _value = MaxValue;
                else if (value < MinValue)
                    _value = MinValue;
                else
                    _value = value;

                if (Slider != null)
                {
                    float x = 0;
                    float y = 0;
                    switch (Type)
                    {
                        case ScrollBarType.Vertical:
                            x = 0f;
                            y = (_value - MinValue) * StepLength;
                            break;
                        case ScrollBarType.Horizontal:
                            x = (_value - MinValue) * StepLength;
                            y = 0f;
                            break;
                    }
                    Slider.Position = new Vector2(x, y);
                }

                if ((int)lastValue != (int)_value && Scrolled != null)
                {
                    Scrolled(this, new ScrolledEvent((int)_value));
                }
            }
        }

        public float StepLength
        {
            get
            {
                if (Length == 0) return 1;
                var len = MaxValue - MinValue;
                return len == 0 ? Length : Length / len;
            }
        }

        public float Length { set; get; }

        public int MinValue
        {
            get { return _minValue; }
            set
            {
                _minValue = value;
                Value = Value; // renew slider positon
            }
        }

        public int MaxValue
        {
            get { return _maxValue; }
            set
            {
                _maxValue = value;
                Value = Value; // renew slider positon
            }
        }

        public ScrollBarType Type { set; get; }

        public enum ScrollBarType
        {
            Vertical,
            Horizontal
        }

        public ScrollBar(GuiItem parent,
            int width,
            int height,
            Texture baseTexture,
            ScrollBarType type,
            GuiItem slider,
            Vector2 positon,
            int minValue,
            int maxValue,
            int value)
            : base(parent, positon, width, height, baseTexture)
        {
            Type = type;
            Slider = slider;
            switch (type)
            {
                case ScrollBarType.Vertical:
                    Length = height;
                    break;
                case ScrollBarType.Horizontal:
                    Length = width;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("type");
            }
            MinValue = minValue;
            MaxValue = maxValue;
            Value = value;
            RegisterEvent();
        }

        private void RegisterEvent()
        {
            if (Slider == null) return;
            Slider.MouseLeftDown += (arg1, arg2) =>
            {
                _inDragging = true;
                _isSliderClicked = true;

                _lastValue = _value;
                _lastMouseScreenPosition = arg2.MouseScreenPosition;
            };
            Slider.MouseLeftUp += (arg1, arg2) =>
            {
                _inDragging = false;
            };
            Slider.MouseMove += Slider_MouseMove;

            MouseLeftDown += (arg1, arg2) =>
            {
                if (!_isSliderClicked)
                {
                    _lastValue = _value;
                    _lastMouseScreenPosition = arg2.MouseScreenPosition;
                }
            };
            Click += (arg1, arg2) =>
            {
                if (_isSliderClicked)
                {
                    _isSliderClicked = false;
                    return;
                }

                switch (Type)
                {
                    case ScrollBarType.Horizontal:
                        Value += (arg2.MouseScreenPosition.X > Slider.ScreenPosition.X ? 1 : -1);
                        break;
                    case ScrollBarType.Vertical:
                        Value += (arg2.MouseScreenPosition.Y > Slider.ScreenPosition.Y ? 1 : -1);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            };
        }

        void Slider_MouseMove(object arg1, GuiItem.MouseMoveEvent arg2)
        {
            if (_inDragging)
            {
                float offset = 0;
                switch (Type)
                {
                    case ScrollBarType.Horizontal:
                        offset = arg2.MouseScreenPosition.X - _lastMouseScreenPosition.X;
                        break;
                    case ScrollBarType.Vertical:
                        offset = arg2.MouseScreenPosition.Y - _lastMouseScreenPosition.Y;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                var valueOffset = offset / StepLength;
                if (valueOffset != 0)
                {
                    Value =  _lastValue + valueOffset;
                }
            }

        }

        public override void Update(GameTime gameTime)
        {
            if (!IsShow) return;
            if (Slider != null) Slider.Update(gameTime);
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsShow) return;
            base.Draw(spriteBatch);
            if (Slider != null) Slider.Draw(spriteBatch);
        }

        public abstract class ScrollBarEvent: EventArgs
        {
            public int Value { private set; get; }

            public ScrollBarEvent(int value)
            {
                Value = value;
            }
        }

        public class ScrolledEvent: ScrollBarEvent
        {
            public ScrolledEvent(int value)
                : base(value)
            { }
        }
    }
}