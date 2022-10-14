using NoiseEngine.Rendering.Vulkan;

namespace NoiseEngine.Rendering;

public abstract class GraphicsDevice {

    public GraphicsInstance Instance => PhysicalDevice.Instance;
    public GraphicsPhysicalDevice PhysicalDevice { get; }

    protected GraphicsDevice(GraphicsPhysicalDevice physicalDevice) {
        PhysicalDevice = physicalDevice;
    }

    /// <summary>
    /// Creates new or return existing <see cref="GraphicsDevice"/>.
    /// </summary>
    /// <param name="physicalDevice">
    /// <see cref="GraphicsPhysicalDevice"/> of returned <see cref="GraphicsDevice"/>.
    /// </param>
    /// <returns>New or existing <see cref="GraphicsDevice"/>.</returns>
    public static GraphicsDevice Create(GraphicsPhysicalDevice physicalDevice) {
        return VulkanDevice.Create((VulkanPhysicalDevice)physicalDevice);
    }

}
