using System;

namespace NoiseEngine.Nesl;

[Flags]
public enum AttributeTargets : uint {
    Type = 1 << 0,
    GenericTypeParameter = 1 << 1,

    Field = 1 << 2,

    Method = 1 << 3,
    Parameter = 1 << 4,
    ReturnValue = 1 << 5,

    All = Type | GenericTypeParameter | Field | Method | Parameter | ReturnValue
}
