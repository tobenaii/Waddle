using System.Numerics;

namespace Waddle;

public record struct LocalTransform3D(Vector3 Position, Quaternion Rotation, Vector3 Scale)
{
    public Vector3 Forward => Vector3.Transform(-Vector3.UnitZ, Rotation);
    public Vector3 Right => Vector3.Transform(Vector3.UnitX, Rotation);
        
    public LocalTransform3D() : this(Vector3.Zero, Quaternion.Identity, Vector3.One)
    {
    }
        
    public static LocalTransform3D FromPosition(Vector3 position)
    {
        return new LocalTransform3D
        {
            Position = position,
        };
    }
        
    public static LocalTransform3D FromRotation(Quaternion rotation)
    {
        return new LocalTransform3D
        {
            Rotation = rotation
        };
    }
        
    public static LocalTransform3D FromScale(Vector3 scale)
    {
        return new LocalTransform3D
        {
            Scale = scale
        };
    }
        
    public static LocalTransform3D FromPositionRotation(Vector3 position, Quaternion rotation)
    {
        return new LocalTransform3D
        {
            Position = position,
            Rotation = rotation
        };
    }
        
    public static LocalTransform3D FromPositionScale(Vector3 position, Vector3 scale)
    {
        return new LocalTransform3D
        {
            Position = position,
            Scale = scale
        };
    }
        
    public static LocalTransform3D FromRotationScale(Quaternion rotation, Vector3 scale)
    {
        return new LocalTransform3D
        {
            Rotation = rotation,
            Scale = scale
        };
    }

    public static LocalTransform3D FromPositionRotationScale(Vector3 position, Quaternion rotation, Vector3 scale)
    {
        return new LocalTransform3D
        {
            Position = position,
            Rotation = rotation,
            Scale = scale
        };
    }
}