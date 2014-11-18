using System;
using System.Collections.Generic;
using IniParser;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Gui
{
    public sealed class DialogGui : GuiItem
    {
        private TextGui _text;
        private TextGui _selectA;
        private TextGui _selectB;
        private GuiItem _portrait;
        private Dictionary<int, Texture> _portraitList;

        private void LoadPortrait()
        {
            const string path = @"ini\ui\dialog\HeadFile.ini";
            _portraitList = new Dictionary<int, Texture>();
            try
            {
                var parser = new FileIniDataParser();
                var data = parser.ReadFile(path, Globals.SimpleChinaeseEncoding);
                foreach (var item in data["PORTRAIT"])
                {
                    _portraitList[int.Parse(item.KeyName)] = new Texture(Utils.GetAsf(@"asf\portrait\", item.Value));
                }
            }
            catch (Exception exception)
            {
                Log.LogFileLoadError("Portrait", path, exception);
            }
        }

        public DialogGui()
        {
            IsShow = false;
            LoadPortrait();
            BaseTexture = new Texture(Utils.GetAsf(@"asf\ui\dialog\", "panel.asf"));
            Width = BaseTexture.Width;
            Height = BaseTexture.Height;
            Position = new Vector2((Globals.WindowWidth - BaseTexture.Width) / 2f, 
                Globals.WindowHeight - 208f);
            _text = new TextGui(this,
                new Vector2(65, 30),
                310,
                70,
                Globals.FontSize12,
                1,
                2,
                "",
                Color.Black*0.8f);
            _portrait = new GuiItem(this,
                new Vector2(5, -143),
                200,
                160,
                null);
        }

        public void ShowText(string text, int portraitIndex = -1)
        {
            _text.Text = text;
            if (portraitIndex != -1 && _portraitList.ContainsKey(portraitIndex))
            {
                _portrait.BaseTexture = _portraitList[portraitIndex];
            }
            else _portrait.BaseTexture = null;
            IsShow = true;
        }

        public bool NextPage()
        {
            return _text.NextPage();
        }

        public override void Update(GameTime gameTime)
        {
            if(!IsShow) return;
            base.Update(gameTime);
            _text.Update(gameTime);
            _portrait.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsShow) return;
            base.Draw(spriteBatch);
            _portrait.Draw(spriteBatch);
            _text.Draw(spriteBatch);
        }
    }
}