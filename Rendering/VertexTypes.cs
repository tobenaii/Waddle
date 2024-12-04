using System.Runtime.InteropServices;
using MoonWorks.Graphics;
using System.Numerics;

namespace Waddle;

[StructLayout(LayoutKind.Sequential)]
public struct PositionVertex : IVertexType
{
	public Vector3 Position;

	public PositionVertex(Vector3 position)
	{
		Position = position;
	}

	public static VertexElementFormat[] Formats { get; } =
	[
		VertexElementFormat.Float3
	];

	public static uint[] Offsets { get; } = [ 0 ];

	public override string ToString()
	{
		return Position.ToString();
	}
}

[StructLayout(LayoutKind.Sequential)]
public struct PositionColorVertex : IVertexType
{
	public Vector3 Position;
	public Color Color;

	public PositionColorVertex(Vector3 position, Color color)
	{
		Position = position;
		Color = color;
	}

	public static VertexElementFormat[] Formats { get; } =
	[
		VertexElementFormat.Float3,
		VertexElementFormat.Ubyte4Norm
	];

	public static uint[] Offsets { get; } =
	[
		0,
		12
	];

	public override string ToString()
	{
		return Position + " | " + Color;
	}
}

[StructLayout(LayoutKind.Sequential)]
public struct PositionTextureVertex : IVertexType
{
	public Vector3 Position;
	public Vector2 TexCoord;

	public PositionTextureVertex(Vector3 position, Vector2 texCoord)
	{
		Position = position;
		TexCoord = texCoord;
	}

	public static VertexElementFormat[] Formats { get; } = new VertexElementFormat[]
	{
		VertexElementFormat.Float3,
		VertexElementFormat.Float2
	};

	public static uint[] Offsets { get; } =
	[
		0,
		12
	];

	public override string ToString()
	{
		return Position + " | " + TexCoord;
	}
}

[StructLayout(LayoutKind.Explicit, Size = 48)]
public struct PositionTextureColorVertex : IVertexType
{
	[FieldOffset(0)]
	public Vector4 Position;

	[FieldOffset(16)]
	public Vector2 TexCoord;

	[FieldOffset(32)]
	public Vector4 Color;

	public static VertexElementFormat[] Formats { get; } =
	[
		VertexElementFormat.Float4,
		VertexElementFormat.Float2,
		VertexElementFormat.Float4
	];

	public static uint[] Offsets { get; } =
	[
		0,
		16,
		32
	];
}

[StructLayout(LayoutKind.Sequential)]
public struct VertexPositionNormalTexture : IVertexType
{
	public Vector3 Position;
	public Vector3 Normal;
	public Vector2 TexCoord;

	public VertexPositionNormalTexture(Vector3 position, Vector3 normal, Vector2 texCoord)
	{
		Position = position;
		Normal = normal;
		TexCoord = texCoord;
	}

	public static VertexElementFormat[] Formats { get; } =
	[
		VertexElementFormat.Float3,
		VertexElementFormat.Float3,
		VertexElementFormat.Float2,
	];

	public static uint[] Offsets { get; } =
	[
		0,
		12,
		24,
	];

	public override string ToString()
	{
		return Position + " | " + TexCoord + " | " + Normal;
	}
}
