using System;
using System.IO;
using Engine.Gui.Base;
using Engine.ListManager;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Texture = Engine.Gui.Base.Texture;

namespace Engine.Gui
{
    public sealed class NpcEquipGui : GuiItem
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

        private Character _curCharacter;

        public Character CurCharacter
        {
            get { return _curCharacter; }
        }

        public class NpcEquipItemInfo
        {
            public Good.EquipPosition Pos;

            public NpcEquipItemInfo(Good.EquipPosition pos)
            {
                Pos = pos;
            }
        }

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

        public NpcEquipGui()
        {
            var cfg = GuiManager.Setttings.Sections["NpcEquip"];
            BaseTexture = new Texture(Utils.GetAsf(null, cfg["Image"]));
            Width = BaseTexture.Width;
            Height = BaseTexture.Height;
            Position = new Vector2(
                Globals.WindowWidth / 2f - Width + int.Parse(cfg["LeftAdjust"]),
                0f + int.Parse(cfg["TopAdjust"]));

            cfg = GuiManager.Setttings.Sections["NpcEquip_Head"];
            var hasCount = GoodsListManager.Type != GoodsListManager.ListType.TypeByGoodItem;
            _items[0] = _head = new DragDropItem(this,//Head
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                null,
                new NpcEquipItemInfo(Good.EquipPosition.Head),
                hasCount);

            cfg = GuiManager.Setttings.Sections["NpcEquip_Neck"];
            _items[1] = _neck = new DragDropItem(this, //Neck
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                null,
                new NpcEquipItemInfo(Good.EquipPosition.Neck),
                hasCount);

            cfg = GuiManager.Setttings.Sections["NpcEquip_Body"];
            _items[2] = _body = new DragDropItem(this, //Body
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                null,
                new NpcEquipItemInfo(Good.EquipPosition.Body),
                hasCount);

            cfg = GuiManager.Setttings.Sections["NpcEquip_Back"];
            _items[3] = _back = new DragDropItem(this, //Back
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                null,
                new NpcEquipItemInfo(Good.EquipPosition.Back),
                hasCount);

            cfg = GuiManager.Setttings.Sections["NpcEquip_Hand"];
            _items[4] = _hand = new DragDropItem(this, //Hand
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                null,
                new NpcEquipItemInfo(Good.EquipPosition.Hand),
                hasCount);

            cfg = GuiManager.Setttings.Sections["NpcEquip_Wrist"];
            _items[5] = _wrist = new DragDropItem(this, //Wrist
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                null,
                new NpcEquipItemInfo(Good.EquipPosition.Wrist),
                hasCount);

            cfg = GuiManager.Setttings.Sections["NpcEquip_Foot"];
            _items[6] = _foot = new DragDropItem(this, //Foot
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                null,
                new NpcEquipItemInfo(Good.EquipPosition.Foot),
                hasCount);
            RegisterEventHandler();

            IsShow = false;
        }

        public void SetCharacterEquip(Character character)
        {
            _curCharacter = character;
            if (!string.IsNullOrEmpty(character.BackgroundTextureEquip))
            {
                if (character.BackgroundTextureEquip.EndsWith(".asf"))
                {
                    BaseTexture = new Texture(Utils.GetAsf(null, character.BackgroundTextureEquip));
                }
                else
                {
                    using (var fs = File.Open(character.BackgroundTextureEquip, FileMode.Open))
                    {
                        BaseTexture = new Texture(new Asf(
                            Texture2D.FromStream(Globals.TheGame.GraphicsDevice, fs)));
                    }
                }
            }
            else
            {
                var cfg = GuiManager.Setttings.Sections["NpcEquip"];
                BaseTexture = new Texture(Utils.GetAsf(null, cfg["Image"]));
            }
            Width = BaseTexture.Width;
            Height = BaseTexture.Height;

            UpdateItems();
        }

