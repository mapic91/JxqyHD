using System.IO;
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

            var cfg = GuiManager.Setttings.Sections["Title"];
            var ImageType = int.Parse(cfg["ImageType"]);
            if (ImageType == 0)
             {
                using (var fs = File.Open(cfg["BackgroundImage"], FileMode.Open))
                {
                    BaseTexture = new Texture(new Asf(
                    Texture2D.FromStream(Globals.TheGame.GraphicsDevice, fs)));
                }

                Width = BaseTexture.Width;
                Height = BaseTexture.Height;
                Position = new Vector2(
                    (Globals.WindowWidth - Width) / 2f,
                    (Globals.WindowHeight - Height) / 2f);
            }
            else
            {
                BaseTexture = new Texture(Utils.GetAsf(null, cfg["Image"]));
                Width = BaseTexture.Width;
                Height = BaseTexture.Height;
                Position = new Vector2(
                    (Globals.WindowWidth - Width) / 2f + int.Parse(cfg["LeftAdjust"]),
                    (Globals.WindowHeight - Height) / 2f + int.Parse(cfg["TopAdjust"]));
            }

            cfg = GuiManager.Setttings.Sections["Title_Btn_Begin"];
            var asf = Utils.GetAsf(null, cfg["Image"]);
            var sound = Utils.GetSoundEffect(cfg["Sound"]);
            _initButton = new GuiItem(this,
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                new Texture(asf, 0, 1),
                new Texture(asf, 1, 1),
                null,
                sound);
            cfg = GuiManager.Setttings.Sections["Title_Btn_Load"];
            asf = Utils.GetAsf(null, cfg["Image"]);
            sound = Utils.GetSoundEffect(cfg["Sound"]);
            _loadButton = new GuiItem(this,
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                new Texture(asf, 0, 1),
                new Texture(asf, 1, 1),
                null,
                sound);
            cfg = GuiManager.Setttings.Sections["Title_Btn_Team"];
            asf = Utils.GetAsf(null, cfg["Image"]);
            sound = Utils.GetSoundEffect(cfg["Sound"]);
            _teamButton = new GuiItem(this,
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
                new Texture(asf, 0, 1),
                new Texture(asf, 1, 1),
                null,
                sound);
            cfg = GuiManager.Setttings.Sections["Title_Btn_Exit"];
            asf = Utils.GetAsf(null, cfg["Image"]);
            sound = Utils.GetSoundEffect(cfg["Sound"]);
            _exitButton = new GuiItem(this,
                new Vector2(int.Parse(cfg["Left"]), int.Parse(cfg["Top"])),
                int.Parse(cfg["Width"]),
                int.Parse(cfg["Height"]),
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
            _exitButton.Click += (arg1, grg2) => Globals.TheGame.ExitGame();
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
