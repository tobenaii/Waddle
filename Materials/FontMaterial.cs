using MoonWorks;
using MoonWorks.Graphics;

namespace Waddle;

public class FontMaterial
{
    public readonly GraphicsPipeline GraphicsPipeline;

    public FontMaterial(Window window, GraphicsDevice graphicsDevice)
    {
        var fontPipelineCreateInfo = new GraphicsPipelineCreateInfo
        {
            VertexShader = graphicsDevice.TextVertexShader,
            FragmentShader = graphicsDevice.TextFragmentShader,
            VertexInputState = graphicsDevice.TextVertexInputState,
            PrimitiveType = PrimitiveType.TriangleList,
            RasterizerState = RasterizerState.CCW_CullNone,
            MultisampleState = MultisampleState.None,
            DepthStencilState = DepthStencilState.Disable,
            TargetInfo = new GraphicsPipelineTargetInfo
            {
                ColorTargetDescriptions = [
                    new ColorTargetDescription
                    {
                        Format = window.SwapchainFormat,
                        BlendState = ColorTargetBlendState.Additive
                    }
                ]
            }
        };
            
        GraphicsPipeline = GraphicsPipeline.Create(graphicsDevice, fontPipelineCreateInfo);
    }
}