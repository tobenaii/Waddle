using System.Numerics;
using System.Runtime.InteropServices;
using MoonWorks.Graphics;
using MoonWorks.Graphics.Font;
using SDL3;
using Texture = MoonWorks.Graphics.Texture;

namespace Waddle;

public static class AssetStore
{
    private static GraphicsDevice _graphicsDevice = null!;
    private static FileSystemWatcher _watcher = null!;
    
    private static readonly Dictionary<AssetRef, Texture> _textures = new();
    private static readonly Dictionary<AssetRef, Font> _fonts = new();
    private static readonly Dictionary<AssetRef, Shader> _shaders = new();
    private static readonly Dictionary<AssetRef, Mesh> _meshes = new();
    private static readonly Dictionary<AssetRef, Atlas> _atlases = new();

    
    private static readonly List<string> _requiresReload = new();
    
    public static void Init(GraphicsDevice graphicsDevice, string contentPath)
    {
        _graphicsDevice = graphicsDevice;
        
        if (Directory.Exists(contentPath + "Textures"))
        {
            var texturePaths = Directory.GetFiles(contentPath + "Textures", "*.png", SearchOption.AllDirectories);
            foreach (var path in texturePaths)
            {
                LoadTexture(path);
            }
        }

        if (Directory.Exists(contentPath + "Models"))
        {
            var modelPaths = Directory.GetFiles(contentPath + "Models", "*.glb", SearchOption.AllDirectories);
            foreach (var path in modelPaths)
            {
                LoadMesh(path);
            }
        }

        if (Directory.Exists(contentPath + "Fonts"))
        {
            var fontPaths = Directory.GetFiles(contentPath + "Fonts", "*.ttf", SearchOption.AllDirectories);
            foreach (var path in fontPaths)
            {
                LoadFont(path);
            }
        }

        if (Directory.Exists(contentPath + "Shaders"))
        {
            var shaderPaths = Directory.GetFiles(contentPath + "Shaders", "*.hlsl", SearchOption.AllDirectories);
            foreach (var path in shaderPaths)
            {
                if (path.Contains("includes")) continue;
                if (!path.Contains(".vert") && !path.Contains(".frag")) continue;
                LoadShader(path);
            }
        }

        if (Directory.Exists(contentPath + "Sprites"))
        {
            var atlasPaths = Directory.GetFiles(contentPath + "Sprites", "*.json", SearchOption.AllDirectories);
            foreach (var path in atlasPaths)
            {
                LoadAtlas(path);
            }
        }

        _watcher = new FileSystemWatcher(Path.GetFullPath(contentPath));
        _watcher.IncludeSubdirectories = true;
        _watcher.NotifyFilter = NotifyFilters.LastWrite;
        _watcher.Changed += (_, args) =>
        {
            if (File.Exists(args.FullPath) && !args.FullPath.EndsWith('~') && !_requiresReload.Contains(args.FullPath))
            {
                _requiresReload.Add(args.FullPath);
            }
        };
        _watcher.EnableRaisingEvents = true;
    }
    
    public static void CheckForReload()
    {
        if (_requiresReload.Count == 0) return;
        foreach (var path in _requiresReload)
        {
            var id = Path.GetFileNameWithoutExtension(path);
            if (_textures.TryGetValue(id, out var texture))
            {
                texture.Dispose();
                LoadTexture(path);
            }
            else if (_meshes.TryGetValue(id, out var mesh))
            {
                mesh.IndexBuffer.Dispose();
                mesh.VertexBuffer.Dispose();
                LoadMesh(path);
            }
            else if (_fonts.TryGetValue(id, out var font))
            {
                font.Dispose();
                LoadFont(path);
            }
            else if (_shaders.TryGetValue(id, out var shader))
            {
                shader.Dispose();
                LoadShader(path);
            }
            else if (_atlases.TryGetValue(id, out var atlas))
            {
                atlas.Texture.Dispose();
                LoadAtlas(path);
            }

            Console.WriteLine($"Reloaded {Path.GetFileName(path)}");
        }

        _requiresReload.Clear();
    }

    public static Shader GetShader(AssetRef shaderId)
    {
        return _shaders[shaderId];
    }

    public static Texture GetTexture(AssetRef textureId)
    {
        return _textures[textureId];
    }

    public static Mesh GetMesh(AssetRef meshId)
    {
        return _meshes[meshId];
    }

    public static Font GetFont(AssetRef fontId)
    {
        return _fonts[fontId];
    }

    public static Atlas GetAtlas(AssetRef atlasId)
    {
        return _atlases[atlasId];
    }

    private static unsafe void LoadTexture(string path)
    {
        var resourceUploader = new ResourceUploader(_graphicsDevice);

        using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read);
        var length = fileStream.Length;
        var buffer = NativeMemory.Alloc((nuint)length);
        var data = new Span<byte>(buffer, (int)length);
        fileStream.ReadExactly(data);
        ImageUtils.ImageInfoFromBytes(data, out var width, out var height, out _);

