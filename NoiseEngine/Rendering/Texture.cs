using NoiseEngine.Interop;
using NoiseEngine.Interop.Rendering;

namespace NoiseEngine.Rendering;

public abstract class Texture {

    public GraphicsDevice Device { get; }
    public TextureFormat Format { get; }

    internal InteropHandle<Texture> Handle { get; }
    internal InteropHandle<Texture> InnerHandle { get; }

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
