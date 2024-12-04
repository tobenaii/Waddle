using System.Numerics;
using System.Runtime.InteropServices;
using MoonWorks;
using MoonWorks.Graphics;
using MoonWorks.Graphics.Font;
using SDL3;
using Sampler = MoonWorks.Graphics.Sampler;
using Texture = MoonWorks.Graphics.Texture;

namespace Waddle;

public static class AssetStore
{
    public delegate void ConfigurePipeline(ref GraphicsPipelineCreateInfo pipelineCreateInfo);
    private record MaterialConfig(AssetRef Id, AssetRef VertexShader, AssetRef FragmentShader, ConfigurePipeline ConfigurePipeline);
    
    private static GraphicsDevice _graphicsDevice = null!;
    private static Window _window = null!;
    private static FileSystemWatcher _watcher = null!;
    
    private static readonly Dictionary<AssetRef, TextureContainer> _textures = new();
    private static readonly Dictionary<AssetRef, MeshContainer> _meshes = new();
    private static readonly Dictionary<AssetRef, FontContainer> _fonts = new();
    private static readonly Dictionary<AssetRef, ShaderContainer> _shaders = new();
    private static readonly Dictionary<AssetRef, MaterialContainer> _materials = new();
    private static readonly Dictionary<AssetRef, AtlasContainer> _atlases = new();
    private static readonly Dictionary<SamplerType, SamplerContainer> _samplers = new();

    private static readonly List<string> _requiresReload = new();
    private static readonly List<MaterialConfig> _materialConfigs = new();

    public static FontMaterial FontMaterial { get; private set; } = null!;

