using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Engine.Gui
{
    public class DragDropItem : GuiItem
    {
        private TextGui TopLeftTextGui { set; get; }

        public event Action<object, DragEvent> Drag;
        public event Action<object, DropEvent> Drop;
        public object Data { set; get; }

        public string TopLeftText
        {
            set
            {
                if(TopLeftTextGui == null)
                {
                    TopLeftTextGui = new TextGui(this,
                    Vector2.Zero,
                    Width,
                    Height,
                    Globals.FontSize7,
                    0,
                    0,
                    value,
                    new Color(167, 157, 255)*0.8f);
                }
                else
                {
                    TopLeftTextGui.Text = value;
                }
            }
            get
            {
                if (TopLeftTextGui == null)
                    return "";
                return TopLeftTextGui.Text;
            }
        }

        private DragDropItem() { }

        public DragDropItem(GuiItem parent,
            Vector2 position,
            int width,
            int height,
            Texture baseTexture,
            object data = null)
            : base(parent, position, width, height, baseTexture)
        {
            Data = data;
            MouseLeftDown += delegate(object arg1, MouseLeftDownEvent arg2)
            {
                GuiManager.DragDropSourceItem = this;
                GuiManager.IsDropped = false;
                IsShow = false;
                if (Drag != null)
                {
                    Drag(this, new DragEvent(arg2.MouseScreenPosition));
                }
            };

            MouseLeftUp += delegate(object arg1, MouseLeftUpEvent arg2)
            {
                if (GuiManager.DragDropSourceItem != null)
                {
                    if (Drop != null &&
                    InRange)
                    {
                        Drop(this, new DropEvent(arg2.MouseScreenPosition, GuiManager.DragDropSourceItem));
                    }
                    GuiManager.IsDropped = true;
                }

            };
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsShow) return;
            base.Update(gameTime);
            if (TopLeftTextGui != null)
                TopLeftTextGui.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsShow) return;
            base.Draw(spriteBatch);
            if (TopLeftTextGui != null)
                TopLeftTextGui.Draw(spriteBatch);
        }

        public abstract class DragDropEvent : EventArgs
        {
            public Vector2 MouseScreenPosition { private set; get; }

            public DragDropEvent(Vector2 mouseScreenPosition)
            {
                MouseScreenPosition = mouseScreenPosition;
            }
        }

        public class DragEvent : DragDropEvent
        {
            public DragEvent(Vector2 mouseScreenPosition)
                : base(mouseScreenPosition)
            {
            }
        }

        public class DropEvent : DragDropEvent
        {
            public DragDropItem Source { private set; get; }
            public DropEvent(Vector2 mouseScreenPosition, DragDropItem source)
                : base(mouseScreenPosition)
            {
                Source = source;
            }
        }
    }
}