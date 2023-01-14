using NoiseEngine.Interop;
using System;

namespace NoiseEngine.Rendering;

internal record struct GraphicsDeviceValue(
    string Name,
    GraphicsDeviceVendor Vendor,
    GraphicsDeviceType Type,
    Version ApiVersion,
    uint DriverVersion,
    Guid Guid,
    bool SupportGraphics,
    bool SupportComputing,
    bool SupportPresentation,
    InteropHandle<GraphicsDevice> Handle
);
