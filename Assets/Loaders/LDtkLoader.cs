using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Waddle;

public static class LDtkLoader
{
    public static Data Load(string path)
    {
        var json = File.ReadAllText(path);
        var data = JsonSerializer.Deserialize<Data>(json)!;
        return data;
    }
    
    [Serializable]
    public record Data
    {
        [JsonPropertyName("levels")] public required Level[] Levels { get; init; }
    }
    
    public record Level
    {
        [JsonPropertyName("layerInstances")] public required LayerInstance[] LayerInstances { get; init; }
    }
    
    public record LayerInstance
    {
        [JsonPropertyName("gridTiles")] public required GridTile[] GridTiles { get; init; }
        [JsonPropertyName("__tilesetRelPath")] public required string TilesetRelPath { get; init; }
    }
    
    public record GridTile
    {
        [JsonPropertyName("px")] public Vector2 Position { get; init; }
        [JsonPropertyName("src")] public Vector2 Source { get; init; }
    }
}