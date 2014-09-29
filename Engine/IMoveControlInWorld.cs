using Microsoft.Xna.Framework;

namespace Engine
{
    public interface IMoveControlInWorld
    {
        //Move length equal direction.Normalize() * elapsedSeconds * velocity
        void MoveTo(Vector2 direction, float elapsedSeconds);
        Vector2 PositionInWorld { get;set; }
        int Width { get; }
        int Height { get; }
        Vector2 Size { get; }
        Rectangle RegionInWorld { get; }
    }
}