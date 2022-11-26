using NoiseEngine.Rendering.Vulkan;

namespace NoiseEngine.Interop.Rendering.Vulkan;

internal static partial class PipelineInterop {

    [InteropImport("rendering_vulkan_pipeline_destroy")]
    public static partial void Destroy(InteropHandle<Pipeline> handle);

}
