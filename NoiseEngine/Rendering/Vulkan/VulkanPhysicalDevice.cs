using NoiseEngine.Interop.Graphics.Vulkan;
using System.Runtime.CompilerServices;

namespace NoiseEngine.Rendering.Vulkan;

internal sealed class VulkanPhysicalDevice : GraphicsPhysicalDevice {

    public new VulkanInstance Instance => Unsafe.As<VulkanInstance>(base.Instance);

    public VulkanPhysicalDevice(VulkanInstance instance, VulkanPhysicalDeviceValue value) : base(
        instance, value.ToGraphics()
    ) {
        value.Dispose();
    }

    internal override void InternalDispose() {
        VulkanPhysicalDeviceInterop.Destroy(Handle);
    }

}
