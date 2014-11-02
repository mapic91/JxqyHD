using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Engine.Gui
{
    public class GuiItem
    {
        private MouseState _lastMouseState;
        private bool _isShow = true;
        private bool _isClicked;
        private Texture _baseTexture = new Texture();
        public event Action<object, MouseMoveEvent> MouseMove;
        public event Action<object, MouseLeftDownEvent> MouseLeftDown;
        public event Action<object, MouseLeftUpEvent> MouseLeftUp;
        public event Action<object, MouseRightDownEvent> MouseRightDown;
        public event Action<object, MouseRightUpEvent> MouseRightUp;
        public event Action<object, MouseLeftClickEvent> Click;
        public event Action<object, MouseRightClickEvent> RightClick;

        public bool IsShow
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
                IsMouveOver = false;
            }
        }

        public Texture BaseTexture
        {
            get { return _baseTexture; }
            set { _baseTexture = value ?? new Texture(); }
        }

        public Vector2 Position { get; set; }
        public Texture MouseOverTexture { get; set; }
        public Texture ClickedTexture { get; set; }
        public bool IsMouveOver { get; set; }

        public Rectangle Region
        {
            get
            {
                return new Rectangle((int)Position.X, 
                    (int)Position.Y,
                    BaseTexture.Width,
                    BaseTexture.Height);
            }
        }

        public GuiItem() { }

        public GuiItem(Vector2 position,
            Texture baseTexture,
            Texture mouseOverTexture = null,
            Texture clickedTexture = null)
        {
            Position = position;
            BaseTexture = baseTexture;
            MouseOverTexture = mouseOverTexture;
            ClickedTexture = clickedTexture;
        }

        public void Update(GameTime gameTime)
        {
            if(!IsShow) return;
            var mouseState = Mouse.GetState();
            if (Region.Contains(mouseState.X, mouseState.Y))
            {
                IsMouveOver = true;
            }
            else
            {
                IsMouveOver = false;
                return;
            }

            var position = new Vector2(mouseState.X, mouseState.Y);
            var lastPosition = new Vector2(_lastMouseState.X, _lastMouseState.Y);
            if (lastPosition != position)
            {
                if (MouseMove != null)
                {
                    MouseMove(this, 
                        new MouseMoveEvent(position, 
                            mouseState.LeftButton == ButtonState.Pressed,
                            mouseState.RightButton == ButtonState.Pressed));
                }
            }

            if (mouseState.LeftButton == ButtonState.Pressed &&
                _lastMouseState.LeftButton == ButtonState.Released)
            {
                IsClicked = true;
                if (MouseLeftDown != null)
                {
                    MouseLeftDown(this, new MouseLeftDownEvent(position));
                }
            }
            else IsClicked = false;

            if (mouseState.LeftButton == ButtonState.Released &&
                _lastMouseState.LeftButton == ButtonState.Pressed)
            {
                if (MouseLeftUp != null)
                {
                    MouseLeftUp(this, new MouseLeftUpEvent(position));
                }

                if (Click != null)
                {
                    Click(this, new MouseLeftClickEvent(position));
                }
            }

            if (mouseState.RightButton == ButtonState.Pressed &&
                _lastMouseState.RightButton == ButtonState.Released)
            {
                if (MouseRightDown != null)
                {
                    MouseRightDown(this, new MouseRightDownEvent(position));
                }
            }

            if (mouseState.RightButton == ButtonState.Released &&
                _lastMouseState.RightButton == ButtonState.Pressed)
            {
                if (MouseRightUp != null)
                {
                    MouseRightUp(this, new MouseRightUpEvent(position));
                }

                if (RightClick != null)
                {
                    RightClick(this, new MouseRightClickEvent(position));
                }
            }

            _lastMouseState = mouseState;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            if(!IsShow) return;

            if (IsClicked && ClickedTexture != null)
            {
                ClickedTexture.Draw(spriteBatch, Position);
            }
            else if (IsMouveOver && MouseOverTexture != null)
            {
                MouseOverTexture.Draw(spriteBatch, Position);
            }
            else if (BaseTexture != null)
            {
                BaseTexture.Draw(spriteBatch, Position);
            }
        }

        #region EventArgs class
        public class MouseMoveEvent : EventArgs
        {
            public Vector2 MousePosition { private set; get; }
            public bool LeftDown { private set; get; }
            public bool RightDown { private set; get; }

            public MouseMoveEvent(Vector2 mousePosition, bool leftDown, bool rightDown)
            {
                MousePosition = mousePosition;
                LeftDown = leftDown;
                RightDown = rightDown;
            }
        }

        public class MouseLeftDownEvent : EventArgs
        {
            public Vector2 MousePosition { private set; get; }
            public MouseLeftDownEvent(Vector2 mousePosition)
            {
                MousePosition = mousePosition;
            }
        }

        public class MouseLeftUpEvent : EventArgs
        {
            public Vector2 MousePosition { private set; get; }
            public MouseLeftUpEvent(Vector2 mousePosition)
            {
                MousePosition = mousePosition;
            }
        }

        public class MouseRightDownEvent : EventArgs
        {
            public Vector2 MousePosition { private set; get; }
            public MouseRightDownEvent(Vector2 mousePosition)
            {
                MousePosition = mousePosition;
            }
        }

        public class MouseRightUpEvent : EventArgs
        {
            public Vector2 MousePosition { private set; get; }
            public MouseRightUpEvent(Vector2 mousePosition)
            {
                MousePosition = mousePosition;
            }
        }

        public class MouseLeftClickEvent : EventArgs
        {
            public Vector2 MousePosition { private set; get; }
            public MouseLeftClickEvent(Vector2 mousePosition)
            {
                MousePosition = mousePosition;
            }
        }

        public class MouseRightClickEvent : EventArgs
        {
            public Vector2 MousePosition { private set; get; }
            public MouseRightClickEvent(Vector2 mousePosition)
            {
                MousePosition = mousePosition;
            }
        }
        #endregion EventArgs class
    }
}