using System.Runtime.CompilerServices;

namespace NoiseEngine.Rendering.Vulkan;

internal sealed class VulkanDevice : GraphicsDevice {

    public new VulkanInstance Instance => Unsafe.As<VulkanInstance>(base.Instance);
    public new VulkanPhysicalDevice PhysicalDevice => Unsafe.As<VulkanPhysicalDevice>(base.PhysicalDevice);

    private VulkanDevice(VulkanPhysicalDevice physicalDevice) : base(physicalDevice) {
    }

    public static VulkanDevice Create(VulkanPhysicalDevice physicalDevice) {
        return new VulkanDevice(physicalDevice);
    }

}
