using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Vulkan;

namespace NoiseEngine.Interop.Rendering.Vulkan;

internal static partial class RenderPassInterop {

    [InteropImport("rendering_vulkan_render_pass_create")]
    public static partial InteropResult<InteropHandle<RenderPass>> Create(
        InteropHandle<GraphicsDevice> device, RenderPassCreateInfo createInfo
    );

    [InteropImport("rendering_vulkan_render_pass_destroy")]
    public static partial void Destroy(InteropHandle<RenderPass> handle);

}
