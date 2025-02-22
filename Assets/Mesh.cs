﻿using Buffer = MoonWorks.Graphics.Buffer;

namespace Waddle;

public record Mesh
{
    public required Buffer VertexBuffer { get; set; }
    public required Buffer IndexBuffer { get; set; }
    
    public uint IndexCount => IndexBuffer.Size / sizeof(uint);
}