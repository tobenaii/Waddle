using System.Numerics;
using System.Runtime.InteropServices;
using MoonWorks.Graphics;
using Buffer = MoonWorks.Graphics.Buffer;

namespace Waddle;

public class SpriteBatch
{
    [StructLayout(LayoutKind.Explicit, Size = 64)]
    private struct ComputeSpriteData
    {
        [FieldOffset(0)] public Vector3 Position;

        [FieldOffset(12)] public float Rotation;

        [FieldOffset(16)] public Vector2 Scale;

        [FieldOffset(32)] public Vector4 Color;
        
        [FieldOffset(48)] public Vector2 UVTopLeft;

        [FieldOffset(56)] public Vector2 UVBottomRight;
    }
    
    private readonly TransferBuffer _spriteComputeTransferBuffer;
    private readonly ComputePipeline _computePipeline;
    private readonly Buffer _spriteComputeBuffer;
    private readonly Buffer _spriteVertexBuffer;
    private readonly Buffer _spriteIndexBuffer;
    private readonly ComputeSpriteData[] _spriteData;

    private uint _index;

    public SpriteBatch(uint maxSpriteCount, GraphicsDevice graphicsDevice)
    {
        _spriteData = new ComputeSpriteData[maxSpriteCount];
        
        _computePipeline = ShaderCross.Create(
            graphicsDevice,
            "Content/Shaders/SpriteBatch.comp.hlsl",
            "main",
            ShaderCross.ShaderFormat.HLSL
        );

        _spriteComputeBuffer = Buffer.Create<ComputeSpriteData>(
            graphicsDevice,
            BufferUsageFlags.ComputeStorageRead,
            maxSpriteCount
        );

        _spriteVertexBuffer = Buffer.Create<PositionTextureColorVertex>(
            graphicsDevice,
            BufferUsageFlags.ComputeStorageWrite | BufferUsageFlags.Vertex,
            maxSpriteCount * 4
        );

        _spriteIndexBuffer = Buffer.Create<uint>(
            graphicsDevice,
            BufferUsageFlags.Index,
            maxSpriteCount * 6
        );
        _spriteComputeTransferBuffer = TransferBuffer.Create<ComputeSpriteData>(
            graphicsDevice,
            TransferBufferUsage.Upload,
            maxSpriteCount
        );
        
        var spriteIndexTransferBuffer = TransferBuffer.Create<uint>(
            graphicsDevice,
            TransferBufferUsage.Upload,
            maxSpriteCount * 6
        );

        var indexSpan = spriteIndexTransferBuffer.Map<uint>(false);
        for (int i = 0, j = 0; i < maxSpriteCount * 6; i += 6, j += 4)
        {
            indexSpan[i]     =  (uint) j;
            indexSpan[i + 1] =  (uint) j + 1;
            indexSpan[i + 2] =  (uint) j + 2;
            indexSpan[i + 3] =  (uint) j + 3;
            indexSpan[i + 4] =  (uint) j + 2;
            indexSpan[i + 5] =  (uint) j + 1;
        }
        spriteIndexTransferBuffer.Unmap();

        var cmdBuf = graphicsDevice.AcquireCommandBuffer();
        var copyPass = cmdBuf.BeginCopyPass();
        copyPass.UploadToBuffer(spriteIndexTransferBuffer, _spriteIndexBuffer, false);
        cmdBuf.EndCopyPass(copyPass);
        graphicsDevice.Submit(cmdBuf);
    }

    public void Begin()
    {
        _index = 0;
    }
    
    public void AddSprite(Sprite sprite, Vector3 position, float rotation, Vector2 scale, Vector4 color)
    {
        var data = new ComputeSpriteData
        {
            Position = position,
            Rotation = rotation,
            Scale = sprite.Size * scale,
            Color = color,
            UVTopLeft = sprite.UVTopLeft,
            UVBottomRight = sprite.UVBottomRight
        };
        _spriteData[_index++] = data;
    }

    public void End(CommandBuffer cmdBuf)
    {
        var data = _spriteComputeTransferBuffer.Map<ComputeSpriteData>(true);
        for (var i = 0; i < _index; i++)
        {
            data[i] = _spriteData[i];
        }
        _spriteComputeTransferBuffer.Unmap();
        
        var copyPass = cmdBuf.BeginCopyPass();
        copyPass.UploadToBuffer(_spriteComputeTransferBuffer, _spriteComputeBuffer, true);
        cmdBuf.EndCopyPass(copyPass);
        
        // Set up compute pass to build sprite vertex buffer
        var computePass = cmdBuf.BeginComputePass(
            new StorageBufferReadWriteBinding(_spriteVertexBuffer, true)
        );

        computePass.BindComputePipeline(_computePipeline);
        computePass.BindStorageBuffers(_spriteComputeBuffer);
        var workgroups = (_index + 63) / 64;
        computePass.Dispatch(workgroups, 1, 1);

        cmdBuf.EndComputePass(computePass);
    }

    public void Draw(RenderPass renderPass, Texture texture, Sampler sampler)
    {
        renderPass.BindVertexBuffers(_spriteVertexBuffer);
        renderPass.BindIndexBuffer(_spriteIndexBuffer, IndexElementSize.ThirtyTwo);
        renderPass.BindFragmentSamplers(new TextureSamplerBinding(texture, sampler));
        renderPass.DrawIndexedPrimitives(_index * 6, 1, 0, 0, 0);
    }
}