using NoiseEngine.Rendering.Vulkan;

namespace NoiseEngine.Interop.Graphics.Vulkan;

internal partial class VulkanLibraryInterop {

    [InteropImport("graphics_vulkan_vulkan_library_interop_create")]
    public static partial InteropResult<InteropHandle<VulkanLibrary>> Create();

    [InteropImport("graphics_vulkan_vulkan_library_interop_destroy")]
    public static partial void Destroy(InteropHandle<VulkanLibrary> handle);

}
