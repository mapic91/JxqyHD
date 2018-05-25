namespace Engine
{
    public static class GameState
    {
        public static StateType State;

        public enum StateType
        {
            Start,
            Title,
            Playing,
            EndAds
        }
    }
}