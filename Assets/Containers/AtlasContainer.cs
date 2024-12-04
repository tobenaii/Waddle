using MoonWorks.Graphics;

namespace Waddle;

public record AtlasContainer
{
    public required Atlas Atlas { get; set; }
    
    public Texture Texture => Atlas.Texture;
    
    public Sprite GetSprite(AssetRef assetRef)
    {
        return Atlas.Sprites[assetRef];
    }
}