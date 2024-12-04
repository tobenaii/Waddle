using MoonWorks.Graphics;

namespace Waddle;

public record Atlas(Texture Texture, Dictionary<AssetRef, Sprite> Sprites);