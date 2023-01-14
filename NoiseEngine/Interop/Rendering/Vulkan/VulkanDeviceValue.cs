using NoiseEngine.Interop.InteropMarshalling;
using NoiseEngine.Rendering;
using System;

namespace NoiseEngine.Interop.Rendering.Vulkan;

internal readonly record struct VulkanDeviceValue(
    InteropString Name,
    GraphicsDeviceVendor UnsafeVendor,
    GraphicsDeviceType Type,
    VulkanVersion ApiVersion,
    uint DriverVersion,
    Guid Guid,
    InteropBool SupportsGraphics,
    InteropBool SupportsComputing,
    InteropBool SupportsPresentation,
    InteropHandle<GraphicsDevice> Handle
) : IDisposable {

    public GraphicsDeviceVendor Vendor {
        get {
            if (Enum.IsDefined(UnsafeVendor))
                return UnsafeVendor;

            // Checks repeated Vendors from PCI Vendor Id list.
            return (uint)UnsafeVendor switch {
                5197 => GraphicsDeviceVendor.Samsung,
                6091 => GraphicsDeviceVendor.Qualcomm,
                6629 => GraphicsDeviceVendor.Huawei,
                6880 => GraphicsDeviceVendor.Google,
                _ => GraphicsDeviceVendor.Unknown,
            };
        }
    }

    public void Dispose() {
        Name.Dispose();
    }

    public GraphicsDeviceValue ToGraphics() {
        return new GraphicsDeviceValue(
            Name.ToString(),
            Vendor,
            Type,
            ApiVersion.ToVersion(),
            DriverVersion,
            Guid,
            SupportsGraphics,
            SupportsComputing,
            SupportsPresentation,
            Handle
        );
    }

}
