using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Engine.Gui.Base
{
    public class GuiItem
    {
        protected MouseState _lastMouseState;
        private bool _isShow = true;
        private bool _isClicked;
        private bool _isRightClicked;
        private bool _isStayOver;
        private float _stayOverMilliSecond;
        public event Action<object, MouseEvent> MouseLeave;
        public event Action<object, MouseEvent> MouseEnter; 
        public event Action<object, MouseEvent> MouseStayOver;
        public event Action<object, MouseMoveEvent> MouseMove;
        public event Action<object, MouseLeftDownEvent> MouseLeftDown;
        public event Action<object, MouseLeftUpEvent> MouseLeftUp;
        public event Action<object, MouseLeftClickingEvent> MouseLeftClicking;
        public event Action<object, MouseRightDownEvent> MouseRightDown;
        public event Action<object, MouseRightUpEvent> MouseRightUp;
        public event Action<object, MouseLeftClickEvent> Click;
        public event Action<object, MouseRightClickEvent> RightClick;
        public event Action<object, MouseEvent> MouseScrollUp;
        public event Action<object, MouseEvent> MouseScrollDown;
        public event Action<object, GameTime> OnUpdate;

        public virtual bool IsShow
        {
            get { return _isShow; }
            set { _isShow = value; }
        }

        public bool IsClicked
        {
            get { return _isClicked; }
            set
            {
                _isClicked = value;
                if (value) IsMouveOver = false;
            }
        }

        public bool IsRightClicked
        {
            get { return _isRightClicked; }
            set
            {
                _isRightClicked = value;
            }
        }

        /// <summary>
        /// Is mouse on top of this gui item.
        /// </summary>
        public bool InRange { get; set; }

        public Vector2 ScreenPosition
        {
            get
            {
                if (Parent != null)
                {
                    return Parent.ScreenPosition + Position;
                }
                return Position;
            }
        }

        public Vector2 CenterScreenPosition
        {
            get { return ScreenPosition + new Vector2(Width/2f, Height/2f); }
        }

        public Vector2 Position { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public Texture BaseTexture { get; set; }
        public Texture MouseOverTexture { get; set; }
        public Texture ClickedTexture { get; set; }
        public SoundEffect EnteredSound { get; set; }
        public SoundEffect ClickedSound { get; set; }
        public bool IsMouveOver { get; set; }
        public GuiItem Parent { get; set; }

        public Rectangle RegionInScreen
        {
            get
            {
                return new Rectangle((int)ScreenPosition.X,
                    (int)ScreenPosition.Y,
                    Width,
                    Height);
            }
        }

        public GuiItem() { }

        public GuiItem(
            GuiItem parent,
            Vector2 position,
            int width,
            int height,
            Texture baseTexture,
            Texture mouseOverTexture = null,
            Texture clickedTexture = null,
            SoundEffect enteredSound = null,
            SoundEffect clickedSound = null)
        {
            Parent = parent;
            Position = position;
            Width = width;
            Height = height;
            BaseTexture = baseTexture;
            MouseOverTexture = mouseOverTexture;
            ClickedTexture = clickedTexture;
            EnteredSound = enteredSound;
            ClickedSound = clickedSound;
        }

        public Vector2 ToLocalPosition(Vector2 screenPositon)
        {
            return screenPositon - ScreenPosition;
        }

        public virtual void Update(GameTime gameTime)
        {
            if (!IsShow) return;
            var mouseState = Mouse.GetState();
            if (Globals.IsInputDisabled)
                mouseState = Utils.GetMouseStateJustPosition(mouseState);
            var screenPosition = new Vector2(mouseState.X, mouseState.Y);
            var position = screenPosition - ScreenPosition;
            var lastPosition = new Vector2(_lastMouseState.X, _lastMouseState.Y) - ScreenPosition;

            if (OnUpdate != null)
            {
                OnUpdate(this, gameTime);
            }

            if (RegionInScreen.Contains(mouseState.X, mouseState.Y))
            {
                if (InRange == false)
                {
                    if (EnteredSound != null)
                    {
                        EnteredSound.Play();
                    }
                    if (MouseEnter != null)
                    {
                        MouseEnter(this, new MouseEvent(position, screenPosition));
                    }
                } 

                if (lastPosition == position && !_isStayOver)
                {
                    _stayOverMilliSecond += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (_stayOverMilliSecond > 500)
                    {
                        _isStayOver = true;
                        if (MouseStayOver != null)
                        {
                            MouseStayOver(this, new MouseEvent(position, screenPosition));
                        }
                    }
                }

                if (mouseState.LeftButton == ButtonState.Pressed &&
                _lastMouseState.LeftButton == ButtonState.Released)
                {
                    IsClicked = true;
                    if (ClickedTexture != null) ClickedTexture.CurrentFrameIndex = 0;

                    if (MouseLeftDown != null)
                    {
                        MouseLeftDown(this, new MouseLeftDownEvent(position, screenPosition));
                    }

                    if (ClickedSound != null) ClickedSound.Play();
                }

                if (mouseState.LeftButton == ButtonState.Released &&
                    _lastMouseState.LeftButton == ButtonState.Pressed &&
                    IsClicked)
                {
                    if (Click != null)
                    {
                        Click(this, new MouseLeftClickEvent(position, screenPosition));
                    }
                }

                if (mouseState.RightButton == ButtonState.Pressed &&
                    _lastMouseState.RightButton == ButtonState.Released)
                {
                    IsRightClicked = true;
                    if (MouseRightDown != null)
                    {
                        MouseRightDown(this, new MouseRightDownEvent(position, screenPosition));
                    }

                    if (RightClick != null)
                    {
                        RightClick(this, new MouseRightClickEvent(position, screenPosition));
                    }
                }

                if (MouseScrollHandler.IsScrollUp && 
                    MouseScrollUp != null)
                {
                    MouseScrollUp(this, new MouseEvent(position, screenPosition));
                }
                if (MouseScrollHandler.IsScrollDown &&
                    MouseScrollDown != null)
                {
                    MouseScrollDown(this, new MouseEvent(position, screenPosition));
                }

                InRange = true;
                IsMouveOver = true;
            }
            else
            {
                if (InRange)
                {
                    if (MouseLeave != null)
                    {
                        MouseLeave(this, new MouseEvent(position, ScreenPosition));
                    }
                }
                InRange = false;
                IsMouveOver = false;
                _isStayOver = false;
                _stayOverMilliSecond = 0;
            }

            if (mouseState.LeftButton == ButtonState.Released &&
                    _lastMouseState.LeftButton == ButtonState.Pressed)
            {
                if (MouseLeftUp != null)
                {
                    MouseLeftUp(this, new MouseLeftUpEvent(position, screenPosition));
                }

                IsClicked = false;
            }

            if (mouseState.RightButton == ButtonState.Released &&
                    _lastMouseState.RightButton == ButtonState.Pressed)
            {
                if (MouseRightUp != null)
                {
                    MouseRightUp(this, new MouseRightUpEvent(position, screenPosition));
                }

                IsRightClicked = false;
            }

            if (lastPosition != position)
            {
                if (MouseMove != null)
                {
                    MouseMove(this,
                        new MouseMoveEvent(position,
                            screenPosition,
                            mouseState.LeftButton == ButtonState.Pressed,
                            mouseState.RightButton == ButtonState.Pressed));
                }
            }

            if (IsClicked)
            {
                if (MouseLeftClicking != null)
                {
                    MouseLeftClicking(this, new MouseLeftClickingEvent(position, screenPosition));
                }
            }

            if (IsClicked || IsRightClicked)
                GuiManager.IsMouseStateEated = true;

            if (IsClicked && ClickedTexture != null)
                ClickedTexture.Update(gameTime);
            else if (IsMouveOver && MouseOverTexture != null)
                MouseOverTexture.Update(gameTime);
            else if (BaseTexture != null)
                BaseTexture.Update(gameTime);

            _lastMouseState = mouseState;
        }

        public virtual void Draw(SpriteBatch spriteBatch)
        {
            if (!IsShow) return;

            if (IsClicked && ClickedTexture != null)
            {
                ClickedTexture.Draw(spriteBatch, ScreenPosition);
            }
            else if (IsMouveOver && MouseOverTexture != null)
            {
                MouseOverTexture.Draw(spriteBatch, ScreenPosition);
            }
            else if (BaseTexture != null)
            {
                BaseTexture.Draw(spriteBatch, ScreenPosition);
            }
        }

        #region EventArgs class

        public class MouseEvent : EventArgs
        {
            public Vector2 MousePosition { private set; get; }
            public Vector2 MouseScreenPosition { private set; get; }

            public MouseEvent(Vector2 mousePosition, Vector2 mouseScreenPosition)
            {
                MousePosition = mousePosition;
                MouseScreenPosition = mouseScreenPosition;
            }
        }
        public class MouseMoveEvent : MouseEvent
        {

            public bool LeftDown { private set; get; }
            public bool RightDown { private set; get; }

            public MouseMoveEvent(Vector2 mousePosition,
                Vector2 mouseScreenPosition,
                bool leftDown,
                bool rightDown)
                : base(mousePosition, mouseScreenPosition)
            {
                LeftDown = leftDown;
                RightDown = rightDown;
            }
        }

        public class MouseLeftDownEvent : MouseEvent
        {
            public MouseLeftDownEvent(Vector2 mousePosition, Vector2 mouseScreenPosition)
                : base(mousePosition, mouseScreenPosition)
            {
            }
        }

        public class MouseLeftUpEvent : MouseEvent
        {
            public MouseLeftUpEvent(Vector2 mousePosition, Vector2 mouseScreenPosition)
                : base(mousePosition, mouseScreenPosition)
            {
            }
        }

        public class MouseRightDownEvent : MouseEvent
        {
            public MouseRightDownEvent(Vector2 mousePosition, Vector2 mouseScreenPosition)
                : base(mousePosition, mouseScreenPosition)
            {
            }
        }

        public class MouseRightUpEvent : MouseEvent
        {
            public MouseRightUpEvent(Vector2 mousePosition, Vector2 mouseScreenPosition)
                : base(mousePosition, mouseScreenPosition)
            {
            }
        }

        public class MouseLeftClickEvent : MouseEvent
        {
            public MouseLeftClickEvent(Vector2 mousePosition, Vector2 mouseScreenPosition)
                : base(mousePosition, mouseScreenPosition)
            {
            }
        }

        public class MouseLeftClickingEvent : MouseEvent
        {
            public MouseLeftClickingEvent(Vector2 mousePosition, Vector2 mouseScreenPosition)
                : base(mousePosition, mouseScreenPosition)
            {
            }
        }

        public class MouseRightClickEvent : MouseEvent
        {
            public MouseRightClickEvent(Vector2 mousePosition, Vector2 mouseScreenPosition)
                : base(mousePosition, mouseScreenPosition)
            {
            }
        }
        #endregion EventArgs class
    }
}