using NoiseEngine.Interop.Rendering.Vulkan;

namespace NoiseEngine.Rendering.Vulkan;

internal readonly record struct VulkanExtensionProperties(string Name, uint SpecificationVersion) {

    internal VulkanExtensionProperties(VulkanExtensionPropertiesRaw raw) : this(raw.Name, raw.SpecificationVersion) {
    }

}
