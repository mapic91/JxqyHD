using Engine.Gui.Base;
using Engine.ListManager;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Texture = Engine.Gui.Base.Texture;

namespace Engine.Gui
{
    public sealed class EquipGui : GuiItem
    {
        private int _index;
        private DragDropItem[] _items = new DragDropItem[7];
        private DragDropItem _head;
        private DragDropItem _neck;
        private DragDropItem _body;
        private DragDropItem _back;
        private DragDropItem _hand;
        private DragDropItem _wrist;
        private DragDropItem _foot;

        public int Index
        {
            get { return _index; }
            set
            {
                _index = value;
                var fileName = "panel7.asf";
                if (value > 0)
                {
                    fileName = "panel7" + (char)('a' + value) + ".asf";
                }
                BaseTexture = new Texture(Utils.GetAsf(@"asf\ui\common\", fileName));
            }
        }

        public EquipGui()
        {
            var cfg = GuiManager.Setttings.Sections["Equip"];
            BaseTexture = new Texture(Utils.GetAsf(null, cfg["Image"]));
            Width = BaseTexture.Width;
            Height = BaseTexture.Height;
            Position = new Vector2(
                Globals.WindowWidth / 2f - Width + int.Parse(cfg["LeftAdjust"]),
                0f + int.Parse(cfg["TopAdjust"]));

            cfg = GuiManager.Setttings.Sections["Equip_Head"];
            var hasCount = GoodsListManager.Type != GoodsListManager.ListType.TypeByGoodItem;
            _items[0] = _head = new DragDropItem(this,//Head
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                null,
                new GoodsGui.GoodItemData(GoodsListManager.EquipIndexBegin),
                hasCount);

            cfg = GuiManager.Setttings.Sections["Equip_Neck"];
            _items[1] = _neck = new DragDropItem(this, //Neck
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                null,
                new GoodsGui.GoodItemData(GoodsListManager.EquipIndexBegin+1),
                hasCount);

            cfg = GuiManager.Setttings.Sections["Equip_Body"];
            _items[2] = _body = new DragDropItem(this, //Body
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                null,
                new GoodsGui.GoodItemData(GoodsListManager.EquipIndexBegin+2),
                hasCount);

            cfg = GuiManager.Setttings.Sections["Equip_Back"];
            _items[3] = _back = new DragDropItem(this, //Back
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                null,
                new GoodsGui.GoodItemData(GoodsListManager.EquipIndexBegin+3),
                hasCount);

            cfg = GuiManager.Setttings.Sections["Equip_Hand"];
            _items[4] = _hand = new DragDropItem(this, //Hand
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                null,
                new GoodsGui.GoodItemData(GoodsListManager.EquipIndexBegin+4),
                hasCount);

            cfg = GuiManager.Setttings.Sections["Equip_Wrist"];
            _items[5] = _wrist = new DragDropItem(this, //Wrist
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                null,
                new GoodsGui.GoodItemData(GoodsListManager.EquipIndexBegin+5),
                hasCount);

            cfg = GuiManager.Setttings.Sections["Equip_Foot"];
            _items[6] = _foot = new DragDropItem(this, //Foot
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                null,
                new GoodsGui.GoodItemData(GoodsListManager.EquipIndexBegin+6),
                hasCount);
            RegisterEventHandler();

            IsShow = false;
        }

        private void RegisterEventHandler()
        {
            int index, sourceIndex;
            _head.Drop += (arg1, arg2) =>
            {
                if (CanDrop(arg2, Good.EquipPosition.Head))
                {
                    if (GoodsGui.ExchangeItem(arg1, arg2, out index, out sourceIndex))
                    {

                    }
                }
            };
            _neck.Drop += (arg1, arg2) =>
            {
                if (CanDrop(arg2, Good.EquipPosition.Neck))
                {
                    if (GoodsGui.ExchangeItem(arg1, arg2, out index, out sourceIndex))
                    {

                    }
                }
            };
            _body.Drop += (arg1, arg2) =>
            {
                if (CanDrop(arg2, Good.EquipPosition.Body))
                {
                    if (GoodsGui.ExchangeItem(arg1, arg2, out index, out sourceIndex))
                    {

                    }
                }
            };
            _back.Drop += (arg1, arg2) =>
            {
                if (CanDrop(arg2, Good.EquipPosition.Back))
                {
                    if (GoodsGui.ExchangeItem(arg1, arg2, out index, out sourceIndex))
                    {

                    }
                }
            };
            _hand.Drop += (arg1, arg2) =>
            {
                if (CanDrop(arg2, Good.EquipPosition.Hand))
                {
                    if (GoodsGui.ExchangeItem(arg1, arg2, out index, out sourceIndex))
                    {

                    }
                }
            };
            _wrist.Drop += (arg1, arg2) =>
            {
                if (CanDrop(arg2, Good.EquipPosition.Wrist))
                {
                    if (GoodsGui.ExchangeItem(arg1, arg2, out index, out sourceIndex))
                    {

                    }
                }
            };
            _foot.Drop += (arg1, arg2) =>
            {
                if (CanDrop(arg2, Good.EquipPosition.Foot))
                {
                    if (GoodsGui.ExchangeItem(arg1, arg2, out index, out sourceIndex))
                    {

                    }
                }
            };

            foreach (var item in _items)
            {
                item.RightClick += (arg1, arg2) =>
                {
                    var theItem = (DragDropItem) arg1;
                    var data = theItem.Data as GoodsGui.GoodItemData;
                    if (data != null)
                    {
                        int newIndex;
                        if (GoodsListManager.PlayerUnEquiping(data.Index, out newIndex))
                        {
                            theItem.BaseTexture = null;
                            theItem.TopLeftText = "";
                            GuiManager.UpdateGoodItemView(newIndex);
                        }
                    }
                };
                item.MouseStayOver += GoodsGui.MouseStayOverHandler;
                item.MouseLeave += GoodsGui.MouseLeaveHandler;
            }
        }

        public void UpdateItems()
        {
            foreach (var item in _items)
            {
                var index = ((GoodsGui.GoodItemData) item.Data).Index;
                item.BaseTexture = GoodsListManager.GetTexture(index);
                var info = GoodsListManager.GetItemInfo(index);
                if (info != null && info.Count > 0)
                {
                    item.TopLeftText = info.Count.ToString();
                }
                else
                {
                    item.TopLeftText = "";
                }
            }
        }

        private bool CanDrop(DragDropItem.DropEvent arg2, Good.EquipPosition position)
        {
            var sourceItem = arg2.Source;
            var sourceData = sourceItem.Data as GoodsGui.GoodItemData;
            if (sourceData != null)
            {
                return GoodsListManager.CanEquip(sourceData.Index, position);
            }
            return false;
        }

        private static GoodsGui.GoodItemData ToGoodItemData(object data)
        {
            return (GoodsGui.GoodItemData) data;
        }

        public bool EquipGood(int goodListIndex)
        {
            if (GoodsListManager.IsInStoreRange(goodListIndex) ||
                GoodsListManager.IsInBottomGoodsRange(goodListIndex))
            {
                var info = GoodsListManager.GetItemInfo(goodListIndex);
                if (info == null) return false;
                var good = info.TheGood;
                if (good != null &&
                    good.Kind == Good.GoodKind.Equipment)
                {
                    DragDropItem item;
                    var index = 0;
                    switch (good.Part)
                    {
                        case Good.EquipPosition.Body:
                            item = _body;
                            index = ToGoodItemData(item.Data).Index;
                            break;
                        case Good.EquipPosition.Foot:
                            item = _foot;
                            index = ToGoodItemData(item.Data).Index;
                            break;
                        case Good.EquipPosition.Head:
                            item = _head;
                            index = ToGoodItemData(item.Data).Index;
                            break;
                        case Good.EquipPosition.Neck:
                            item = _neck;
                            index = ToGoodItemData(item.Data).Index;
                            break;
                        case Good.EquipPosition.Back:
                            item = _back;
                            index = ToGoodItemData(item.Data).Index;
                            break;
                        case Good.EquipPosition.Wrist:
                            item = _wrist;
                            index = ToGoodItemData(item.Data).Index;
                            break;
                        case Good.EquipPosition.Hand:
                            item = _hand;
                            index = ToGoodItemData(item.Data).Index;
                            break;
                        default:
                            return false;
                    }
                    GoodsListManager.ExchangeListItemAndEquiping(goodListIndex, index);
                    item.BaseTexture = new Texture(good.Image);
                    item.TopLeftText = info.Count.ToString();
                    GuiManager.UpdateGoodItemView(goodListIndex);
                    return true;
                }
            }
            return false;
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsShow) return;
            base.Update(gameTime);
            foreach (var item in _items)
            {
                item.Update(gameTime);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsShow) return;
            base.Draw(spriteBatch);
            foreach (var item in _items)
            {
                item.Draw(spriteBatch);
            }
        }
    }
}