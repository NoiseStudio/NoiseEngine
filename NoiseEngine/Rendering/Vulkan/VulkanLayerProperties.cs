using NoiseEngine.Interop.Rendering.Vulkan;
using System;

namespace NoiseEngine.Rendering.Vulkan;

internal readonly record struct VulkanLayerProperties(string Name, string Description, uint ImplementationVersion) {

    private readonly VulkanVersion specificationVersion;

    public Version SpecificationVersion => specificationVersion.ToVersion();

    internal VulkanLayerProperties(
        VulkanLayerPropertiesRaw raw
    ) : this(raw.Name, raw.Description, raw.ImplementationVersion) {
        specificationVersion = raw.SpecificationVersion;
    }

}
