using System;

namespace NoiseEngine.Inputs;

[Flags]
public enum KeyModifier : ushort {
    CapsLock = 1 << 0,
    Shift = 1 << 1,
    Alt = 1 << 2,
    Super = 1 << 4,
    LeftShift = 1 << 5,
    LeftControl = 1 << 6,
    LeftAlt = 1 << 7,
    LeftSuper = 1 << 8,
    RightShift = 1 << 9,
    RightControl = 1 << 10,
    RightAlt = 1 << 11,
    RightSuper = 1 << 12
}
