﻿using System;

namespace NoiseEngine.Inputs;

[Flags]
public enum KeyModifier : ushort {
    Shift = 1 << 0,
    Control = 1 << 1,
    Alt = 1 << 2,
    Super = 1 << 3,
    CapsLock = 1 << 4
}
