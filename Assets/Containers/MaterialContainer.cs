using MoonWorks.Graphics;

namespace Waddle;

public record MaterialContainer
{
    public required GraphicsPipeline Pipeline { get; set; }
}