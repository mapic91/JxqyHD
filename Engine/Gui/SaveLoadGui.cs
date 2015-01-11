using Engine.Gui.Base;
using Microsoft.Xna.Framework;

namespace Engine.Gui
{
    public sealed class SaveLoadGui : GuiItem
    {
        public SaveLoadGui()
        {
            IsShow = false;
            BaseTexture = new Texture(Utils.GetAsf(@"asf\ui\saveload", "panel.asf"));
            Width = BaseTexture.Width;
            Height = BaseTexture.Height;
            Position = new Vector2(
                (Globals.WindowWidth - Width) / 2f,
                (Globals.WindowHeight - Height) / 2f);

        }
    }
}