using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Gui.Base
{
    public class ListTextItem : ListItem
    {
        private Color _textColor = Color.Black;
        private Color _seclectedTextColor = Color.Black;
        private TextGui[] _items;

        public ListTextItem(
            GuiItem parent,
            Vector2 position,
            int width,
            int height,
            Texture baseTexture,
            int itemCount,
            string[] itemText,
            int extraCharecterSpace,
            int extraLineSpace,
            int itemHeight,
            Color textColor,
            Color selectedTextColor
            )
            : base(parent, position, width, height, baseTexture)
        {
            _textColor = textColor;
            _seclectedTextColor = selectedTextColor;

            //Create items
            _items = new TextGui[itemCount];
            for (var i = 0; i < itemCount; i++)
            {
                _items[i] = new TextGui(this, 
                    new Vector2(0, i * itemHeight), 
                    width, 
                    itemHeight, 
                    Globals.FontSize12,
                    extraCharecterSpace,
                    extraLineSpace,
                    itemText[i],
                    textColor);

                int i1 = i;
                _items[i].Click += (arg1, arg2) =>
                {
                    //Handle item click event
                    OnItemClick(new ListItemClickEvent(i1));

                    //Item selection change, change item text color.
                    foreach (var item in _items)
                    {
                        item.SetDrawColor(_textColor);
                    }
                    _items[i1].SetDrawColor(_seclectedTextColor);
                };
            }
        }

        public override void Update(GameTime gameTime)
        {
            if(!IsShow || _items == null) return;
            base.Update(gameTime);
            foreach (var item in _items)
            {
                item.Update(gameTime);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsShow || _items == null) return;
            base.Draw(spriteBatch);
            foreach (var item in _items)
            {
                item.Draw(spriteBatch);
            }
        }
    }
}