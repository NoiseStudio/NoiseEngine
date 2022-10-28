using NoiseEngine.Interop.Rendering.Vulkan;
using System;

namespace NoiseEngine.Rendering.Vulkan;

internal readonly record struct VulkanLayerProperties(string Name, string Description) {

    private readonly VulkanVersion specificationVersion;
    private readonly VulkanVersion implementationVersion;

    public Version SpecificationVersion => specificationVersion.ToVersion();
    public Version ImplementationVersion => implementationVersion.ToVersion();

    internal VulkanLayerProperties(VulkanLayerPropertiesRaw raw) : this(raw.Name, raw.Description) {
        specificationVersion = raw.SpecificationVersion;
        implementationVersion = raw.ImplementationVersion;
    }

}
