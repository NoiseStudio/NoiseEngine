using System;

namespace NoiseEngine.Nesl;

[Flags]
public enum NeslModifiers : uint {
    None = 0,
    Static = 1 << 0,
    Uniform = 1 << 1,
    Abstract = 1 << 2
}
