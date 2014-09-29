using Microsoft.Xna.Framework;

namespace Engine
{
    public interface IScreen
    {
        Vector2 Position { get; set; }
        int Width { get; }
        int Height { get; }
        Vector2 Size { get; }
        Rectangle Region { get; } 
    }
}