using System;

namespace NoiseEngine.Tests;

[Flags]
internal enum TestRequirements {
    Graphics = 1 << 0,
    Gui = 1 << 1,
    Vulkan = 1 << 2
}
