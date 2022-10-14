using NoiseEngine.Interop;
using NoiseEngine.Rendering.Vulkan;
using System;

namespace NoiseEngine.Rendering;

public abstract class GraphicsPhysicalDevice {

    // TODO: Add property of driver version which returns Version. Different vendors have different implementations.
    // https://registry.khronos.org/vulkan/specs/1.3-extensions/man/html/VkPhysicalDeviceProperties.html
    private readonly uint driverVersion;

    public GraphicsInstance Instance { get; }
    public string Name { get; }
    public GraphicsPhysicalDeviceVendor Vendor { get; }
    public GraphicsPhysicalDeviceType Type { get; }
    public Version ApiVersion { get; }
    public Guid Guid { get; }

    internal InteropHandle<GraphicsPhysicalDevice> Handle { get; }

    private protected GraphicsPhysicalDevice(GraphicsInstance instance, GraphicsPhysicalDeviceValue value) {
        Instance = instance;
        Name = value.Name;
        Vendor = value.Vendor;
        Type = value.Type;
        ApiVersion = value.ApiVersion;
        Guid = value.Guid;
        Handle = value.Handle;

        driverVersion = value.DriverVersion;
    }

    public override string ToString() {
        return $"{GetType().Name} {{ " +
            $"{nameof(Instance)} = {Instance}, " +
            $"{nameof(Name)} = \"{Name}\", " +
            $"{nameof(Vendor)} = {Vendor}, " +
            $"{nameof(Type)} = {Type}, " +
            $"{nameof(Guid)} = {Guid}, " +
            $"{nameof(Handle)} = {Handle} }}";
    }

    internal abstract void InternalDispose();

    internal VulkanPhysicalDevice AsVulkan() {
        return (VulkanPhysicalDevice)this;
    }

}
