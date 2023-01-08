using NoiseEngine.Rendering;

namespace NoiseEngine.Interop.Rendering;

internal static partial class TextureInterop {

    [InteropImport("rendering_texture_interop_destroy")]
    public static partial void Destroy(InteropHandle<Texture> handle);

}
