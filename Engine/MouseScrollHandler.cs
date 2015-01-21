using Microsoft.Xna.Framework.Input;

namespace Engine
{
    public static class MouseScrollHandler
    {
        private static int _lastScrollValue;

        public static bool IsScrollUp;
        public static bool IsScrollDown;
        

        public static void Update()
        {
            var scrollValue = Mouse.GetState().ScrollWheelValue;
            IsScrollDown = _lastScrollValue > scrollValue;
            IsScrollUp = _lastScrollValue < scrollValue;
            _lastScrollValue = scrollValue;
        }
    }
}