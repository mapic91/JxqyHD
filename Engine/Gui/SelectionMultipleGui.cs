using System.Collections.Generic;
using System.Linq;
using Engine.Gui.Base;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Engine.Gui
{

    public class SelectionMultipleGui : GuiItem
    {
        private TextGui _messageText;
        private List<TextGui> _selectionLineTexts;
        private Color _normalColor = new Color(0, 255, 0) * 0.8f;
        private Color _selectionColor = Color.Yellow * 0.8f;
        private Texture2D _fadeTexture = TextureGenerator.GetColorTexture(
            Color.Black * 0.8f,
            1,
            1);

        private Texture2D _selectionBg = TextureGenerator.GetColorTexture(Color.Yellow * 0.2f, 1, 1);

        public bool IsInSelecting { get; private set; }
        public List<int> Selection { get; private set; }
        public string VarName { get; private set; }
        public int SelectionCount { get; private set; }

        public SelectionMultipleGui()
        {
            IsShow = false;
        }

        public void Select(int column, int selectionCount, string varName, string message, List<string> selections, List<bool> isShows)
        {
            if (!isShows.Any(s => s))
            {
                return;
            }

            SelectionCount = selectionCount;
            VarName = varName;
            Selection = new List<int>();

            const int lineGap = 5;

            var startY = 60;
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

            startY += _messageText.RealHeight + 10;

            const int xMargin = 50;
            const int itemXGap = 80;
            var itemMaxWidth = (Globals.WindowWidth - 2 * xMargin) / column;

            _selectionLineTexts = new List<TextGui>();
            for (var index = 0; index < selections.Count; index++)
            {
                if (isShows[index])
                {
                    var selection = selections[index];
                    var textGui = new TextGui(null,
                        new Vector2(0, 0),
                        itemMaxWidth,
                        0,
                        Globals.FontSize12,
                        0,
                        0,
                        selection,
                        _normalColor,
                        TextGui.Align.Left);

                    textGui.OverrideColor = _selectionColor;
                    textGui.MouseEnterText += (arg1, arg2) => textGui.UseOverrideColor = true;
                    textGui.MouseLeaveText += (arg1, arg2) => textGui.UseOverrideColor = false;
                    var currentIndex = index;
                    textGui.MouseLeftDownText += (arg1, arg2) =>
                    {
                        if (Selection.Contains(currentIndex))
                        {
                            Selection.Remove(currentIndex);
                        }
                        else
                        {
                            Selection.Add(currentIndex);
                            if (Selection.Count == selectionCount)
                            {
                                IsInSelecting = false;
                                IsShow = false;
                            }
                        }
                        
                    };

                    _selectionLineTexts.Add(textGui);
                }
            }

            var itemMaxRealWidth = 0;
            foreach (var text in _selectionLineTexts)
            {
                if (text.RealWidth > itemMaxRealWidth)
                {
                    itemMaxRealWidth = text.RealWidth;
                }
            }

            var startX = (Globals.WindowWidth - (itemMaxRealWidth * column + (column - 1) * itemXGap)) / 2;

            var itemIndex = 0;
            while (true)
            {
                var maxHeight = 0;
                for (var i = 0; i < column; i++)
                {
                    if (itemIndex + i < _selectionLineTexts.Count)
                    {
                        var item = _selectionLineTexts[itemIndex + i];
                        if (item.RealHeight > maxHeight)
                        {
                            maxHeight = item.RealHeight;
                        }
                    }
                }

                for (var i = 0; i < column; i++)
                {
                    if (itemIndex + i < _selectionLineTexts.Count)
                    {
                        var item = _selectionLineTexts[itemIndex + i];
                        item.Position = new Vector2(startX + i * (itemMaxRealWidth + itemXGap), startY);
                    }
                }

                startY += maxHeight + 10;
                itemIndex += column;

                if (itemIndex >= _selectionLineTexts.Count)
                {
                    break;
                }
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

            foreach (var i in Selection)
            {
                var item = _selectionLineTexts[i];
                spriteBatch.Draw(_selectionBg, new Rectangle((int)item.Position.X, (int)(item.Position.Y), (int)item.RealWidth, (int)item.RealHeight), Color.White);
            }

            _messageText.Draw(spriteBatch);

            foreach (var selectionLineText in _selectionLineTexts)
            {
                selectionLineText.Draw(spriteBatch);
            }
        }
    }

}