using NoiseEngine.Interop;
using System;

namespace NoiseEngine.Rendering;

internal record struct GraphicsPhysicalDeviceValue(
    string Name,
    GraphicsPhysicalDeviceVendor Vendor,
    GraphicsPhysicalDeviceType Type,
    Version ApiVersion,
    uint DriverVersion,
    Guid Guid,
    InteropHandle<GraphicsPhysicalDevice> Handle
);