    public static void Init(GraphicsDevice graphicsDevice, Window window, string contentPath)
    {
        _graphicsDevice = graphicsDevice;
        _window = window;

        FontMaterial = new FontMaterial(window, graphicsDevice);
        
        AddSamplers();

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

    private static void AddSamplers()
    {
        _samplers.Add(SamplerType.LinearClamp, new SamplerContainer
        {
            Sampler = Sampler.Create(_graphicsDevice, SamplerCreateInfo.LinearClamp)
        });
        
        _samplers.Add(SamplerType.LinearWrap, new SamplerContainer
        {
            Sampler = Sampler.Create(_graphicsDevice, SamplerCreateInfo.LinearWrap)
        });
        
        _samplers.Add(SamplerType.AnisotropicClamp, new SamplerContainer
        {
            Sampler = Sampler.Create(_graphicsDevice, SamplerCreateInfo.AnisotropicClamp)
        });
        
        _samplers.Add(SamplerType.AnisotropicWrap, new SamplerContainer
        {
            Sampler = Sampler.Create(_graphicsDevice, SamplerCreateInfo.AnisotropicWrap)
        });
        
        _samplers.Add(SamplerType.PointClamp, new SamplerContainer
        {
            Sampler = Sampler.Create(_graphicsDevice, SamplerCreateInfo.PointClamp)
        });
        
        _samplers.Add(SamplerType.PointWrap, new SamplerContainer
        {
            Sampler = Sampler.Create(_graphicsDevice, SamplerCreateInfo.PointWrap)
        });
    }

    public static void RegisterMaterial<T>() where T : IMaterial
    {
        var materialConfig = new MaterialConfig(typeof(T).Name, T.VertexShader, T.FragmentShader, T.ConfigurePipeline);
        _materialConfigs.Add(materialConfig);
        LoadMaterial(materialConfig);
    }

    public static void CheckForReload()
    {
        if (_requiresReload.Count == 0) return;
        foreach (var path in _requiresReload)
        {
            var id = Path.GetFileNameWithoutExtension(path);
            if (_textures.TryGetValue(id, out var texture))
            {
                texture.Texture.Dispose();
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
                font.Font.Dispose();
                LoadFont(path);
            }
            else if (_shaders.TryGetValue(id, out var shader))
            {
                shader.Shader.Dispose();
                LoadShader(path);
                var materialConfig = _materialConfigs.Find(config => config.VertexShader == id || config.FragmentShader == id);
                if (materialConfig != null)
                {
                    LoadMaterial(materialConfig);
                }
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

    public static ShaderContainer GetShader(AssetRef shaderId)
    {
        return _shaders[shaderId];
    }

    public static TextureContainer GetTexture(AssetRef textureId)
    {
        return _textures[textureId];
    }

    public static MeshContainer GetMesh(AssetRef meshId)
    {
        return _meshes[meshId];
    }

    public static FontContainer GetFont(AssetRef fontId)
    {
        return _fonts[fontId];
    }

    public static AtlasContainer GetAtlas(AssetRef atlasId)
    {
        return _atlases[atlasId];
    }
    
    public static MaterialContainer GetMaterial<T>() where T : IMaterial
    {
        return _materials[typeof(T).Name];
    }

    public static SamplerContainer GetSampler(SamplerType type)
    {
        return _samplers[type];
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
        if (!_textures.TryGetValue(id, out var container))
        {
            _textures.Add(id, new TextureContainer()
            {
                Texture = texture
            });
        }
        else
        {
            container.Texture = texture;
        }
        var cmdBuf = _graphicsDevice.AcquireCommandBuffer();
        SDL.SDL_GenerateMipmapsForGPUTexture(cmdBuf.Handle, texture.Handle);
        _graphicsDevice.Submit(cmdBuf);
    }

    private static void LoadMesh(string path)
    {
        var meshes = GLTFLoader.Load(path);
        foreach (var meshData in meshes)
        {
            var resourceUploader = new ResourceUploader(_graphicsDevice, 1024 * 1024);
            var vertexBuffer = resourceUploader.CreateBuffer(meshData.Vertices.AsSpan(), BufferUsageFlags.Vertex);
            var indexBuffer = resourceUploader.CreateBuffer(meshData.Indices.AsSpan(), BufferUsageFlags.Index);
            vertexBuffer.Name = Path.GetFileNameWithoutExtension(path) + " Vertices";
            indexBuffer.Name = Path.GetFileNameWithoutExtension(path) + " Indices";
            resourceUploader.Upload();
            resourceUploader.Dispose();
            if (!_meshes.TryGetValue(meshData.Name, out var container))
            {
                _meshes.Add(meshData.Name, new MeshContainer()
                {
                    VertexBuffer = vertexBuffer,
                    IndexBuffer = indexBuffer
                });
            }
            else
            {
                container.VertexBuffer = vertexBuffer;
                container.IndexBuffer = indexBuffer; 
            }
        }
    }

    private static void LoadFont(string path)
    {
        var font = Font.Load(_graphicsDevice, path);
        var id = Path.GetFileNameWithoutExtension(path);
        if (!_fonts.TryGetValue(id, out var container))
        {
            _fonts.Add(id, new FontContainer()
            {
                Font = font
            });
        }
        else
        {
            container.Font = font;
        }
    }

    private static void LoadShader(string path)
    {
        var shader = ShaderLoader.LoadShader(path, _graphicsDevice);
        var id = Path.GetFileNameWithoutExtension(path);
        if (!_shaders.TryGetValue(id, out var container))
        {
            _shaders.Add(id, new ShaderContainer()
            {
                Shader = shader
            });
        }
        else
        {
            container.Shader = shader;
        }
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
        if (!_atlases.TryGetValue(id, out var container))
        {
            _atlases.Add(id, new AtlasContainer()
            {
                Atlas = atlas
            });
        }
        else
        {
            container.Atlas = atlas;
        }
    }

    private static void LoadMaterial(MaterialConfig config)
    {
        var vertexShader = _shaders[config.VertexShader];
        var fragmentShader = _shaders[config.FragmentShader];
        var pipelineCreateInfo = GetStandardGraphicsPipelineCreateInfo(
            _window.SwapchainFormat,
            vertexShader.Shader,
            fragmentShader.Shader
        );
        config.ConfigurePipeline(ref pipelineCreateInfo);
        
        var pipeline = GraphicsPipeline.Create(_graphicsDevice, pipelineCreateInfo);
        
        var id = config.Id;
        if (!_materials.TryGetValue(id, out var container))
        {
            _materials.Add(id, new MaterialContainer()
            {
                Pipeline = pipeline
            });
        }
        else
        {
            container.Pipeline = pipeline;
        }
    }

    private static GraphicsPipelineCreateInfo GetStandardGraphicsPipelineCreateInfo(
        TextureFormat swapchainFormat, Shader vertexShader, Shader fragmentShader)
    {
        return new GraphicsPipelineCreateInfo
        {
            TargetInfo = new GraphicsPipelineTargetInfo
            {
                ColorTargetDescriptions = [
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
            VertexShader = vertexShader,
            FragmentShader = fragmentShader
        };
    }
}