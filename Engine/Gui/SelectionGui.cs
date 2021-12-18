using System.Collections.Generic;
using Engine.Gui.Base;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Gui
{
    public class SelectionGui : GuiItem
    {
        private TextGui _messageText;
        private List<TextGui> _selectionLineTexts;
        private Color _normalColor = new Color(0,255,0)*0.8f;
        private Color _selectionColor = Color.Yellow*0.8f;
        private Texture2D _fadeTexture = TextureGenerator.GetColorTexture(
                    Color.Black * 0.8f,
                    1,
                    1);

        public bool IsInSelecting { get; private set; }
        public int Selection { get; private set; }

        public SelectionGui()
        {
            IsShow = false;
        }

        /// <summary>
        /// Show selections.
        /// </summary>
        /// <param name="selections">String content of selections</param>
        public void Select(string message, List<string> selections)
        {
            const int lineHeight = 30;
            const int lineGap = 5;

            var startY = (Globals.WindowHeight - (selections.Count + 1) * (lineHeight + lineGap)) / 2;

            _messageText = new TextGui(null,
                new Vector2(0, startY),
                Globals.WindowWidth,
                0,
                Globals.FontSize12,
                0,
                lineGap,
                message,
                new Color(255, 215, 0) * 0.8f,
                TextGui.Align.Center);

            startY += _messageText.RealHeight;

            _selectionLineTexts = new List<TextGui>();
            var index = 0;
            
            foreach(var selection in selections)
            {
                var textGui = new TextGui(null,
                new Vector2(0, startY),
                Globals.WindowWidth,
                0,
                Globals.FontSize12,
                0,
                lineGap,
                selection,
                _normalColor,
                TextGui.Align.Center);

                textGui.OverrideColor = _selectionColor;
                textGui.MouseEnterText += (arg1, arg2) => textGui.UseOverrideColor = true;
                textGui.MouseLeaveText += (arg1, arg2) => textGui.UseOverrideColor = false;
                var currentIndex = index;
                textGui.MouseLeftDownText += (arg1, arg2) =>
                {
                    IsInSelecting = false;
                    Selection = currentIndex;
                    IsShow = false;
                };

                _selectionLineTexts.Add(textGui);

                startY += textGui.RealHeight;
                index += 1;
            }

            IsInSelecting = true;
            IsShow = true;
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsShow) return;
            base.Update(gameTime);

            _messageText.Update(gameTime);

            foreach (var selectionLineText in _selectionLineTexts)
            {
                selectionLineText.Update(gameTime);
            }
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            if (!IsShow) return;
            base.Draw(spriteBatch);

            spriteBatch.Draw(_fadeTexture,
                new Rectangle(0,
                    0,
                    Globals.TheGame.GraphicsDevice.PresentationParameters.BackBufferWidth,
                    Globals.TheGame.GraphicsDevice.PresentationParameters.BackBufferHeight),
                Color.White);

            _messageText.Draw(spriteBatch);

            foreach (var selectionLineText in _selectionLineTexts)
            {
                selectionLineText.Draw(spriteBatch);
            }
        }
    }
}