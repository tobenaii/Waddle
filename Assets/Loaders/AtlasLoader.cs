using System.Text.Json;

namespace Waddle;

public static class AtlasLoader
{
    [Serializable]
    public record AtlasData(string Name, int Width, int Height, AtlasImage[] Images);
    
    [Serializable]
    public record AtlasImage(string Name, int X, int Y, int W, int H, int TrimOffsetX, int TrimOffsetY, int UntrimmedWidth, int UntrimmedHeight);
    
    public static AtlasData Load(string path)
    {
        var json = File.ReadAllText(path);
        var atlas = JsonSerializer.Deserialize<AtlasData>(json)!;
        return atlas;
    } 
}