using System;

namespace NoiseEngine.Nesl;

[Flags]
public enum TypeAttributes {
    Private = 1 << 0,
    Internal = 1 << 1,
    Public = 1 << 2,

    Abstract = 1 << 3,

    Class = 1 << 4,
    Struct = 1 << 5,
    Interface = 1 << 6
}
