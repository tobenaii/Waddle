using System.Numerics;

namespace Waddle;

public record struct LocalTransform2D(Vector2 Position, float Rotation, Vector2 Scale)
{
    public Vector2 Forward => new(-MathF.Sin(Rotation), MathF.Cos(Rotation));
    public Vector2 Right => new(MathF.Cos(Rotation), MathF.Sin(Rotation));
    
    public LocalTransform2D() : this(Vector2.Zero, 0, Vector2.One)
    {
    }
        
    public static LocalTransform2D FromPosition(Vector2 position)
    {
        return new LocalTransform2D
        {
            Position = position,
        };
    }
        
    public static LocalTransform2D FromRotation(float rotation)
    {
        return new LocalTransform2D
        {
            Rotation = rotation
        };
    }
        
    public static LocalTransform2D FromScale(Vector2 scale)
    {
        return new LocalTransform2D
        {
            Scale = scale
        };
    }
        
    public static LocalTransform2D FromPositionRotation(Vector2 position, float rotation)
    {
        return new LocalTransform2D
        {
            Position = position,
            Rotation = rotation
        };
    }
        
    public static LocalTransform2D FromPositionScale(Vector2 position, Vector2 scale)
    {
        return new LocalTransform2D
        {
            Position = position,
            Scale = scale
        };
    }
        
    public static LocalTransform2D FromRotationScale(float rotation, Vector2 scale)
    {
        return new LocalTransform2D
        {
            Rotation = rotation,
            Scale = scale
        };
    }

    public static LocalTransform2D FromPositionRotationScale(Vector2 position, float rotation, Vector2 scale)
    {
        return new LocalTransform2D
        {
            Position = position,
            Rotation = rotation,
            Scale = scale
        };
    }
}