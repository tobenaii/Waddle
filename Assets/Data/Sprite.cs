using System.Numerics;

namespace Waddle;

public record Sprite(Vector2 UVTopLeft, Vector2 UVBottomRight, uint Width, uint Height)
{
    public Vector2 Size => new(Width, Height);
}