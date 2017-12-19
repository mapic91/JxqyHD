using System;
using System.Collections.Generic;
using System.IO;
using Engine.Gui.Base;
using Engine.ListManager;
using IniParser;
using IniParser.Model;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Texture = Engine.Gui.Base.Texture;

namespace Engine.Gui
{
    public sealed class BuyGui: GuiItem
    {
        private ListView _listView;
        private GuiItem _closeButton;
        private readonly Dictionary<int, GoodsListManager.GoodsItemInfo> _goods = new Dictionary<int, GoodsListManager.GoodsItemInfo>();
        private string _fileName;
        private int _goodTypeCount;
        private int _goodTypeCountAtStart;
        private bool _numberValid;
        public bool CanSellSelfGoods = true;

        public BuyGui()
        {
            var cfg = GuiManager.Setttings.Sections["BuySell"];
            var baseTexture = new Texture(Utils.GetAsf(null, cfg["Image"]));
            var position = new Vector2(
                Globals.WindowWidth/2f - baseTexture.Width + int.Parse(cfg["LeftAdjust"]),
                0f + int.Parse(cfg["TopAdjust"]));
            _listView = new ListView(null,
                position,
                new Vector2(int.Parse(cfg["ScrollBarLeft"]), int.Parse(cfg["ScrollBarRight"])),
                baseTexture.Width,
                baseTexture.Height,
                baseTexture,
                27,
                GuiManager.Setttings.Sections["BuySell_List_Items"],
                int.Parse(cfg["ScrollBarWidth"]),
                int.Parse(cfg["ScrollBarHeight"]),
                cfg["ScrollBarButton"]);
            var asf = Utils.GetAsf(null, cfg["CloseImage"]);
            baseTexture = new Texture(asf, 0, 1);
            _closeButton = new GuiItem(_listView,
                new Vector2(int.Parse(cfg["CloseLeft"]), int.Parse(cfg["CloseTop"])),
                baseTexture.Width,
                baseTexture.Height,
                baseTexture,
                null,
                new Texture(asf, 1, 1),
                null,
                Utils.GetSoundEffect(cfg["CloseSound"]));
            _listView.Scrolled += (arg1, arg2) => UpdateItems();
            _listView.RegisterItemMouseStayOverHandler((arg1, arg2) =>
            {
                var item = (DragDropItem)arg1;
                var index = (int) item.Data;
                if (_goods.ContainsKey(index))
                {
                    GuiManager.ToolTipInterface.ShowGood(_goods[index].TheGood);
                }
            });
            _listView.RegisterItemMouseLeaveHandler(
                (arg1, arg2) => GuiManager.ToolTipInterface.IsShow = false);
            _listView.RegisterItemMouseRightClickeHandler((arg1, arg2) =>
            {
                var item = (DragDropItem)arg1;
                var index = (int) item.Data;
                if (_goods.ContainsKey(index))
                {
                    if (_numberValid && _goods[index].Count <= 0)
                    {
                        GuiManager.ShowMessage("该物品已售罄");
                    }
                    else if(_goods[index].TheGood != null)
                    {
                        if (Globals.ThePlayer.BuyGood(_goods[index].TheGood))
                        {
                            if (_numberValid)
                            {
                                _goods[index].Count--;
                                UpdateItems();
                            }
                        }
                    } 
                }
            });
            _closeButton.Click += (arg1, arg2) => GuiManager.EndBuyGoods();

            IsShow = false;
        }

        public void BeginBuy(string listFileName, bool canSellSelfGoods)
        {
            _fileName = listFileName;
            var path = @"save\game\" + listFileName;
            if (!File.Exists(path))
            {
                path = @"ini\buy\" + listFileName;
            }
            CanSellSelfGoods = canSellSelfGoods;
            try
            {
                _goods.Clear();
                var parser = new FileIniDataParser();
                var data =parser.ReadFile(path, Globals.LocalEncoding);
                _goodTypeCountAtStart = _goodTypeCount = int.Parse(data["Header"]["Count"]);
                _numberValid = !string.IsNullOrEmpty(data["Header"]["NumberValid"]) && int.Parse(data["Header"]["NumberValid"]) != 0;
                for (var i = 1; i <= _goodTypeCountAtStart; i++)
                {
                    var count = 0;
                    if (_numberValid)
                    {
                        var number = data[i.ToString()]["Number"];
                        count = string.IsNullOrEmpty(number) ? 0 : int.Parse(number);
                    }
                    _goods[i] = new GoodsListManager.GoodsItemInfo(data[i.ToString()]["IniFile"], count);
                }
                _listView.ScrollToRow(0);
                UpdateItems();
                IsShow = true;
            }
            catch (Exception exception)
            {
                Log.LogFileLoadError("BuySell", path, exception);
            }
        }

        public void EndBuy()
        {
            if (IsShow)
            {
                IsShow = false;
                if (_numberValid)
                {
                    var data = new IniData();
                    data.Sections.AddSection("Header");
                    data["Header"].AddKey("Count", _goodTypeCountAtStart.ToString());
                    data["Header"].AddKey("NumberValid", "1");

                    for (var i = 1; i <= _goodTypeCountAtStart; i++)
                    {
                        data.Sections.AddSection(i.ToString());
                        var s = data[i.ToString()];
                        s.AddKey("IniFile", _goods[i].TheGood.FileName);
                        s.AddKey("Number", _goods[i].Count.ToString());
                    }

                    File.WriteAllText(@"save\game\" + _fileName, data.ToString(), Globals.LocalEncoding);
                }
            }
        }

        public void AddGood(Good good)
        {
            if(good == null)return;

            foreach (var goodItem in _goods)
            {
                if(goodItem.Value == null || goodItem.Value.TheGood == null) continue;
                if (Utils.EqualNoCase(good.FileName, goodItem.Value.TheGood.FileName))
                {
                    //Already exist.
                    if (_numberValid)
                    {
                        goodItem.Value.Count++;
                        UpdateItems();
                        
                    }
                    return;
                }
            }

            _goodTypeCount++;
            _goods[_goodTypeCount] = new GoodsListManager.GoodsItemInfo(good, 1);
            UpdateItems();
        }

        private void UpdateItems()
        {
            for (var i = 0; i < 9; i++)
            {
                Texture texture = null;
                var countStr = "";
                var index = _listView.ToListIndex(i);
                if (_goods.ContainsKey(index) &&
                    _goods[index] != null &&
                    _goods[index].TheGood != null)
                {
                    texture = new Texture(_goods[index].TheGood.Image);
                    countStr = _goods[index].Count.ToString();
                }
                _listView.SetListItem(i, texture, index);
                _listView.SetItemTopLeftText(i, _numberValid ? countStr : "");
            }
        }

        public override void Update(GameTime gameTime)
        {
            if(!IsShow) return;
            _closeButton.Update(gameTime);
            _listView.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsShow) return;
            _listView.Draw(spriteBatch);
            _closeButton.Draw(spriteBatch);
        }
    }
}