using NoiseEngine.Rendering;

namespace NoiseEngine.Interop.Rendering;

internal static partial class TextureSamplerInterop {

    [InteropImport("rendering_texture_sampler_interop_destroy")]
    public static partial void Destroy(InteropHandle<TextureSampler> imageView);

}
