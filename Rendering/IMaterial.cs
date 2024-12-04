using MoonWorks.Graphics;

namespace Waddle;

public interface IMaterial
{
    public static abstract AssetRef VertexShader { get; }
    public static abstract AssetRef FragmentShader { get; }
    public static abstract void ConfigurePipeline(ref GraphicsPipelineCreateInfo pipelineCreateInfo);
}