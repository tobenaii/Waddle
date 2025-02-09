using MoonWorks.Graphics;

namespace Waddle;

public class ShaderLoader
{
    public static Shader LoadShader(string filePath, GraphicsDevice graphicsDevice)
    {
        ShaderStage stage;
        if (filePath.EndsWith(".vert.hlsl"))
        {
            stage = ShaderStage.Vertex;
        }
        else if (filePath.EndsWith(".frag.hlsl"))
        {
            stage = ShaderStage.Fragment;
        }
        else
        {
            throw new Exception("Unknown shader type");
        }
        var shader = ShaderCross.Create(
            graphicsDevice,
            filePath,
            "main",
            ShaderCross.ShaderFormat.HLSL,
            stage,
            "Content/Shaders/"
        );
        return shader;
    }
}