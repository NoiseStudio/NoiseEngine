using System;

namespace NoiseEngine.Nesl;

[Flags]
public enum AttributeTargets : uint {
    Class = 1 << 0,
    Struct = 1 << 1,

    Field = 1 << 2,

    Method = 1 << 3,
    Parameter = 1 << 4,
    ReturnValue = 1 << 5,

    Type = Class | Struct,
    All = uint.MaxValue
}
