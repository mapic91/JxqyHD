using System.Collections.Generic;
using Engine.Gui.Base;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Texture = Engine.Gui.Base.Texture;

namespace Engine.Gui
{
    public sealed class LittleHeadGui : GuiItem
    {
        private Dictionary<string, Asf> _headIco = new Dictionary<string, Asf>();
        private List<GuiItem> _heads = new List<GuiItem>();
        private List<TextGui> _lvs = new List<TextGui>();
        private List<Npc> _curPartners = new List<Npc>();
        public LittleHeadGui()
        {
            IsShow = true;
        }

        public override void Update(GameTime gameTime)
        {
            for (var i = 0; i < _curPartners.Count; i++)
            {
                _heads[i].Update(gameTime);
                _lvs[i].Text = "LV" + _curPartners[i].Level;
            }

            var partners = NpcManager.GetAllPartner();
            if (partners.Count == _curPartners.Count)
            {
                bool allSame = true;
                for (var i = 0; i < partners.Count; i++)
                {
                    if (partners[i] != _curPartners[i])
                    {
                        allSame = false;
                        break;
                    }
                }

                if (allSame)
                {
                    return;
                }
            }

            _curPartners = partners;
            const int x = 5;
            var y = 5;
            for(var i = 0; i < _curPartners.Count; i++)
            {
                var curIndex = i;
                var partner = _curPartners[i];
                var name = partner.Name;
                if (string.IsNullOrEmpty(name)) continue;
                if (!_headIco.ContainsKey(name))
                {
                    _headIco[name] = Utils.GetAsf(@"asf\ui\littlehead\", name + ".asf");
                }
                if (_headIco[name] == null) continue;

                GuiItem item;
                TextGui text;
                if (_heads.Count < i + 1)
                {
                    item = new GuiItem();
                    _heads.Add(item);
                    item.MouseLeftDown += (arg1, arg2) =>
                    {
                        if (_curPartners[curIndex].CanEquip > 0)
                        {
                            GuiManager.ShowAllPanels(false);
                            GuiManager.NpcEquipInterface.SetCharacterEquip(_curPartners[curIndex]);
                            GuiManager.NpcEquipInterface.IsShow = true;
                            GuiManager.GoodsInterface.IsShow = true;
                        }
                    };

                    text = new TextGui(item, //Level
                        new Vector2(0, 0),
                        0,
                        0,
                        Globals.FontSize7,
                        0,
                        0,
                        "",
                        Color.White);
                    _lvs.Add(text);
                }
                else
                {
                    item = _heads[i];
                    text = _lvs[i];
                }

                item.BaseTexture = new Texture(_headIco[name]);
                item.Width = item.BaseTexture.Width;
                item.Height = item.BaseTexture.Height;
                item.Position = new Vector2(x, y);

                text.Position = new Vector2(item.Width + 3, item.Height - Globals.FontSize7.MeasureString("LV").Y);

                y += item.Height + 2;
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            for (var i = 0; i < _curPartners.Count; i++)
            {
                _heads[i].Draw(spriteBatch);
                _lvs[i].Draw(spriteBatch);
            }
        }
    }
}