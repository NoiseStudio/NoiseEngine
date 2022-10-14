using NoiseEngine.Interop.InteropMarshalling;
using NoiseEngine.Rendering;
using System;

namespace NoiseEngine.Interop.Graphics.Vulkan;

internal readonly record struct VulkanPhysicalDeviceValue(
    InteropString Name,
    GraphicsPhysicalDeviceVendor UnsafeVendor,
    GraphicsPhysicalDeviceType Type,
    VulkanVersion ApiVersion,
    uint DriverVersion,
    Guid Guid,
    InteropHandle<GraphicsPhysicalDevice> Handle
) : IDisposable {

    public GraphicsPhysicalDeviceVendor Vendor {
        get {
            if (Enum.IsDefined(UnsafeVendor))
                return UnsafeVendor;

            return (uint)UnsafeVendor switch {
                5197 => GraphicsPhysicalDeviceVendor.Samsung,
                6091 => GraphicsPhysicalDeviceVendor.Qualcomm,
                6629 => GraphicsPhysicalDeviceVendor.Huawei,
                6880 => GraphicsPhysicalDeviceVendor.Google,
                _ => GraphicsPhysicalDeviceVendor.Unknown,
            };
        }
    }

    public void Dispose() {
        Name.Dispose();
    }

    public GraphicsPhysicalDeviceValue ToGraphics() {
        return new GraphicsPhysicalDeviceValue(
            Name.ToString(),
            Vendor,
            Type,
            ApiVersion.ToVersion(),
            DriverVersion,
            Guid,
            Handle
        );
    }

}
