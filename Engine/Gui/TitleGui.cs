using Engine.Gui.Base;
using Engine.Script;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Texture = Engine.Gui.Base.Texture;

namespace Engine.Gui
{
    public sealed class TitleGui : GuiItem
    {
        private readonly GuiItem _initButton;
        private readonly GuiItem _loadButton;
        private readonly GuiItem _teamButton;
        private readonly GuiItem _exitButton;
        private readonly GuiItem[] _guiItems;
        public TitleGui()
        {
            BaseTexture = new Texture(new Asf(
                Utils.LoadTexture2D(@"ui\title\title")));
            Width = BaseTexture.Width;
            Height = BaseTexture.Height;
            Position = new Vector2(
                (Globals.WindowWidth - Width)/2f,
                (Globals.WindowHeight - Height)/2f);
            var asf = Utils.GetAsf(@"asf\ui\title\", "InitBtn.asf");
            var sound = Utils.GetSoundEffect("界-主菜单.wav");
            _initButton = new GuiItem(this,
                new Vector2(327, 112),
                81,
                66,
                new Texture(asf, 0, 1),
                new Texture(asf, 1, 1),
                null,
                sound);
            asf = Utils.GetAsf(@"asf\ui\title\", "LoadBtn.asf");
            _loadButton = new GuiItem(this,
                new Vector2(327, 177),
                81,
                66,
                new Texture(asf, 0, 1),
                new Texture(asf, 1, 1),
                null,
                sound);
            asf = Utils.GetAsf(@"asf\ui\title\", "TeamBtn.asf");
            _teamButton = new GuiItem(this,
                new Vector2(327, 240),
                81,
                66,
                new Texture(asf, 0, 1),
                new Texture(asf, 1, 1),
                null,
                sound);
            asf = Utils.GetAsf(@"asf\ui\title\", "ExitBtn.asf");
            _exitButton = new GuiItem(this,
                new Vector2(327, 303),
                81,
                66,
                new Texture(asf, 0, 1),
                new Texture(asf, 1, 1),
                null,
                sound);
            _guiItems = new[]
            {
                _initButton,
                _loadButton,
                _teamButton,
                _exitButton
            };
            RegisterEvent();

            IsShow = false;
        }

        private void RegisterEvent()
        {
            _initButton.Click += (arg1, arg2) =>
            {
                IsShow = false;
                ScriptManager.RunScript(Utils.GetScriptParser("NewGame.txt"));
            };
            _loadButton.Click += (arg1, arg2) =>
            {
                IsShow = false;
                GuiManager.ShowLoad();
            };
            _teamButton.Click += (arg1, arg2) =>
            {
                IsShow = false;
                ScriptManager.RunScript(Utils.GetScriptParser("team.txt"));
            };
            _exitButton.Click += (arg1, grg2) => Globals.TheGame.Exit();
        }

        public override void Update(GameTime gameTime)
        {
            if(!IsShow) return;
            base.Update(gameTime);
            foreach (var guiItem in _guiItems)
            {
                guiItem.Update(gameTime);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsShow) return;
            base.Draw(spriteBatch);
            foreach (var guiItem in _guiItems)
            {
                guiItem.Draw(spriteBatch);
            }
        }
    }
}
