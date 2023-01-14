using System;

namespace NoiseEngine;

[Flags]
public enum WindowControls : uint {
    MinimizeButton = 1 << 0,
    MaximizeButton = 1 << 1,

    All = MinimizeButton | MaximizeButton
}