        private void RegisterEventHandler()
        {
            int index, sourceIndex;
            _head.Drop += (arg1, arg2) =>
            {
                if (CanDrop(arg2, Good.EquipPosition.Head))
                {
                    OnDrop(arg1, arg2);
                }
            };
            _neck.Drop += (arg1, arg2) =>
            {
                if (CanDrop(arg2, Good.EquipPosition.Neck))
                {
                    OnDrop(arg1, arg2);
                }
            };
            _body.Drop += (arg1, arg2) =>
            {
                if (CanDrop(arg2, Good.EquipPosition.Body))
                {
                    OnDrop(arg1, arg2);
                }
            };
            _back.Drop += (arg1, arg2) =>
            {
                if (CanDrop(arg2, Good.EquipPosition.Back))
                {
                    OnDrop(arg1, arg2);
                }
            };
            _hand.Drop += (arg1, arg2) =>
            {
                if (CanDrop(arg2, Good.EquipPosition.Hand))
                {
                    OnDrop(arg1, arg2);
                }
            };
            _wrist.Drop += (arg1, arg2) =>
            {
                if (CanDrop(arg2, Good.EquipPosition.Wrist))
                {
                    OnDrop(arg1, arg2);
                }
            };
            _foot.Drop += (arg1, arg2) =>
            {
                if (CanDrop(arg2, Good.EquipPosition.Foot))
                {
                    OnDrop(arg1, arg2);
                }
            };

            foreach (var item in _items)
            {
                item.RightClick += (arg1, arg2) =>
                {
                    var theItem = (DragDropItem) arg1;
                    var data = theItem.Data as NpcEquipItemInfo;
                    if (data != null)
                    {
                        int newIndex;
                        if (GoodsListManager.NpcUnEquiping(_curCharacter, GetGood(data.Pos), out newIndex))
                        {
                            theItem.BaseTexture = null;
                            theItem.TopLeftText = "";
                            GuiManager.UpdateGoodItemView(newIndex);
                        }
                    }
                };
                item.MouseStayOver += MouseStayOverHandler;
                item.MouseLeave += GoodsGui.MouseLeaveHandler;
            }
        }

        public void OnDrop(object arg1, DragDropItem.DropEvent arg2)
        {
            var item = (DragDropItem)arg1;
            var sourceItem = arg2.Source;
            var data = item.Data as NpcEquipItemInfo;
            var sourceData = sourceItem.Data as GoodsGui.GoodItemData;

            var good = GetGood(data.Pos);
            GoodsListManager.EquipListItemToNpcAndEquiping(_curCharacter, sourceData.Index, good);
            if (GoodsListManager.EquipListItemToNpcAndEquiping(_curCharacter, sourceData.Index, good))
            {
                GuiManager.UpdateGoodsView();
            }

        }

        public void MouseStayOverHandler(object arg1, GuiItem.MouseEvent arg2)
        {
            var item = (DragDropItem)arg1;
            var data = item.Data as NpcEquipItemInfo;
            if (data != null)
            {
                var good = GetGood(data.Pos);
                if (good != null)
                    GuiManager.ToolTipInterface.ShowGood(good, GuiManager.BuyInterface.IsShow);
            }
        }

        public string GetGoodFileName(Good.EquipPosition pos)
        {
            if (_curCharacter == null)
            {
                return null;
            }

            var fileName = "";
            switch (pos)
            {
                case Good.EquipPosition.None:
                    break;
                case Good.EquipPosition.Head:
                    fileName = _curCharacter.HeadEquip;
                    break;
                case Good.EquipPosition.Neck:
                    fileName = _curCharacter.NeckEquip;
                    break;
                case Good.EquipPosition.Body:
                    fileName = _curCharacter.BodyEquip;
                    break;
                case Good.EquipPosition.Back:
                    fileName = _curCharacter.BackEquip;
                    break;
                case Good.EquipPosition.Hand:
                    fileName = _curCharacter.HandEquip;
                    break;
                case Good.EquipPosition.Wrist:
                    fileName = _curCharacter.WristEquip;
                    break;
                case Good.EquipPosition.Foot:
                    fileName = _curCharacter.FootEquip;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(pos), pos, null);
            }

            return fileName;
        }

        public Good GetGood(Good.EquipPosition pos)
        {
            var fileName = GetGoodFileName(pos);
            if (!string.IsNullOrEmpty(fileName))
            {
                var good = Utils.GetGood(fileName);
                return good;
            }

            return null;
        }

        public Texture GetTexture(Good.EquipPosition pos)
        {
            var good = GetGood(pos);
            if (good != null)
            {
                return new Texture(good.Image);
            }

            return null;
        }

        public void UpdateItems()
        {
            foreach (var item in _items)
            {
                var pos = ((NpcEquipItemInfo)item.Data).Pos;
                item.BaseTexture = GetTexture(pos);
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
                    switch (good.Part)
                    {
                        case Good.EquipPosition.Body:
                            item = _body;
                            break;
                        case Good.EquipPosition.Foot:
                            item = _foot;
                            break;
                        case Good.EquipPosition.Head:
                            item = _head;
                            break;
                        case Good.EquipPosition.Neck:
                            item = _neck;
                            break;
                        case Good.EquipPosition.Back:
                            item = _back;
                            break;
                        case Good.EquipPosition.Wrist:
                            item = _wrist;
                            break;
                        case Good.EquipPosition.Hand:
                            item = _hand;
                            break;
                        default:
                            return false;
                    }

                    var nInfo = item.Data as NpcEquipItemInfo;
                    if (GoodsListManager.EquipListItemToNpcAndEquiping(_curCharacter, goodListIndex,
                            GetGood(nInfo.Pos)))
                    {
                        GuiManager.UpdateGoodsView();
                    }
                   
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