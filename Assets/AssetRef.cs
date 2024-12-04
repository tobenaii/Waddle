namespace Waddle;

public record struct AssetRef(int Id)
{
    public static implicit operator AssetRef(string asset) => new AssetRef(asset.GetHashCode());
    public static implicit operator AssetRef(int asset) => new AssetRef(asset);
}