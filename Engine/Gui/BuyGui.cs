using System;
using System.Collections.Generic;
using Engine.Gui.Base;
using IniParser;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Texture = Engine.Gui.Base.Texture;

namespace Engine.Gui
{
    public sealed class BuyGui: GuiItem
    {
        private ListView _listView;
        private GuiItem _closeButton;
        private readonly Dictionary<int, Good> _goods = new Dictionary<int, Good>();

        public BuyGui()
        {
            var baseTexture = new Texture(Utils.GetAsf(@"asf\ui\common\", "panel8.asf"));
            var position = new Vector2(
                Globals.WindowWidth/2f - baseTexture.Width,
                0f);
            _listView = new ListView(null,
                position,
                new Vector2(285, 120),
                baseTexture.Width,
                baseTexture.Height,
                baseTexture,
                3,
                new Vector2(-17, 0));
            var asf = Utils.GetAsf(@"asf\ui\buysell\", "CloseBtn.asf");
            baseTexture = new Texture(asf, 0, 1);
            _closeButton = new GuiItem(_listView,
                new Vector2(117, 354),
                baseTexture.Width,
                baseTexture.Height,
                baseTexture,
                null,
                new Texture(asf, 1, 1),
                null,
                Utils.GetSoundEffect("界-大按钮.wav"));
            _listView.Scrolled += (arg1, arg2) => UpdateItems();
            _listView.RegisterItemMouseStayOverHandler((arg1, arg2) =>
            {
                var item = (DragDropItem)arg1;
                var index = (int) item.Data;
                if (_goods.ContainsKey(index))
                {
                    GuiManager.ToolTipInterface.ShowGood(_goods[index]);
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
                    Globals.ThePlayer.BuyGood(_goods[index]);
                }
            });
            _closeButton.Click += (arg1, arg2) => GuiManager.EndBuyGoods();

            IsShow = false;
        }

        public void BeginBuy(string listFileName)
        {
            var path = @"ini\buy\" + listFileName;
            try
            {
                _goods.Clear();
                var parser = new FileIniDataParser();
                var data =parser.ReadFile(path, Globals.LocalEncoding);
                var count = int.Parse(data["Header"]["Count"]);
                const string basePath = @"ini\goods\";
                for (var i = 1; i <= count; i++)
                {
                    _goods[i] = new Good(basePath + data[i.ToString()]["IniFile"]);
                }
                var rowCount = count % 3 == 0
                ? count / 3
                : (count / 3 + 1);
                _listView.SetMaxRow(rowCount);
                _listView.ScrollToRow(0);
                UpdateItems();
                IsShow = true;
            }
            catch (Exception exception)
            {
                Log.LogFileLoadError("BuySell", path, exception);
            }
        }

        private void UpdateItems()
        {
            for (var i = 0; i < 9; i++)
            {
                Texture texture = null;
                var index = _listView.ToListIndex(i);
                if (_goods.ContainsKey(index) &&
                    _goods[index] != null)
                {
                    texture = new Texture(_goods[index].Image);
                }
                _listView.SetListItem(i, texture, index);
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