using System;

namespace NoiseEngine.Rendering;

[Flags]
public enum TextureUsage : uint {
    TransferSource = 1 << 0,
    TransferDestination = 1 << 1,
    Sampled = 1 << 2,
    Storage = 1 << 3,
    ColorAttachment = 1 << 4,
    DepthStencilAttachment = 1 << 5,
    TransientAttachment = 1 << 6,
    InputAttachment = 1 << 7,

    TransferAll = TransferSource | TransferDestination
}
