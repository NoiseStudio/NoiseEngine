using System;

namespace NoiseEngine.Tests;

[Flags]
internal enum TestRequirements {
    Gpu = 1 << 0,
    Gui = 1 << 1
}
