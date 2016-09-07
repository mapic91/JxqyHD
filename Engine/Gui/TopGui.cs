using Engine.Gui.Base;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Texture = Engine.Gui.Base.Texture;

namespace Engine.Gui
{
    public class TopGui : GuiItem
    {
        private GuiItem[] _buttons = new GuiItem[7];
        public TopGui()
        {
            var cfg = GuiManager.Setttings.Sections["Top"];
            BaseTexture = new Texture(Utils.GetAsf(null, cfg["Image"]));
            Width = BaseTexture.Width;
            Height = BaseTexture.Height;
            Position = new Vector2((Globals.WindowWidth - BaseTexture.Width) / 2f + int.Parse(cfg["LeftAdjust"]),
                0f + int.Parse(cfg["TopAdjust"]));
            InitializeItems();
        }

        private void RegisterClickHandler()
        {
            _buttons[0].Click += (arg1, arg2) => GuiManager.ToggleStateGuiShow();
            _buttons[1].Click += (arg1, arg2) => GuiManager.ToggleEquipGuiShow();
            _buttons[2].Click += (arg1, arg2) => GuiManager.ToggleXiuLianGuiShow();
            _buttons[3].Click += (arg1, arg2) => GuiManager.ToggleGoodsGuiShow();
            _buttons[4].Click += (arg1, arg2) => GuiManager.ToggleMagicGuiShow();
            _buttons[5].Click += (arg1, arg2) => GuiManager.ToggleMemoGuiShow();
            _buttons[6].Click += (arg1, arg2) => GuiManager.ShowSystem();
        }

        private void InitializeItems()
        {
            string[] sectionNames =
            {
                "Top_State_Btn",
                "Top_Equip_Btn",
                "Top_XiuLian_Btn",
                "Top_Goods_Btn",
                "Top_Magic_Btn",
                "Top_Memo_Btn",
                "Top_System_Btn",
            };
            for (var i = 0; i < 7; i++)
            {
                var cfg = GuiManager.Setttings.Sections[sectionNames[i]];
                var asf = Utils.GetAsf(null, cfg["Image"]);
                var baseTexture = new Texture(asf, 0, 1);
                var clickedTexture = new Texture(asf, 1, 1);
                _buttons[i] = new GuiItem(this,
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                baseTexture,
                null,
                clickedTexture,
                null,
                Utils.GetSoundEffect(cfg["Sound"]));
            }
            RegisterClickHandler();
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            foreach (var button in _buttons)
            {
                button.Update(gameTime);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            foreach (var button in _buttons)
            {
                button.Draw(spriteBatch);
            }
        }
    }
}