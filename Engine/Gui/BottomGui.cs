using Engine.Gui.Base;
using Engine.ListManager;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Texture = Engine.Gui.Base.Texture;

namespace Engine.Gui
{
    public class BottomGui : GuiItem
    {
        private DragDropItem[] _items = new DragDropItem[8];
        private Texture2D _coldTimeBackground;
        private Color _colodTimeFontColor = Color.White;

        public BottomGui()
        {
            var cfg = GuiManager.Setttings.Sections["Bottom"];
            BaseTexture = new Texture(Utils.GetAsf(null, cfg["Image"]));
            Width = BaseTexture.Width;
            Height = BaseTexture.Height;
            Position = new Vector2(
                (Globals.WindowWidth - BaseTexture.Width)/2f + int.Parse(cfg["LeftAdjust"]),
                Globals.WindowHeight - BaseTexture.Height + int.Parse(cfg["TopAdjust"]));

            InitializeItems();
        }

        private void InitializeItems()
        {
            var cfg = GuiManager.Setttings.Sections["Bottom_Items"];
            var hasCount = GoodsListManager.Type != GoodsListManager.ListType.TypeByGoodItem;
            _items[0] = new DragDropItem(this, 
                new Vector2(int.Parse(cfg["Item_Left_1"]), int.Parse(cfg["Item_Top_1"])),
                int.Parse(cfg["Item_Width_1"]),
                int.Parse(cfg["Item_Height_1"]), 
                null, 
                new GoodsGui.GoodItemData(GoodsListManager.BottomIndexBegin),
                hasCount);
            _items[1] = new DragDropItem(this, 
                new Vector2(int.Parse(cfg["Item_Left_2"]), int.Parse(cfg["Item_Top_2"])),
                int.Parse(cfg["Item_Width_2"]),
                int.Parse(cfg["Item_Height_2"]), 
                null, 
                new GoodsGui.GoodItemData(GoodsListManager.BottomIndexBegin+1),
                hasCount);
            _items[2] = new DragDropItem(this, 
                new Vector2(int.Parse(cfg["Item_Left_3"]), int.Parse(cfg["Item_Top_3"])),
                int.Parse(cfg["Item_Width_3"]),
                int.Parse(cfg["Item_Height_3"]), 
                null, 
                new GoodsGui.GoodItemData(GoodsListManager.BottomIndexBegin+2),
                hasCount);
            _items[3] = new DragDropItem(this, 
                new Vector2(int.Parse(cfg["Item_Left_4"]), int.Parse(cfg["Item_Top_4"])),
                int.Parse(cfg["Item_Width_4"]),
                int.Parse(cfg["Item_Height_4"]), 
                null, 
                new MagicGui.MagicItemData(MagicListManager.BottomIndexBegin));
            _items[4] = new DragDropItem(this, 
                new Vector2(int.Parse(cfg["Item_Left_5"]), int.Parse(cfg["Item_Top_5"])),
                int.Parse(cfg["Item_Width_5"]),
                int.Parse(cfg["Item_Height_5"]), 
                null, 
                new MagicGui.MagicItemData(MagicListManager.BottomIndexBegin+1));
            _items[5] = new DragDropItem(this,
                new Vector2(int.Parse(cfg["Item_Left_6"]), int.Parse(cfg["Item_Top_6"])),
                int.Parse(cfg["Item_Width_6"]),
                int.Parse(cfg["Item_Height_6"]), 
                null, 
                new MagicGui.MagicItemData(MagicListManager.BottomIndexBegin+2));
            _items[6] = new DragDropItem(this, 
                new Vector2(int.Parse(cfg["Item_Left_7"]), int.Parse(cfg["Item_Top_7"])),
                int.Parse(cfg["Item_Width_7"]),
                int.Parse(cfg["Item_Height_7"]), 
                null, 
                new MagicGui.MagicItemData(MagicListManager.BottomIndexBegin+3));
            _items[7] = new DragDropItem(this, 
                new Vector2(int.Parse(cfg["Item_Left_8"]), int.Parse(cfg["Item_Top_8"])),
                int.Parse(cfg["Item_Width_8"]),
                int.Parse(cfg["Item_Height_8"]), 
                null, 
                new MagicGui.MagicItemData(MagicListManager.BottomIndexBegin+4));

            for (var i = 0; i < 3; i++)
            {
                _items[i].Drag += (arg1, arg2) =>
                {

                };
                _items[i].Drop += GoodsGui.DropHandler;
                _items[i].RightClick += GoodsGui.RightClickHandler;
                _items[i].MouseStayOver += GoodsGui.MouseStayOverHandler;
                _items[i].MouseLeave += GoodsGui.MouseLeaveHandler;
            }

            for (var i = 3; i < 8; i++)
            {
                _items[i].Drag += (arg1, arg2) =>
                {

                };
                _items[i].Drop += MagicGui.DropHandler;
                _items[i].RightClick += (arg1, arg2) =>
                {
                    var data = (MagicGui.MagicItemData)(((DragDropItem)arg1).Data);
                    var info = MagicListManager.GetItemInfo(data.Index);
                    if (info != null)
                    {
                        Globals.ThePlayer.CurrentMagicInUse = info;
                    }
                };
                _items[i].MouseStayOver += MagicGui.MouseStayOverHandler;
                _items[i].MouseLeave += MagicGui.MouseLeaveHandler;
                _items[i].OnUpdate += (arg1, arg2) =>
                {
                    var data = (MagicGui.MagicItemData)(((DragDropItem)arg1).Data);
                    var info = MagicListManager.GetItemInfo(data.Index);
                    if (info != null)
                    {
                        var gameTime = (GameTime)arg2;
                        info.RemainColdMilliseconds -= (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    }
                };
            }
        }

        public int ToMagicListIndex(int itemIndex)
        {
            return itemIndex - 3 + MagicListManager.BottomIndexBegin;
        }

        public int ToGoodsListIndex(int itemIndex)
        {
            return itemIndex + GoodsListManager.BottomIndexBegin;
        }

        public void SetItem(int index, Texture texture, string topLeftText = "")
        {
            if (index >= 0 && index < 8)
            {
                _items[index].BaseTexture = texture;
                _items[index].TopLeftText = topLeftText;
            }
        }

        public void UpdateMagicItems()
        {
            for (var i = 3; i < 8; i++)
            {
                UpdateItem(i);
            }
        }

        public void UpdateGoodsItems()
        {
            for (var i = 0; i < 3; i++)
            {
                UpdateItem(i);
            }
        }

        public void UpdateGoodItem(int listIndex)
        {
            UpdateItem(listIndex - GoodsListManager.BottomIndexBegin);
        }

        public void UpdateItem(int itemIndex)
        {
            if (itemIndex >= 0 && itemIndex < 3)//Goods
            {
                var index = ToGoodsListIndex(itemIndex);
                var info = GoodsListManager.GetItemInfo(index);
                Good good = null;
                var text = "";
                if (info != null)
                {
                    good = info.TheGood;
                    text = info.Count.ToString();
                }
                var icon = good == null ? null : good.Icon;
                SetItem(itemIndex, new Texture(icon), text);
            }

            if (itemIndex >= 3 && itemIndex < 8)//Magic
            {
                var index = ToMagicListIndex(itemIndex);
                var magic = MagicListManager.Get(index);
                var icon = magic == null ? null : magic.Icon;
                SetItem(itemIndex, new Texture(icon));
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsShow) return;

            base.Update(gameTime);
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

            for (var i = 3; i < 8; i++) //Magic
            {
                var item = _items[i];
                var data = (MagicGui.MagicItemData)item.Data;
                var info = MagicListManager.GetItemInfo(data.Index);
                if (info != null && info.RemainColdMilliseconds > 0)
                {
                    if (_coldTimeBackground == null)
                    {
                        _coldTimeBackground = TextureGenerator.GetColorTexture(new Color(0, 0, 0, 180), item.Width,
                            item.Height);
                    }

                    var timeTxt = (info.RemainColdMilliseconds/1000f).ToString("0.0");
                    var font = Globals.FontSize10;

                    spriteBatch.Draw(
                     _coldTimeBackground,
                     item.ScreenPosition,
                     Color.White);

                    spriteBatch.DrawString(font,
                    timeTxt,
                    item.CenterScreenPosition - font.MeasureString(timeTxt) / 2,
                    _colodTimeFontColor);
                }
            }
        }
    }
}