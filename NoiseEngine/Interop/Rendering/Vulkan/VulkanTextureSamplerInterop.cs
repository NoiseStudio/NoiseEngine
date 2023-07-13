using NoiseEngine.Rendering;

namespace NoiseEngine.Interop.Rendering.Vulkan;

internal static partial class VulkanTextureSamplerInterop {

    [InteropImport("rendering_vulkan_sampler_interop_create")]
    public static partial InteropResult<TextureSamplerCreateReturnValue> Create(
        InteropHandle<GraphicsDevice> device, TextureSamplerCreateInfo createInfo
    );

}