        var mipLevels = (uint)Math.Floor(Math.Log2(Math.Max(width, height))) + 1;
        var texture = Texture.Create2D(
            _graphicsDevice,
            width,
            height,
            TextureFormat.R8G8B8A8Unorm,
            TextureUsageFlags.Sampler,
            mipLevels
        );

        var region = new TextureRegion
        {
            Texture = texture.Handle,
            W = width,
            H = height,
            D = 1
        };
        resourceUploader.SetTextureDataFromCompressed(region, data);
        resourceUploader.Upload();
        resourceUploader.Dispose();
        NativeMemory.Free(buffer);
        
        var id = Path.GetFileNameWithoutExtension(path);
        _textures[id] = texture;
        var cmdBuf = _graphicsDevice.AcquireCommandBuffer();
        SDL.SDL_GenerateMipmapsForGPUTexture(cmdBuf.Handle, texture.Handle);
        _graphicsDevice.Submit(cmdBuf);
    }

    private static void LoadMesh(string path)
    {
        var meshes = GLTFLoader.Load(path);
        var name = Path.GetFileNameWithoutExtension(path);
        var vertexCount = 0;
        var indexCount = 0;
        foreach (var meshData in meshes)
        {
            vertexCount += meshData.Vertices.Length;
            indexCount += meshData.Indices.Length;
        }
        
        var vertices = new VertexPositionNormalTexture[vertexCount];
        var indices = new uint[indexCount];
        
        var vertexOffset = 0;
        var indexOffset = 0;
        foreach (var meshData in meshes)
        {
            meshData.Vertices.CopyTo(vertices.AsSpan(vertexOffset));
            meshData.Indices.CopyTo(indices.AsSpan(indexOffset));
            vertexOffset += meshData.Vertices.Length;
            indexOffset += meshData.Indices.Length;
        }
        
        var resourceUploader = new ResourceUploader(_graphicsDevice, 1024 * 1024);
        var vertexBuffer = resourceUploader.CreateBuffer(vertices.AsSpan(), BufferUsageFlags.Vertex);
        var indexBuffer = resourceUploader.CreateBuffer(indices.AsSpan(), BufferUsageFlags.Index);
        vertexBuffer.Name = Path.GetFileNameWithoutExtension(path) + " Vertices";
        indexBuffer.Name = Path.GetFileNameWithoutExtension(path) + " Indices";
        resourceUploader.Upload();
        resourceUploader.Dispose();
        _meshes[name] = new Mesh
        {
            VertexBuffer = vertexBuffer,
            IndexBuffer = indexBuffer
        };
    }

    private static void LoadFont(string path)
    {
        var font = Font.Load(_graphicsDevice, path);
        var id = Path.GetFileNameWithoutExtension(path);
        _fonts[id] = font;
    }

    private static void LoadShader(string path)
    {
        var shader = ShaderLoader.LoadShader(path, _graphicsDevice);
        var id = Path.GetFileNameWithoutExtension(path);
        _shaders[id] = shader;
    }

    private static unsafe void LoadAtlas(string path)
    {
        var resourceUploader = new ResourceUploader(_graphicsDevice);
        using var fileStream = new FileStream(
            Path.Combine(Path.GetDirectoryName(path)!, Path.GetFileNameWithoutExtension(path) + ".png"),
            FileMode.Open, FileAccess.Read);
        var length = fileStream.Length;
        var buffer = NativeMemory.Alloc((nuint)length);
        var data = new Span<byte>(buffer, (int)length);
        fileStream.ReadExactly(data);
        ImageUtils.ImageInfoFromBytes(data, out var width, out var height, out _);
        var texture = Texture.Create2D(
            _graphicsDevice,
            width,
            height,
            TextureFormat.R8G8B8A8Unorm,
            TextureUsageFlags.Sampler
        );
        var region = new TextureRegion
        {
            Texture = texture.Handle,
            W = width,
            H = height,
            D = 1
        };
        resourceUploader.SetTextureDataFromCompressed(region, data);
        resourceUploader.Upload();
        resourceUploader.Dispose();
        NativeMemory.Free(buffer);
        
        var atlasData = AtlasLoader.Load(path);
        var images = new Dictionary<AssetRef, Sprite>();
        foreach (var image in atlasData.Images)
        {
            var uvTopLeft = new Vector2(
                (float)image.X / width,
                (float)image.Y / height
            );
            var uvBottomRight = new Vector2(
                (float)(image.X + image.W) / width,
                (float)(image.Y + image.H) / height
            );
            images.Add(Path.GetFileNameWithoutExtension(image.Name), new Sprite(uvTopLeft, uvBottomRight, (uint)image.W, (uint)image.H));
        }
        
        var atlas = new Atlas(texture, images);
        var id = Path.GetFileNameWithoutExtension(path);
        _atlases[id] = atlas;
    }
}