using System;

namespace NoiseEngine.Nesl;

[Flags]
public enum MethodAttributes {
    Private = 1 << 0,
    Protected = 1 << 1,
    Internal = 1 << 2,
    Public = 1 << 3,

    Static = 1 << 4,
    Abstract = 1 << 5,
    Virtual = 1 << 6
}
