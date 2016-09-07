using Engine.Gui.Base;
using Engine.Script;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Texture = Engine.Gui.Base.Texture;

namespace Engine.Gui
{
    public sealed class SystemGui : GuiItem
    {
        private GuiItem _saveloadButton;
        private GuiItem _opetionButton;
        private GuiItem _exitButton;
        private GuiItem _returnButton;

        public SystemGui()
        {
            var cfg = GuiManager.Setttings.Sections["System"];
            BaseTexture = new Texture(Utils.GetAsf(null, cfg["Image"]));
            Width = BaseTexture.Width;
            Height = BaseTexture.Height;
            Position = new Vector2(
                (Globals.WindowWidth - Width) / 2f + int.Parse(cfg["LeftAdjust"]),
                0 + int.Parse(cfg["TopAdjust"]));

            //Button
            cfg = GuiManager.Setttings.Sections["System_SaveLoad_Btn"];
            var asf = Utils.GetAsf(@"asf\ui\system", "saveload.asf");
            var clickedSound = Utils.GetSoundEffect(cfg["Sound"]);
            _saveloadButton = new GuiItem(this,
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                new Texture(asf, 0, 1),
                null,
                new Texture(asf, 1, 1),
                null,
                clickedSound);

            cfg = GuiManager.Setttings.Sections["System_Option_Btn"];
            asf = Utils.GetAsf(@"asf\ui\system", "option.asf");
            clickedSound = Utils.GetSoundEffect(cfg["Sound"]);
            _opetionButton = new GuiItem(this,
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                new Texture(asf, 0, 1),
                null,
                new Texture(asf, 1, 1),
                null,
                clickedSound);

            cfg = GuiManager.Setttings.Sections["System_Exit_Btn"];
            asf = Utils.GetAsf(@"asf\ui\system", "quit.asf");
            clickedSound = Utils.GetSoundEffect(cfg["Sound"]);
            _exitButton = new GuiItem(this,
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                new Texture(asf, 0, 1),
                null,
                new Texture(asf, 1, 1),
                null,
                clickedSound);

            cfg = GuiManager.Setttings.Sections["System_Return_Btn"];
            asf = Utils.GetAsf(@"asf\ui\system", "return.asf");
            clickedSound = Utils.GetSoundEffect(cfg["Sound"]);
            _returnButton = new GuiItem(this,
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                new Texture(asf, 0, 1),
                null,
                new Texture(asf, 1, 1),
                null,
                clickedSound);

            RegisterEvent();

            IsShow = false;
        }

        private void RegisterEvent()
        {
            _saveloadButton.Click += (arg1, arg2) => GuiManager.ShowSaveLoad();
            _opetionButton.Click += (arg1, arg2) =>
            {
                GuiManager.ShowMessage("请用游戏设置程序进行设置");
            };
            _exitButton.Click += (arg1, arg2) =>
            {
                IsShow = false;
                ScriptManager.RunScript(Utils.GetScriptParser("return.txt"));
            };
            _returnButton.Click += (arg1, arg2) => GuiManager.ShowSystem(false);
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsShow) return;
            base.Update(gameTime);
            _saveloadButton.Update(gameTime);
            _opetionButton.Update(gameTime);
            _exitButton.Update(gameTime);
            _returnButton.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsShow) return;
            base.Draw(spriteBatch);
            _saveloadButton.Draw(spriteBatch);
            _opetionButton.Draw(spriteBatch);
            _exitButton.Draw(spriteBatch);
            _returnButton.Draw(spriteBatch);
        }
    }
}