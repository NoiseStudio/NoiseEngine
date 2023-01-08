using NoiseEngine.Interop;
using NoiseEngine.Interop.Rendering;
using NoiseEngine.Mathematics;

namespace NoiseEngine.Rendering;

public abstract class Texture : ICameraRenderTarget {

    public GraphicsDevice Device { get; }
    public TextureFormat Format { get; }

    internal abstract Vector3<uint> Extent { get; }
    internal abstract uint SampleCountInternal { get; }

    internal InteropHandle<Texture> Handle { get; }
    internal InteropHandle<Texture> InnerHandle { get; }

    Vector3<uint> ICameraRenderTarget.Extent => Extent;
    uint ICameraRenderTarget.SampleCount => SampleCountInternal;

    private protected Texture(
        GraphicsDevice device, TextureFormat format, InteropHandle<Texture> handle, InteropHandle<Texture> innerHandle
    ) {
        Device = device;
        Format = format;
        Handle = handle;
        InnerHandle = innerHandle;
    }

    ~Texture() {
        if (Handle == InteropHandle<Texture>.Zero)
            return;

        TextureInterop.Destroy(Handle);
    }

}
