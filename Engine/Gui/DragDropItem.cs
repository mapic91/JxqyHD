using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Engine.Gui
{
    public class DragDropItem : GuiItem
    {
        public event Action<object, DragEvent> Drag;
        public event Action<object, DropEvent> Drop;
        public object Data { set; get; }

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