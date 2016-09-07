using System;
using IniParser.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Gui.Base
{
    public class ListView : GuiItem
    {
        private ScrollBar _scrollBar;
        private DragDropItem[] _items = new DragDropItem[9];
        private KeyDataCollection _config;
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
            int rowCouunts,
            KeyDataCollection config,
            string slideButtonImage)
            : base(parent, position, width, height, baseTexture)
        {
            _config = config;
            InitializeItems();
            var slideTexture = Utils.GetAsf(null, slideButtonImage);
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

            MouseScrollUp += (arg1, arg2) => _scrollBar.Value -= 1;
            MouseScrollDown += (arg1, arg2) => _scrollBar.Value += 1;
        }

        private void InitializeItems()
        {
            var c = _config;
            _items[0] = new DragDropItem(this, new Vector2(int.Parse(c["Item_Left_1"]), int.Parse(c["Item_Top_1"])), int.Parse(c["Item_Width_1"]), int.Parse(c["Item_Height_1"]), null);
            _items[1] = new DragDropItem(this, new Vector2(int.Parse(c["Item_Left_2"]), int.Parse(c["Item_Top_2"])), int.Parse(c["Item_Width_2"]), int.Parse(c["Item_Height_2"]), null);
            _items[2] = new DragDropItem(this, new Vector2(int.Parse(c["Item_Left_3"]), int.Parse(c["Item_Top_3"])), int.Parse(c["Item_Width_3"]), int.Parse(c["Item_Height_3"]), null);
            _items[3] = new DragDropItem(this, new Vector2(int.Parse(c["Item_Left_4"]), int.Parse(c["Item_Top_4"])), int.Parse(c["Item_Width_4"]), int.Parse(c["Item_Height_4"]), null);
            _items[4] = new DragDropItem(this, new Vector2(int.Parse(c["Item_Left_5"]), int.Parse(c["Item_Top_5"])), int.Parse(c["Item_Width_5"]), int.Parse(c["Item_Height_5"]), null);
            _items[5] = new DragDropItem(this, new Vector2(int.Parse(c["Item_Left_6"]), int.Parse(c["Item_Top_6"])), int.Parse(c["Item_Width_6"]), int.Parse(c["Item_Height_6"]), null);
            _items[6] = new DragDropItem(this, new Vector2(int.Parse(c["Item_Left_7"]), int.Parse(c["Item_Top_7"])), int.Parse(c["Item_Width_7"]), int.Parse(c["Item_Height_7"]), null);
            _items[7] = new DragDropItem(this, new Vector2(int.Parse(c["Item_Left_8"]), int.Parse(c["Item_Top_8"])), int.Parse(c["Item_Width_8"]), int.Parse(c["Item_Height_8"]), null);
            _items[8] = new DragDropItem(this, new Vector2(int.Parse(c["Item_Left_9"]), int.Parse(c["Item_Top_9"])), int.Parse(c["Item_Width_9"]), int.Parse(c["Item_Height_9"]), null);
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