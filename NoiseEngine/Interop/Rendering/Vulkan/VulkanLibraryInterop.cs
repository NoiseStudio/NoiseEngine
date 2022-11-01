using NoiseEngine.Rendering.Vulkan;
using System;

namespace NoiseEngine.Interop.Rendering.Vulkan;

internal static partial class VulkanLibraryInterop {

    [InteropImport("rendering_vulkan_library_interop_create")]
    public static partial InteropResult<InteropHandle<VulkanLibrary>> Create(IntPtr symbol);

    [InteropImport("rendering_vulkan_library_interop_destroy")]
    public static partial void Destroy(InteropHandle<VulkanLibrary> handle);

    [InteropImport("rendering_vulkan_library_interop_get_extension_properties")]
    public static partial InteropResult<InteropArray<VulkanExtensionPropertiesRaw>> GetExtensionProperties(
        InteropHandle<VulkanLibrary> handle
    );

    [InteropImport("rendering_vulkan_library_interop_get_layer_properties")]
    public static partial InteropResult<InteropArray<VulkanLayerPropertiesRaw>> GetLayerProperties(
        InteropHandle<VulkanLibrary> handle
    );

}
