using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Vulkan;

namespace NoiseEngine.Interop.Rendering.Vulkan;

internal static partial class SwapchainInterop {

    [InteropImport("rendering_vulkan_swapchain_interop_create")]
    public static partial InteropResult<SwapchainCreateReturnValue> Create(
        InteropHandle<GraphicsDevice> device, InteropHandle<Window> window
    );

    [InteropImport("rendering_vulkan_swapchain_interop_destroy")]
    public static partial void Destroy(InteropHandle<Swapchain> handle);

}
