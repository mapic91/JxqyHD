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
            IsShow = false;
            BaseTexture = new Texture(Utils.GetAsf(
                @"asf\ui\common", "panel.asf"));
            Width = BaseTexture.Width;
            Height = BaseTexture.Height;
            Position = new Vector2(
                (Globals.WindowWidth - Width) / 2f,
                26);

            //Button
            var asf = Utils.GetAsf(@"asf\ui\system", "saveload.asf");
            var clickedSound = Utils.GetSoundEffect("界-大按钮.wav");
            _saveloadButton = new GuiItem(this,
                new Vector2(58, 86),
                69, 
                64,
                new Texture(asf, 0, 1),
                null,
                new Texture(asf, 1, 1),
                null,
                clickedSound);
            asf = Utils.GetAsf(@"asf\ui\system", "option.asf");
            _opetionButton = new GuiItem(this,
                new Vector2(58, 150),
                69,
                54,
                new Texture(asf, 0, 1),
                null,
                new Texture(asf, 1, 1),
                null,
                clickedSound);
            asf = Utils.GetAsf(@"asf\ui\system", "quit.asf");
            _exitButton = new GuiItem(this,
                new Vector2(58, 213),
                69,
                54,
                new Texture(asf, 0, 1),
                null,
                new Texture(asf, 1, 1),
                null,
                clickedSound);
            asf = Utils.GetAsf(@"asf\ui\system", "return.asf");
            _returnButton = new GuiItem(this,
                new Vector2(58, 276),
                69,
                54,
                new Texture(asf, 0, 1),
                null,
                new Texture(asf, 1, 1),
                null,
                clickedSound);

            RegisterEvent();
        }

        private void RegisterEvent()
        {
            _saveloadButton.Click += (arg1, arg2) => GuiManager.ShowSaveLoad();
            _opetionButton.Click += (arg1, arg2) =>
            {

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