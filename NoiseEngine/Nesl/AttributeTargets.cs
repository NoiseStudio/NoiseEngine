using System;

namespace NoiseEngine.Nesl;

[Flags]
public enum AttributeTargets : uint {
    Class = 1 << 0,
    Struct = 1 << 1,

    Field = 1 << 2,

    Method = 1 << 3,

    Type = Class | Struct,
    All = uint.MaxValue
}
