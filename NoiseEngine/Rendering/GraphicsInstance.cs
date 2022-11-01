using NoiseEngine.Rendering.Vulkan;
using System.Collections.Generic;

namespace NoiseEngine.Rendering;

public abstract class GraphicsInstance {

    public abstract GraphicsApi Api { get; }

    public IReadOnlyList<GraphicsDevice> Devices => ProtectedDevices;

    protected abstract IReadOnlyList<GraphicsDevice> ProtectedDevices { get; set; }

    /// <summary>
    /// Creates new <see cref="GraphicsInstance"/>.
    /// </summary>
    /// <returns>New <see cref="GraphicsInstance"/>.</returns>
    public static GraphicsInstance Create() {
        VulkanLibrary library = new VulkanLibrary();
        return new VulkanInstance(library, VulkanLogSeverity.All, VulkanLogType.All, library.SupportsValidationLayers);
    }

}
