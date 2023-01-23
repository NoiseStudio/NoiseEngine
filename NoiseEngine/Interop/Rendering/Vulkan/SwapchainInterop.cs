using NoiseEngine.Rendering;
using NoiseEngine.Rendering.Vulkan;

namespace NoiseEngine.Interop.Rendering.Vulkan;

internal static partial class SwapchainInterop {

    [InteropImport("rendering_vulkan_swapchain_interop_create")]
    public static partial InteropResult<SwapchainCreateReturnValue> Create(
        InteropHandle<GraphicsDevice> device, InteropHandle<Window> window, uint targetMinImageCount
    );

    [InteropImport("rendering_vulkan_swapchain_interop_destroy")]
    public static partial void Destroy(InteropHandle<Swapchain> handle);

    [InteropImport("rendering_vulkan_swapchain_interop_change_min_image_count")]
    public static partial InteropResult<uint> ChangeMinImageCount(InteropHandle<Swapchain> swapchain, uint targetCount);

}
