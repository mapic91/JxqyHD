using C5;

namespace Engine.Gui
{
    public static class GuiManager
    {
        private static LinkedList<GuiItem> _list = new LinkedList<GuiItem>();

        public static void AddGuiItem(GuiItem item)
        {
            if(item == null) return;
            _list.Add(item);
        }
    }
}