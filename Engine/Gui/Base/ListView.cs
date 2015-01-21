using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Gui.Base
{
    public class ListView : GuiItem
    {
        private ScrollBar _scrollBar;
        private DragDropItem[] _items = new DragDropItem[9];
        private Vector2 _itemPositinOffset = Vector2.Zero;
        public event Action<object, ListScrollEvent> Scrolled;

        public int CurrentScrollValue
        {
            get
            {
                if (_scrollBar == null)
                    return 0;
                return _scrollBar.Value;
            }
        }

        public ListView(GuiItem parent,
            Vector2 position,
            Vector2 scrollBarPosition,
            int width,
            int height,
            Texture baseTexture,
            int rowCouunts)
            :this(parent, 
            position,
            scrollBarPosition, 
            width, 
            height, 
            baseTexture,
            rowCouunts, 
            Vector2.Zero)
        {
            
        }

        public ListView(GuiItem parent,
            Vector2 position,
            Vector2 scrollBarPosition,
            int width,
            int height,
            Texture baseTexture,
            int rowCouunts,
            Vector2 itemPositionOffset)
            : base(parent, position, width, height, baseTexture)
        {
            _itemPositinOffset = itemPositionOffset;
            InitializeItems();
            var slideTexture = Utils.GetAsf(@"asf\ui\option\", "slidebtn.asf");
            var slideBaseTexture = new Texture(slideTexture);
            var slideClikedTexture = new Texture(slideTexture, 0, 1);
            var slideButton = new GuiItem(this,
                Vector2.Zero,
                slideBaseTexture.Width,
                slideBaseTexture.Height,
                slideBaseTexture,
                null,
                slideClikedTexture,
                null,
                Utils.GetSoundEffect("界-大按钮.wav"));
            _scrollBar = new ScrollBar(this,
                28,
                190,
                null,
                ScrollBar.ScrollBarType.Vertical,
                slideButton,
                scrollBarPosition,
                0,
                rowCouunts - 1 - 2,
                0);
            _scrollBar.Scrolled += delegate(object arg1, ScrollBar.ScrolledEvent arg2)
            {
                if (Scrolled != null)
                    Scrolled(this, new ListScrollEvent(arg2.Value));
            };
        }

        private void InitializeItems()
        {
            _items[0] = new DragDropItem(this, new Vector2(72, 91) + _itemPositinOffset, 60, 75, null);
            _items[1] = new DragDropItem(this, new Vector2(137, 91) + _itemPositinOffset, 60, 75, null);
            _items[2] = new DragDropItem(this, new Vector2(201, 91) + _itemPositinOffset, 60, 75, null);
            _items[3] = new DragDropItem(this, new Vector2(72, 170) + _itemPositinOffset, 60, 75, null);
            _items[4] = new DragDropItem(this, new Vector2(137, 170) + _itemPositinOffset, 60, 75, null);
            _items[5] = new DragDropItem(this, new Vector2(202, 170) + _itemPositinOffset, 60, 75, null);
            _items[6] = new DragDropItem(this, new Vector2(72, 250) + _itemPositinOffset, 60, 75, null);
            _items[7] = new DragDropItem(this, new Vector2(137, 250) + _itemPositinOffset, 60, 75, null);
            _items[8] = new DragDropItem(this, new Vector2(202, 250) + _itemPositinOffset, 60, 75, null);
        }

        public void RegisterItemDragHandler(Action<object, DragDropItem.DragEvent> handler)
        {
            foreach (var dragDropItem in _items)
            {
                dragDropItem.Drag += handler;
            }
        }

        public void RegisterItemDropHandler(Action<object, DragDropItem.DropEvent> handler)
        {
            foreach (var dragDropItem in _items)
            {
                dragDropItem.Drop += handler;
            }
        }

        public void RegisterItemMouseRightClickeHandler(Action<object, MouseRightClickEvent> handler)
        {
            foreach (var dragDropItem in _items)
            {
                dragDropItem.RightClick += handler;
            }
        }

        public void RegisterItemMouseStayOverHandler(Action<object, MouseEvent> handler)
        {
            foreach (var dragDropItem in _items)
            {
                dragDropItem.MouseStayOver += handler;
            }
        }

        public void RegisterItemMouseLeaveHandler(Action<object, MouseEvent> handler)
        {
            foreach (var dragDropItem in _items)
            {
                dragDropItem.MouseLeave += handler;
            }
        }

        /// <summary>
        /// List index begin at 1
        /// </summary>
        /// <param name="itemIndex">range: 0 - 8</param>
        /// <returns></returns>
        public int ToListIndex(int itemIndex)
        {
            return CurrentScrollValue * 3 + itemIndex + 1;
        }

        /// <summary>
        /// List index begin at 1
        /// </summary>
        /// <param name="listIndex"></param>
        /// <param name="index">range: 0 - 8</param>
        /// <returns></returns>
        public bool IsItemShow(int listIndex, out int index)
        {
            var start = ToListIndex(0);
            var end = ToListIndex(8);
            if (listIndex >= start && listIndex <= end)
            {
                index = listIndex - start;
                return true;
            }
            index = 0;
            return false;
        }

        public void SetListItem(int index, Texture texture, object data)
        {
            if (index >= 0 && index < 9)
            {
                _items[index].BaseTexture = texture;
                _items[index].Data = data;
            }
        }

        public void SetListItemTexture(int index, Texture texture)
        {
            if (index >= 0 && index < 9)
            {
                _items[index].BaseTexture = texture;
            }
        }

        public void SetItemTopLeftText(int index, string text)
        {
            if (index >= 0 && index < 9)
            {
                _items[index].TopLeftText = text;
            }
        }

        public void SetMaxRow(int value)
        {
            _scrollBar.MaxValue = value;
        }

        public void ScrollToRow(int value)
        {
            _scrollBar.Value = value;
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsShow) return;

            base.Update(gameTime);
            _scrollBar.Update(gameTime);
            foreach (var dragDropItem in _items)
            {
                dragDropItem.Update(gameTime);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsShow) return;

            base.Draw(spriteBatch);
            foreach (var dragDropItem in _items)
            {
                dragDropItem.Draw(spriteBatch);
            }
            _scrollBar.Draw(spriteBatch);
        }

        #region Event
        public abstract class ListEvent : EventArgs
        {
            public int ScrollValue { private set; get; }

            public ListEvent(int scrollValue)
            {
                ScrollValue = scrollValue;
            }
        }

        public class ListScrollEvent : ListEvent
        {
            public ListScrollEvent(int scrollValue)
                : base(scrollValue)
            { }
        }
        #endregion Event
    }
}