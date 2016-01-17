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
            BaseTexture = new Texture(Utils.GetAsf(@"asf\ui\top\", "window.asf"));
            Width = BaseTexture.Width;
            Height = BaseTexture.Height;
            Position = new Vector2((Globals.WindowWidth - BaseTexture.Width) / 2f, 0f);
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
            string[] fileNames =
            {
                "BtnState.asf", //0
                "BtnEquip.asf", //1
                "BtnXiuLian.asf", //2
                "BtnGoods.asf", //3
                "BtnMagic.asf", //4
                "BtnNotes.asf", //5
                "BtnOption.asf" //6
            };
            Vector2[] position =
            {
                new Vector2(52, 0),
                new Vector2(80, 0),
                new Vector2(107, 0),
                new Vector2(135, 0),
                new Vector2(162, 0),
                new Vector2(189, 0),
                new Vector2(216, 0)
            };
            var clickedSound = Utils.GetSoundEffect("界-大按钮.wav");
            for (var i = 0; i < 7; i++)
            {
                var asf = Utils.GetAsf(@"asf\ui\top\", fileNames[i]);
                var baseTexture = new Texture(asf, 0, 1);
                var clickedTexture = new Texture(asf, 1, 1);
                _buttons[i] = new GuiItem(this, 
                position[i],
                baseTexture.Width,
                baseTexture.Height,
                baseTexture,
                null,
                clickedTexture,
                null,
                clickedSound);
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