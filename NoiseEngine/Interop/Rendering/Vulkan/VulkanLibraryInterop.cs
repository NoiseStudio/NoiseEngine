using NoiseEngine.Rendering.Vulkan;

namespace NoiseEngine.Interop.Rendering.Vulkan;

internal static partial class VulkanLibraryInterop {

    [InteropImport("rendering_vulkan_library_interop_create")]
    public static partial InteropResult<InteropHandle<VulkanLibrary>> Create();

    [InteropImport("rendering_vulkan_library_interop_destroy")]
    public static partial void Destroy(InteropHandle<VulkanLibrary> handle);

}
