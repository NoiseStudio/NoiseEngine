using NoiseEngine.Interop.InteropMarshalling;
using NoiseEngine.Rendering;
using System;

namespace NoiseEngine.Interop.Graphics.Vulkan;

internal readonly record struct VulkanDeviceValue(
    InteropString Name,
    GraphicsDeviceVendor UnsafeVendor,
    GraphicsDeviceType Type,
    VulkanVersion ApiVersion,
    uint DriverVersion,
    Guid Guid,
    InteropBool IsSupportsGraphics,
    InteropBool IsSupportsComputing,
    InteropHandle<GraphicsDevice> Handle
) : IDisposable {

    public GraphicsDeviceVendor Vendor {
        get {
            if (Enum.IsDefined(UnsafeVendor))
                return UnsafeVendor;

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
            IsSupportsGraphics,
            IsSupportsComputing,
            Handle
        );
    }

}
