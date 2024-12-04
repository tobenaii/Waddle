using MoonWorks.Graphics;

namespace Waddle;

public static class GraphicsHelpers
{
    public static GraphicsPipelineCreateInfo GetStandardGraphicsPipelineCreateInfo(
        TextureFormat swapchainFormat,
        Shader vertShader,
        Shader fragShader
    )
    {
        return new GraphicsPipelineCreateInfo
        {
            TargetInfo = new GraphicsPipelineTargetInfo
            {
                ColorTargetDescriptions =
                [
                    new ColorTargetDescription
                    {
                        Format = swapchainFormat,
                        BlendState = ColorTargetBlendState.Opaque
                    }
                ]
            },
            DepthStencilState = DepthStencilState.Disable,
            MultisampleState = MultisampleState.None,
            PrimitiveType = PrimitiveType.TriangleList,
            RasterizerState = RasterizerState.CCW_CullNone,
            VertexInputState = VertexInputState.Empty,
            VertexShader = vertShader,
            FragmentShader = fragShader
        };
    }
}