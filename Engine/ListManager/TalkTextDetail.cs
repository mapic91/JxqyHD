namespace Engine.ListManager
{
    public class TalkTextDetail
    {
        public int Index;
        public int PortraitIndex;
        public string Text;

        public TalkTextDetail(int index, int portraitIndex, string text)
        {
            Index = index;
            PortraitIndex = portraitIndex;
            Text = text;
        }
    }
}