using NoiseEngine.Interop.Rendering.Vulkan;
using System;
using System.Runtime.InteropServices;

namespace NoiseEngine.Rendering.Vulkan;

internal readonly record struct VulkanExtensionProperties(string Name) {

    private readonly VulkanVersion specificationVersion;

    public Version SpecificationVersion => specificationVersion.ToVersion();

    internal VulkanExtensionProperties(VulkanExtensionPropertiesRaw raw) : this(raw.Name) {
        specificationVersion = raw.SpecificationVersion;
    }

}
