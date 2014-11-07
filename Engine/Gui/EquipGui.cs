using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Gui
{
    public class EquipGui : GuiItem
    {
        private bool _isFemale;
        private DragDropItem[] _items = new DragDropItem[7];
        public new bool IsShow { set; get; }
        public bool IsFemale
        {
            get { return _isFemale; }
            set
            {
                _isFemale = value;
                var path = @"asf\ui\common\panel7.asf";
                if (value)
                    path = @"asf\ui\common\panel7b.asf";
                BaseTexture = new Texture(Utils.GetAsf(path));
            }
        }

        public EquipGui()
        {
            BaseTexture = new Texture(Utils.GetAsf(@"asf\ui\common\panel7.asf"));
            Width = BaseTexture.Width;
            Height = BaseTexture.Height;
            Position = new Vector2(
                Globals.WindowWidth / 2f - Width,
                0f);

            _items[0] = new DragDropItem(this,//Head
                new Vector2(47, 66),
                60,
                75,
                null,
                new GoodsGui.GoodItemData(201));
            _items[1] = new DragDropItem(this, //Neck
                new Vector2(193, 66),
                60,
                75,
                null,
                new GoodsGui.GoodItemData(202));
            _items[2] = new DragDropItem(this, //Body
                new Vector2(121, 168),
                60,
                75,
                null,
                new GoodsGui.GoodItemData(203));
            _items[3] = new DragDropItem(this, //Back
                new Vector2(193, 267),
                60,
                75,
                null,
                new GoodsGui.GoodItemData(204));
            _items[4] = new DragDropItem(this, //Hand
                new Vector2(193, 168),
                60,
                75,
                null,
                new GoodsGui.GoodItemData(205));
            _items[5] = new DragDropItem(this, //Wrist
                new Vector2(47, 168),
                60,
                75,
                null,
                new GoodsGui.GoodItemData(206));
            _items[6] = new DragDropItem(this,
                new Vector2(47, 267),
                60,
                75,
                null,
                new GoodsGui.GoodItemData(207));
            RegisterEventHandler();
        }

        private void RegisterEventHandler()
        {
            int index, sourceIndex;
            _items[0].Drop += (arg1, arg2) =>
            {
                if (GoodsGui.ExchangeItem(arg1, arg2, out index, out sourceIndex))
                {
                    
                }
            };
            _items[1].Drop += (arg1, arg2) =>
            {
                if (GoodsGui.ExchangeItem(arg1, arg2, out index, out sourceIndex))
                {
                    
                }
            };
            _items[2].Drop += (arg1, arg2) =>
            {
                if (GoodsGui.ExchangeItem(arg1, arg2, out index, out sourceIndex))
                {
                    
                }
            };
            _items[3].Drop += (arg1, arg2) =>
            {
                if (GoodsGui.ExchangeItem(arg1, arg2, out index, out sourceIndex))
                {
                    
                }
            };
            _items[4].Drop += (arg1, arg2) =>
            {
                if (GoodsGui.ExchangeItem(arg1, arg2, out index, out sourceIndex))
                {
                    
                }
            };
            _items[5].Drop += (arg1, arg2) =>
            {
                if (GoodsGui.ExchangeItem(arg1, arg2, out index, out sourceIndex))
                {
                    
                }
            };
            _items[6].Drop += (arg1, arg2) =>
            {
                if (GoodsGui.ExchangeItem(arg1, arg2, out index, out sourceIndex))
                {
                    
                }
            };
        }

        public override void Update(GameTime gameTime)
        {
            if(!IsShow) return;
            base.Update(gameTime);
            foreach (var item in _items)
            {
                item.Update(gameTime);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if(!IsShow) return;
            base.Draw(spriteBatch);
            foreach (var item in _items)
            {
                item.Draw(spriteBatch);
            }
        }

        public void UpdateItems()
        {
            
        }
    }
}